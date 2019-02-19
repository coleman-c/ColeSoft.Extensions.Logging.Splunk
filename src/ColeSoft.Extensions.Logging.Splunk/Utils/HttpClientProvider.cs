using System.Net.Http;

namespace ColeSoft.Extensions.Logging.Splunk.Utils
{
    internal class HttpClientProvider : IHttpClientProvider
    {
        public HttpClient CreateClient()
        {
            return new HttpClient();
        }
    }
}