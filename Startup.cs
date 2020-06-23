namespace AzureADProxy
{
    using System;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;

    public sealed class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            GC.KeepAlive(this.Configuration); // FxCop.

            // 0. Make your all Azure resources this app access is 
            // 1. Browse to AAD Tenant Portal
            // 2. Goto your applications page
            // 2. Create/Get a client secret for debugging, let's say for a day or so.
            // 3. Then set AZURE_CLIENT_SECRET prior to starting the app
            var credentials = new ChainedTokenCredential(new ManagedIdentityCredential(Environment.GetEnvironmentVariable("MSIClientId")), new EnvironmentCredential());

            services.AddSingleton<IHost>(new Host(Environment.GetEnvironmentVariable("ForwardHost"), Environment.GetEnvironmentVariable("ForwardScheme")));

            services.AddHttpClient("ProxyClient").ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false, UseCookies = false });
            services.Configure<KestrelServerOptions>(kso =>
            {
                kso.ConfigureHttpsDefaults(o =>
                {
                    o.ServerCertificate = new X509Certificate2(Convert.FromBase64String(new SecretClient(new Uri(Environment.GetEnvironmentVariable("SSLCertificateSecretUrl")), credentials).GetSecret(Environment.GetEnvironmentVariable("SSLCertificateSecretIdentifier")).Value.Value));
                });
            });

            services.AddSingleton<ProxyService>();
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddJwtBearer(options =>
            {
                options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    "https://login.microsoftonline.com/common/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever());
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudiences = Environment.GetEnvironmentVariable("ValidAudienceList").Split(','),
                    ValidIssuers = Environment.GetEnvironmentVariable("ValidIssuerList").Split(','),
                };
            })
            .AddOpenIdConnect(option =>
            {
                option.ClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
                option.Authority = Environment.GetEnvironmentVariable("OpenIdAuthority");
                option.SignedOutRedirectUri = Environment.GetEnvironmentVariable("OpenIdSignOutUrl");
            });

            services
                .AddDataProtection()
                .PersistKeysToAzureBlobStorage(new Uri(Environment.GetEnvironmentVariable("DataProtectionStorageUrl")), credentials)
                .ProtectKeysWithAzureKeyVault(new Uri(Environment.GetEnvironmentVariable("DataProtectionKeyIdentifier")), credentials);
        }

        public void Configure(IApplicationBuilder app)
        {
            GC.KeepAlive(this.Configuration); // FxCop.

            app.UseAuthentication();
            app.UseMiddleware<ProxyMiddleware>();
        }
    }
}
