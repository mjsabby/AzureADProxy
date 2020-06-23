namespace AzureADProxy
{
    public interface IHost
    {
        string ForwardHost { get; }

        string ForwardScheme { get; }
    }
}
