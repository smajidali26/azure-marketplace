using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandingPage.Models
{
    public class EmailConfiguration
    {
        public string sendgridApiKey { get; set; }
        public string fromAddress { get; set; }
        public string toAddress { get; set; }

        public string sendgridTemplateId { get; set; }
        public bool IsDev { get; set; }
    }
}