using System;

namespace Handler.Models
{
    public class SubscriptionOperation
    {
        public string id { get; set; }
        public string activityId { get; set; }
        public string subscriptionId { get; set; }
        public string offerId { get; set; }
        public string publisherId { get; set; }
        public string planId { get; set; }
        public string quantity { get; set; }
        public string action { get; set; }
        public DateTime timeStamp { get; set; }
        public string status { get; set; }
    }
}