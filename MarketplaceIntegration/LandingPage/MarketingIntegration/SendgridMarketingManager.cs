using LandingPage.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandingPage.MarketingIntegration
{
    public class SendgridMarketingManager : IMarketingManager
    {
        private readonly SendGridClient _client;
        private readonly EmailConfiguration _emailConfiguration;
        public SendgridMarketingManager(EmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
            _client = new SendGridClient(_emailConfiguration.sendgridApiKey);
        }
        public async Task ProcessCustomerActivation(AzureSubscriptionProvisionModel marketplaceSubscription)
        {
            if (_emailConfiguration.IsDev)
                return;
            var templateCustomization = new SendGridTemplate(marketplaceSubscription, "true", "New Azure Marketplace Subscription");
            var message = new SendGridMessage();
            message.SetFrom(new EmailAddress(_emailConfiguration.fromAddress, "Marketplace Integration Landing Page"));
            message.AddTo(new EmailAddress(_emailConfiguration.toAddress, "Marketplace Support"));
            message.SetTemplateId(_emailConfiguration.sendgridTemplateId);
            message.SetTemplateData(templateCustomization);
            _ = await _client.SendEmailAsync(message);
        }

        public async Task ProcessCustomerSubscriptionChange(AzureSubscriptionProvisionModel marketplaceSubscription)
        {
            if (_emailConfiguration.IsDev)
                return;
            var templateCustomization = new SendGridTemplate(marketplaceSubscription, "false", "Azure Marketplace Subscription Update");
            var message = new SendGridMessage();
            message.SetFrom(new EmailAddress(_emailConfiguration.fromAddress, "Marketplace Integration Landing Page"));
            message.AddTo(new EmailAddress(_emailConfiguration.toAddress, "Marketplace Support"));
            message.SetTemplateId(_emailConfiguration.sendgridTemplateId);
            message.SetTemplateData(templateCustomization);
            _ = await _client.SendEmailAsync(message);
        }
    }
}
