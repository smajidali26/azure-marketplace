namespace Handler.Models
{
    public interface IMarketplaceClient
    {
        MarketplaceSubscription GetSubscription(string subscriptionId);
        SubscriptionOperation GetSubscriptionOperation(string subscriptionId, string operationId);
        bool ValidateSubscriptionOperation(string subscriptionId, string operationId);
    }
}