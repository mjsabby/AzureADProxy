namespace AzureADProxy
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;

    public sealed class ProxyMiddleware
    {
        private static readonly int LoadBalancerPort = int.Parse(Environment.GetEnvironmentVariable("LBPORT"), CultureInfo.InvariantCulture);

        private static readonly string HttpsHostName = Environment.GetEnvironmentVariable("HTTPS_HOST_NAME");

        private static readonly byte[] OK = { (byte)'O', (byte)'K' };

        private readonly RequestDelegate next;

        private readonly IHost host;

        public ProxyMiddleware(RequestDelegate next, IHost host)
        {
            this.next = next;
            this.host = host;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = context.Response;

            if (context.Connection.LocalPort == LoadBalancerPort)
            {
                response.StatusCode = 200;
                await response.Body.WriteAsync(OK, 0, OK.Length).ConfigureAwait(false);
                return;
            }

            if (context.Connection.LocalPort == 80)
            {
                response.StatusCode = 301;
                response.Headers.Add("Location", HttpsHostName);
                return;
            }

            var request = context.Request;

            var useJwt = context.Request.Headers.ContainsKey("Authorization");
            AuthenticateResult authResult;

            if (useJwt)
            {
                authResult = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme).ConfigureAwait(false);
                if (!authResult.Succeeded || !authResult.Principal.Identity.IsAuthenticated)
                {
                    await context.ChallengeAsync(JwtBearerDefaults.AuthenticationScheme).ConfigureAwait(false);
                    return;
                }
            }
            else
            {
                authResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
                if (!authResult.Succeeded || !authResult.Principal.Identity.IsAuthenticated)
                {
                    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = request.GetEncodedPathAndQuery() }).ConfigureAwait(false);
                    return;
                }
            }

            var username = authResult.Principal?.Identity?.Name ?? "(app)";

            var uri = new Uri(UriHelper.BuildAbsolute(this.host.ForwardScheme, new HostString(this.host.ForwardHost), default, request.Path, request.QueryString));
            await ProxyRequest(context, uri, username).ConfigureAwait(false);
        }

        private static async Task ProxyRequest(HttpContext context, Uri destinationUri, string username)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                throw new NotSupportedException();
            }

            using var requestMessage = context.CreateProxyHttpRequest(destinationUri, username);
            using var responseMessage = await context.SendProxyHttpRequest(requestMessage).ConfigureAwait(false);
            await context.CopyProxyHttpResponse(responseMessage).ConfigureAwait(false);
        }
    }
}
