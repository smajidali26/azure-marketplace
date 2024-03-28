using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Marketplace.SaaS.Models;

namespace LandingPage.Models
{
    public class AzureSubscriptionProvisionModel
    {
        [Display(Name = "Beneficiary email")]
        public string BeneficiaryEmail { get; set; }

        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Display(Name = "Offer ID")]
        public string OfferId { get; set; }

        public bool PendingOperations { get; set; }

        [Display(Name = "Plan Id")]
        public string PlanId { get; set; }

        [Display(Name = "SaaS Subscription Id")]
        public Guid SubscriptionId { get; set; }

        [Display(Name = "Quantity")]
        public int SubscriptionQuantity { get; set; }

        [Display(Name = "Subscription name")]
        public string SubscriptionName { get; set; }
        public SubscriptionStatusEnum SubscriptionStatus { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
    }
}