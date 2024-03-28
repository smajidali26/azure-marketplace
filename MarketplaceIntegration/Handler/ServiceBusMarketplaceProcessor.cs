using System;
using System.Threading.Tasks;
using Handler.MarketingIntegration;
using Handler.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Handler
{
    public class ServiceBusMarketplaceProcessor
    {
        private readonly IMarketplaceClient _marketplaceClient;
        private readonly IMarketingManager _marketingManager;

        public ServiceBusMarketplaceProcessor(IMarketplaceClient marketplaceClient, IMarketingManager marketingManager)
        {
            _marketplaceClient = marketplaceClient;
            _marketingManager = marketingManager;
        }
        [FunctionName("ServiceBusMarketplaceProcessor")]
        public void Run([ServiceBusTrigger("MarketplaceHandler", Connection = "MarketplaceConnectionString")]
            string myQueueItem, ILogger log)
        {
            Guid correlationId = Guid.NewGuid();
            log.LogInformation($"{correlationId.ToString()} - ServiceBusMarketplaceProcessor queue trigger received a new event: {myQueueItem}");

            WebhookMessage payload;

            try
            {
                payload = JsonConvert.DeserializeObject<WebhookMessage>(myQueueItem);
            }
            catch (JsonSerializationException ex)
            {
                log.LogError($"{correlationId.ToString()} - There was an error serializing the received message to a WebhookMessage");
                log.LogError($"{correlationId.ToString()} - {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                log.LogError($"{correlationId.ToString()} - An unexpected error ocurred serializing the received message to a WebhookMessage");
                log.LogError($"{correlationId.ToString()} - {ex.Message}");
                return;
            }

            MarketplaceSubscription retrievedSubscription;
            try
            {
                retrievedSubscription = _marketplaceClient.GetSubscription(payload.SubscriptionId.ToString());
            }
            catch (Exception ex)
            {
                log.LogError($"{correlationId.ToString()} - An unexpected error ocurred retrieving subscription with Id {payload.SubscriptionId} from the Azure Marketplace.");
                log.LogError($"{correlationId.ToString()} - {ex.Message}");
                return;
            }

            if (null == retrievedSubscription)
            {
                log.LogError($"{correlationId.ToString()} - Subscription with Id {payload.SubscriptionId} was not retrieved from the Azure Marketplace.");
                return;
            }

            string email = retrievedSubscription.Beneficiary.EmailId;
            switch (payload.Action)
            {
                case WebhookAction.Reinstate:
                    try
                    {
                        _marketingManager.ProcessCustomerResume(retrievedSubscription);
                        log.LogInformation($"Subscription with ID {retrievedSubscription.SubscriptionId} has been reactivated");
                        break;
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"An unexpected error ocurred reactivating the subscription for user with email {email} and an action of \"Resume\" ");
                        log.LogError($"{correlationId.ToString()} - {ex.Message}");
                        return;
                    }
                case WebhookAction.Suspend:
                    try
                    {
                        _marketingManager.ProcessCustomerSuspension(retrievedSubscription);
                        log.LogInformation($"Subscription with ID {retrievedSubscription.SubscriptionId} has been suspended");
                        break;
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"An unexpected error ocurred suspending the subscription for user with email {email} and an action of \"Suspend\" ");
                        log.LogError($"{correlationId.ToString()} - {ex.Message}");
                        return;
                    }
                case WebhookAction.Unsubscribe:
                    try
                    {
                        _marketingManager.ProcessCustomerCancellation(retrievedSubscription);
                        log.LogInformation($"Subscription with ID {retrievedSubscription.SubscriptionId} has been cancelled");
                        break;
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"An unexpected error ocurred cancelling the subscription for user with email {email} and an action of \"Cancel\" ");
                        log.LogError($"{correlationId.ToString()} - {ex.Message}");
                        return;
                    }
                case WebhookAction.ChangeQuantity:
                    try
                    {
                        log.LogInformation($"Subscription with ID {retrievedSubscription.SubscriptionId} has been updated in the quantity");
                        _marketingManager.ProcessCustomerSubscriptionChange(retrievedSubscription);
                        break;
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, $"{correlationId.ToString()} - An unexpected error ocurred changing the subscription quantity for user with email {email} and an action of \"Change No. of Users\" ");
                        return;
                    }
                case WebhookAction.ChangePlan:
                    try
                    {
                        log.LogInformation($"Subscription with ID {retrievedSubscription.SubscriptionId} has been updated in the plan");
                        _marketingManager.ProcessCustomerSubscriptionChange(retrievedSubscription);
                        break;
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, $"{correlationId.ToString()} - An unexpected error ocurred changing the subscription plan for user with email {email} and an action of \"Change Plan\" ");
                        return;
                    }
                case WebhookAction.Renew:
                    try
                    {
                        log.LogInformation($"Subscription with ID {retrievedSubscription.SubscriptionId} has been renewed");
                        _marketingManager.ProcessCustomerRenewal(retrievedSubscription);
                        break;
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, $"{correlationId.ToString()} - An unexpected error ocurred renewing the subscription for user with email {email} and an action of \"Renew\" ");
                        return;
                    }
                default:
                    break;
            }
            log.LogInformation($"Request {correlationId.ToString()} finished successfully");
        }
    }
}