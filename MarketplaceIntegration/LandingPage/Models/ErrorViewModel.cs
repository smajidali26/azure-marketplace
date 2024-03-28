using System;

namespace LandingPage.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public DateTime Time { get; } = DateTime.Now;

        public string? ExceptionMessage { get; set; }

        public string? Path { get; set; }
    }
}