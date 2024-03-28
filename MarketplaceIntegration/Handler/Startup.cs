using Handler.MarketingIntegration;
using Handler.Models;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Handler.Startup))]
namespace Handler
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IMarketplaceClient, MarketplaceClient>();
            builder.Services.AddTransient<IMarketingManager, SendgridMarketingManager>();
            builder.Services.AddHttpClient();
        }
    }
}