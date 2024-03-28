using System;
using System.IO;
using System.Threading.Tasks;
using Handler.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Handler
{
    public class HTTPMarketplaceHandler
    {
        private readonly IMarketplaceClient _client;
        public HTTPMarketplaceHandler(IMarketplaceClient httpClient)
        {
            _client = httpClient;
        }

        [FunctionName("HTTPMarketplaceHandler")]
        [return: ServiceBus("MarketplaceHandler", Connection = "MarketplaceConnectionString")]
        public string Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            Guid correlationId = Guid.NewGuid();
            log.LogInformation($"{correlationId.ToString()} - New request received from the Azure Marketplace");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            log.LogInformation($"{correlationId.ToString()} - Received requestBody: {requestBody}");

            WebhookMessage payload;
            try
            {
                payload = JsonConvert.DeserializeObject<WebhookMessage>(requestBody);
            }
            catch (JsonSerializationException ex)
            {
                log.LogError($"{correlationId.ToString()} - There was an error serializing the received message to a WebhookMessage");
                log.LogError($"{correlationId.ToString()} - {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                log.LogError($"{correlationId.ToString()} - An unexpected error ocurred serializing the received message to a WebhookMessage");
                log.LogError($"{correlationId.ToString()} - {ex.Message}");
                return null;
            }

            if (!_client.ValidateSubscriptionOperation(payload.SubscriptionId.ToString(), payload.OperationId.ToString()))
            {
                log.LogError($"{correlationId.ToString()} - Unvalidated request");
                return null;
            }

            /// Process events that are the actions: Change Quantity or Change Plan (Operation Status: In Progress)
            if (payload.Action == WebhookAction.ChangeQuantity || payload.Action == WebhookAction.ChangePlan)
            {
                log.LogInformation($"{correlationId.ToString()} - Finished processing subscription update successfully");
                return requestBody;
            }

            // We only need to process events that are in Failed, Succeeded or Conflict state since they are the terminal states in which we take action
            if (payload.Status == OperationStatusEnum.InProgress || payload.Status == OperationStatusEnum.NotStarted)
            {
                log.LogInformation($"{correlationId.ToString()} - The operation is {payload.Status.ToString()}. Skipping further processing.");
                return null;
            }



            log.LogInformation($"{correlationId.ToString()} - Finished processing successfully");
            return requestBody;



        }
    }
}