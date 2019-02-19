using System.Net.Http;

namespace ColeSoft.Extensions.Logging.Splunk.Utils
{
    /// <summary>
    /// This abstraction is needed because using the system provided
    /// implementation via AddHttpClient will cause a stackoverflow exception
    /// since it internally uses the Logging framework.
    /// </summary>
    internal interface IHttpClientProvider
    {
        HttpClient CreateClient();
    }
}
