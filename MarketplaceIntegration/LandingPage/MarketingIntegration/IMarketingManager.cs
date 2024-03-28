using LandingPage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandingPage.MarketingIntegration
{
    public interface IMarketingManager
    {
        Task ProcessCustomerActivation(AzureSubscriptionProvisionModel marketplaceSubscription);
        Task ProcessCustomerSubscriptionChange(AzureSubscriptionProvisionModel marketplaceSubscription);
    }
}
