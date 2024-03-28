namespace Handler.Models
{
    public class MarketplaceAuthRequest
    {
        public string grant_type { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string resource { get; set; }
    }
}