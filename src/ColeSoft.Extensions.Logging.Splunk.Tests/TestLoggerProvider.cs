using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ColeSoft.Extensions.Logging.Splunk.Hec;
using ColeSoft.Extensions.Logging.Splunk.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ColeSoft.Extensions.Logging.Splunk.Tests
{
    internal class TestLoggerProvider : SplunkProviderBase
    {
        public TestLoggerProvider(
            IHttpClientProvider httpClientFactory,
            IOptionsMonitor<SplunkLoggerOptions> options,
            ISplunkLoggerProcessor loggerProcessor,
            string endPointCustomization)
            : base(httpClientFactory, options, loggerProcessor, endPointCustomization)
        {
        }

        public HttpClient GetHttpClientForTestInspection() => HttpClient;

        public override ILogger CreateLogger(string categoryName)
        {
            return new TestLogger();
        }

        protected override Task SendToSplunkAsync(IReadOnlyList<string> messages)
        {
            throw new System.NotImplementedException();
        }
    }
}
