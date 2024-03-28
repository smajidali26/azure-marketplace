namespace Handler.Models
{
    public enum WebhookAction
    {
        // (When the resource has been deleted)
        Unsubscribe,

        // (When the change plan operation has completed)
        ChangePlan,

        // (When the change quantity operation has completed),
        ChangeQuantity,

        //(When resource has been suspended)
        Suspend,

        // (When resource has been reinstated after suspension)
        Reinstate,

        // (When resource has been renewed at the end of the subscription term of a month or a year)
        Renew,

        Transfer
    }
}