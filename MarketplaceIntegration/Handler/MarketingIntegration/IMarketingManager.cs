using Handler.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Handler.MarketingIntegration
{
    public interface IMarketingManager
    {
        void ProcessCustomerCancellation(MarketplaceSubscription subscription);

        void ProcessCustomerResume(MarketplaceSubscription subscription);

        void ProcessCustomerSuspension(MarketplaceSubscription subscription);

        void ProcessCustomerSubscriptionChange(MarketplaceSubscription subscription);

        void ProcessCustomerRenewal(MarketplaceSubscription subscription);

    }
}
