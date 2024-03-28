using Azure.Identity;
using LandingPage.MarketingIntegration;
using LandingPage.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Marketplace.SaaS;
using System;

namespace LandingPage
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // NOTE - Dependency Injection for HTTP Client and Fulfillment Client
            services.AddHttpClient();
            services.AddTransient<IMarketingManager, SendgridMarketingManager>();

            var config = new SaaSApiClientConfiguration()
            {
                ClientId = this.Configuration["FulfillmentClient:AzureActiveDirectory:ClientId"],
                ClientSecret = this.Configuration["FulfillmentClient:AzureActiveDirectory:AppKey"],
                TenantId = this.Configuration["FulfillmentClient:AzureActiveDirectory:TenantId"],
                FulFillmentAPIBaseURL = this.Configuration["FulfillmentClient:FulfillmentService:BaseUri"]
            };
            var creds = new ClientSecretCredential(config.TenantId.ToString(), config.ClientId.ToString(), config.ClientSecret);

            if (!Uri.TryCreate(config.FulFillmentAPIBaseURL, UriKind.Absolute, out var fulfillmentBaseApi))
            {
                fulfillmentBaseApi = new Uri("https://marketplaceapi.microsoft.com/api");
            }

            services.AddSingleton<IMarketplaceSaaSClient>(new MarketplaceSaaSClient(fulfillmentBaseApi, creds));
            //services.AddFulfillmentClient(options => this.Configuration.Bind("FulfillmentClient", options));


            // Inject the email configuration
            EmailConfiguration emailConfiguration = new EmailConfiguration();
            Configuration.GetSection("EmailConfiguration").Bind(emailConfiguration);
            services.AddSingleton<EmailConfiguration>(emailConfiguration);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMicrosoftIdentityWebAppAuthentication(Configuration, "AzureAd").EnableTokenAcquisitionToCallDownstreamApi(
                    initialScopes: new string[] { "user.read" })
                .AddInMemoryTokenCaches(); ;
            
            services.AddRazorPages().AddMvcOptions(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                              .RequireAuthenticatedUser()
                              .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();
            services.AddControllersWithViews().AddMicrosoftIdentityUI();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //Note - Added to enforce Authentication
            app.UseAuthentication();


            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=LandingPage}/{action=Index}/{id?}");
            });
        }
    }
}