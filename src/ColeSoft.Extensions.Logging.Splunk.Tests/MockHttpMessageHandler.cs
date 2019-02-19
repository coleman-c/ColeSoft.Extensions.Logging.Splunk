using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ColeSoft.Extensions.Logging.Splunk.Hec;
using ColeSoft.Extensions.Logging.Splunk.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ColeSoft.Extensions.Logging.Splunk.Tests
{
    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

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

        public override ILogger CreateLogger(string categoryName)
        {
            return new TestLogger();
        }

        protected override Task SendToSplunkAsync(IReadOnlyList<string> messages)
        {
            throw new System.NotImplementedException();
        }

        public HttpClient GetHttpClientForTestInspection() => HttpClient;
    }

    internal class TestLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
    }
}
