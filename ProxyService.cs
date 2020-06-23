namespace AzureADProxy
{
    using System;
    using System.Globalization;
    using System.Net.Http;

    internal sealed class ProxyService
    {
        private static readonly int RequestForwardTimeOutInSeconds = int.Parse(Environment.GetEnvironmentVariable("RequestForwardTimeOutInSeconds"), CultureInfo.InvariantCulture);

        public ProxyService(IHttpClientFactory httpClientFactory)
        {
            this.Client = httpClientFactory.CreateClient("ProxyClient");
            this.Client.Timeout = TimeSpan.FromSeconds(RequestForwardTimeOutInSeconds);
        }

        public HttpClient Client { get; }
    }
}
