using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LandingPage.MarketingIntegration;
using LandingPage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Microsoft.Marketplace.SaaS;
using Microsoft.Marketplace.SaaS.Models;
using System.Net;
using Microsoft.Extensions.Primitives;
using Azure.Identity;

namespace LandingPage.Controllers
{
    [Authorize]
    [AuthorizeForScopes(Scopes = new string[] { "user.read" })]
    public class LandingPageController : Controller
    {
        //Client to communicate with the Azure Marketplace API
        private readonly IMarketplaceSaaSClient _fulfillmentClient;
        private readonly IMarketingManager _marketingManager;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly ITokenAcquisition _tokenAcquisition;

        public LandingPageController(IMarketplaceSaaSClient fulfillmentClient, IHttpClientFactory httpClientFactory, IMarketingManager marketingManager, ILogger<LandingPageController> logger, IHttpContextAccessor httpContextAccessor, ITokenAcquisition tokenAcquisition)
        {
            _fulfillmentClient = fulfillmentClient;
            _marketingManager = marketingManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _tokenAcquisition = tokenAcquisition;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(
            AzureSubscriptionProvisionModel provisionModel,
            CancellationToken cancellationToken)
        {
            if (provisionModel.SubscriptionStatus != SubscriptionStatusEnum.Subscribed)
            {
                if (provisionModel.CompanyName == null)
                {
                    this.ModelState.AddModelError(string.Empty, "Please fill in Company Name");
                    return this.View(provisionModel);
                }

                try
                {
                    await this.ProcessLandingPageAsync(provisionModel, cancellationToken);
                    return this.RedirectToAction(nameof(this.Success));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "There was an error processing the Landing Page submission.");
                    this.ModelState.AddModelError(string.Empty, "Cannot process subscription");
                    return this.View();
                }
            }
            else
            {
                return this.RedirectToAction(nameof(this.ActiveSubscription));
            }
        }

        // GET: LandingPage
        public async Task<ActionResult> Index(string token,string access_token, CancellationToken cancellationToken)
        {            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Request received with no token as a query parameter");
                return this.RedirectToAction("NoTokenError");
            }

            AzureSubscriptionProvisionModel provisioningModel;
            try
            {
                provisioningModel = await this.BuildLandingPageModel(token, cancellationToken);

                if (provisioningModel != default)
                {
                    return this.View(provisioningModel);
                }
                else
                {
                    _logger.LogError($"An error ocurred building the landing page for token: {token}");
                    return this.RedirectToAction("Error");
                }
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                _logger.LogInformation(ex, "An error was thrown due to a consent requirement when requesting the Graph API token. This will be handled automatically by the Microsoft Identity framework");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error ocurred while attempting to build the landing page model.");
                throw;
            }
        }

        public ActionResult Success()
        {
            return this.View();
        }

        public ActionResult ActiveSubscription()
        {
            return this.View();
        }

        public ActionResult NoTokenError()
        {
            return this.View();
        }

        public ActionResult Error(string errorMessage)
        {
            ViewBag.Error = errorMessage;
            return this.View();
        }

        private async Task<AzureSubscriptionProvisionModel> BuildLandingPageModel(
           string token,
           CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();
            ResolvedSubscription resolvedSubscription;
            Microsoft.Marketplace.SaaS.Models.Subscription existingSubscription;



            _logger.LogInformation($"Attempting to resolve subscription using Marketplace token: {token}");
            resolvedSubscription = await _fulfillmentClient.Fulfillment.ResolveAsync(
                                           token,
                                           requestId,
                                           correlationId,
                                           cancellationToken);

            if (resolvedSubscription == default(ResolvedSubscription))
            {
                _logger.LogInformation($"The subscription with Marketplace token: {token} was not resolved");
                return default;
            }            
            if (resolvedSubscription.Id == null || resolvedSubscription.Subscription.Id == null)
            {
                _logger.LogError($"The attempt to resolve the Marketplace Subscription using token: {token} was not successful.");
                return default;
            }
            _logger.LogInformation($"Marketplace subscription with token: {token} was resolved");


            existingSubscription = await _fulfillmentClient.Fulfillment.GetSubscriptionAsync(
                                           resolvedSubscription.Subscription.Id.Value,
                                           requestId,
                                           correlationId,
                                           cancellationToken);

            _logger.LogInformation($"Subscription with ID: {resolvedSubscription.Id.Value} was resolved");

            string accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new string[] { "user.read" });

            var defaultAzureCredentail = new DefaultAzureCredential(new DefaultAzureCredentialOptions()
            {
                TenantId= "6c1b0fa6-2690-4fe2-b411-cde26eaac2ca"
            });
            var graphServiceClient = new GraphServiceClient(defaultAzureCredentail);

            //List<Option> options = new List<Option>();
            //options.Add(new QueryOption("$select", "companyName,displayName,userPrincipalName"));
            //User user = await graphServiceClient.GetAsync();
            //_logger.LogInformation($"Graph user {user.UserPrincipalName} resolved using the Graph API");


            //var operations =  _fulfillmentClient.Operations.ListOperations(
            //                                         resolvedSubscription.Subscription.Id.Value,
            //                                         requestId,
            //                                         correlationId,
            //                                         cancellationToken).Value.Operations;

            //var provisioningModel = new AzureSubscriptionProvisionModel
            //{
            //    PlanId = resolvedSubscription.PlanId,
            //    SubscriptionQuantity = resolvedSubscription.Subscription.Quantity.Value,
            //    SubscriptionId = resolvedSubscription.Subscription.Id.Value,
            //    OfferId = resolvedSubscription.OfferId,
            //    SubscriptionName = resolvedSubscription.SubscriptionName,
            //    SubscriptionStatus = existingSubscription.SaasSubscriptionStatus.Value,
            //    PendingOperations = operations.Any(o => o.Status == OperationStatusEnum.InProgress),
            //    CompanyName = user.CompanyName,
            //    FullName = user.DisplayName,
            //    BeneficiaryEmail = existingSubscription.Beneficiary.EmailId
            //};
            return null;

        }

        private async Task ProcessLandingPageAsync(
            AzureSubscriptionProvisionModel provisionModel,
            CancellationToken cancellationToken)
        {
            await _marketingManager.ProcessCustomerActivation(provisionModel);

            // A new subscription will have PendingFulfillmentStart as status
            if (provisionModel.SubscriptionStatus != SubscriptionStatusEnum.Subscribed)
            {
                var result = await _fulfillmentClient.Fulfillment.ActivateSubscriptionAsync(
                                 provisionModel.SubscriptionId,
                                 new SubscriberPlan { PlanId = provisionModel.PlanId },
                                 Guid.Empty,
                                 Guid.Empty,
                                 cancellationToken);


                if (!result.IsError)
                {
                    provisionModel.SubscriptionStatus = SubscriptionStatusEnum.Subscribed;
                    await _marketingManager.ProcessCustomerActivation(provisionModel);
                }
                else
                {
                    // Right now this won't be shown to the customer, just logged. 
                    _logger.LogError($"There was an error activating your subscription in the Azure Marketplace. The error thrown was: {result.ReasonPhrase}");
                }
            }
            else
            {
                // We are not implementing the updates of a subscription                
                return;
            }
        }
    }

}