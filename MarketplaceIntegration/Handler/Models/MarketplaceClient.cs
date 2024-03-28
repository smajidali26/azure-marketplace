using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Handler.Models
{
    public class MarketplaceClient : IMarketplaceClient
    {
        private HttpClient _httpClient; private string apiVersion;
        private string azureTenantId;
        private string clientId;
        private string clientSecret;

        public MarketplaceClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            apiVersion = Environment.GetEnvironmentVariable("apiVersion");
            azureTenantId = Environment.GetEnvironmentVariable("azureTenantId");
            clientId = Environment.GetEnvironmentVariable("clientId");
            clientSecret = Environment.GetEnvironmentVariable("clientSecret");
        }
        public MarketplaceSubscription GetSubscription(string subscriptionId)
        {
            var azureToken = RetrieveAzureAuthenticationToken();
            string getSubscriptionURL =
                $"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}?api-version={apiVersion}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", azureToken);

            var subscriptionResult = _httpClient.GetAsync(getSubscriptionURL);
            MarketplaceSubscription retrievedSubscription =
                JsonConvert.DeserializeObject<MarketplaceSubscription>(subscriptionResult.Result.Content
                    .ReadAsStringAsync().Result);

            return retrievedSubscription;
        }

        public SubscriptionOperation GetSubscriptionOperation(string subscriptionId, string operationId)
        {
            var azureToken = RetrieveAzureAuthenticationToken();
            string getSubscriptionOperationURL =
                $"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}/operations/{operationId}?api-version={apiVersion}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", azureToken);
            var subscriptionOperationResult = _httpClient.GetAsync(getSubscriptionOperationURL);
            SubscriptionOperation retrievedSubscriptionOperation =
                JsonConvert.DeserializeObject<SubscriptionOperation>(subscriptionOperationResult.Result.Content
                    .ReadAsStringAsync().Result);

            return retrievedSubscriptionOperation;
        }

        public bool ValidateSubscriptionOperation(string subscriptionId, string operationId)
        {
            var azureToken = RetrieveAzureAuthenticationToken();
            string getSubscriptionOperationURL =
                $"https://marketplaceapi.microsoft.com/api/saas/subscriptions/{subscriptionId}/operations/{operationId}?api-version={apiVersion}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", azureToken);
            var subscriptionOperationResult = _httpClient.GetAsync(getSubscriptionOperationURL);


            return subscriptionOperationResult.Result.IsSuccessStatusCode;
        }

        private string RetrieveAzureAuthenticationToken()
        {
            var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", "client_credentials" },
                {"resource","20e940b3-4c77-4b0b-9a53-9e16a1b010a7" }
            });

            var authResponse = _httpClient.PostAsync($"https://login.microsoftonline.com/{azureTenantId}/oauth2/token", requestContent);

            string azureToken = JsonConvert
                .DeserializeObject<MarketplaceAuthenticationResponse>(authResponse.Result.Content.ReadAsStringAsync()
                    .Result).access_token;

            return azureToken;

        }
    }
}