using LandingPage.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandingPage.MarketingIntegration
{
    public class SendGridTemplate
    {
        [JsonProperty]
        public string name { get; set; }

        [JsonProperty]
        public string email { get; set; }

        [JsonProperty]
        public string companyName { get; set; }
        
        [JsonProperty]
        public string quantity { get; set; }

        [JsonProperty]
        public string azureSubscriptionId { get; set; }

        [JsonProperty]
        public string subscriptionState { get; set; }

        [JsonProperty]
        public string isNew { get; set; }

        [JsonProperty]
        public string subject { get; set; }

        [JsonProperty]
        public string planId { get; set; }

        public SendGridTemplate(AzureSubscriptionProvisionModel model, string isNewSubscription, string emailSubject)
        {
            name = model.FullName;
            email = model.BeneficiaryEmail;
            companyName = model.CompanyName;
            if (model.SubscriptionQuantity != 0)
            {
                quantity = model.SubscriptionQuantity.ToString();
            }
            else
            {
                quantity = "Not Applicable";
            }
            azureSubscriptionId = model.SubscriptionId.ToString();
            subscriptionState = model.SubscriptionStatus.ToString();
            isNew = isNewSubscription;
            subject = emailSubject;
            planId = model.PlanId;

        }
    }
}
