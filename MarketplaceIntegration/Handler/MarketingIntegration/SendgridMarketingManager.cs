using Handler.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Handler.MarketingIntegration
{
    public class SendgridMarketingManager : IMarketingManager
    {
        private readonly SendGridClient _client;
        private readonly string fromAddress;
        private readonly string toAddress;
        private readonly string sendgridTemplateId;

        public SendgridMarketingManager()
        {
            _client = new SendGridClient(Environment.GetEnvironmentVariable("sendgridApiKey"));
            fromAddress = Environment.GetEnvironmentVariable("fromAddress");
            toAddress = Environment.GetEnvironmentVariable("toAddress");
            sendgridTemplateId = Environment.GetEnvironmentVariable("sendgridTemplateId");
        }

        public async void ProcessCustomerCancellation(MarketplaceSubscription subscription)
        {
            await SendNotification("Azure Marketplace Subscription Cancelled", subscription);
        }

        public async void ProcessCustomerResume(MarketplaceSubscription subscription)
        {
            await SendNotification("Azure Marketplace Subscription Resumed", subscription);
        }

        public async void ProcessCustomerSuspension(MarketplaceSubscription subscription)
        {
            await SendNotification("Azure Marketplace Subscription Suspended", subscription);
        }

        public async void ProcessCustomerSubscriptionChange(MarketplaceSubscription subscription)
        {
            await SendNotification("Azure Marketplace Subscription Update", subscription);
        }

        public async void ProcessCustomerRenewal(MarketplaceSubscription subscription)
        {
            await SendNotification("Azure Marketplace Subscription Renewal", subscription);
        }

        private async Task SendNotification(string subject, MarketplaceSubscription marketplaceSubscription)
        {
            var templateCustomization = new SendGridTemplate(marketplaceSubscription, subject);
            var message = new SendGridMessage();            
            message.SetFrom(new EmailAddress(fromAddress, "Marketplace Integration Landing Page"));
            message.AddTo(new EmailAddress(toAddress, "Marketplace Support"));
            message.SetTemplateId(sendgridTemplateId);
            message.SetTemplateData(templateCustomization);
            _ = await _client.SendEmailAsync(message);
        }
    }
}
