namespace AzureADProxy
{
    public sealed class Host : IHost
    {
        public Host(string forwardHost, string forwardScheme)
        {
            this.ForwardHost = forwardHost;
            this.ForwardScheme = forwardScheme;
        }

        public string ForwardHost { get; }

        public string ForwardScheme { get; }
    }
}
