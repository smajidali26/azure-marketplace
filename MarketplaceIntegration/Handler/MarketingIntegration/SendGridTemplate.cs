using Handler.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Handler.MarketingIntegration
{
    public class SendGridTemplate
    {

        [JsonProperty]
        public string email { get; set; }

        [JsonProperty]
        public string azureSubscriptionId { get; set; }

        [JsonProperty]
        public string subscriptionState { get; set; }

        [JsonProperty]
        public string subject { get; set; }

        [JsonProperty]
        public string planId { get; set; }
        public SendGridTemplate(MarketplaceSubscription subscription, string emailSubject)
        {
            email = subscription.Beneficiary.EmailId;
            azureSubscriptionId = subscription.SubscriptionId.ToString();
            subscriptionState = subscription.SaasSubscriptionStatus.ToString();
            subject = emailSubject;
            planId = subscription.PlanId;
        }
    }
}
