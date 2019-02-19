using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ColeSoft.Extensions.Logging.Splunk.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ColeSoft.Extensions.Logging.Splunk.Hec.Json
{
#pragma warning disable CC0021 // Use nameof
    [ProviderAlias("Splunk")]
#pragma warning restore CC0021 // Use nameof
    internal class SplunkJsonLoggerProvider : SplunkProviderBase
    {
        private readonly ISplunkJsonPayloadTransformer payloadTransformer;

        public SplunkJsonLoggerProvider(IHttpClientProvider httpClientFactory, IOptionsMonitor<SplunkLoggerOptions> options, ISplunkJsonPayloadTransformer payloadTransformer, ISplunkLoggerProcessor loggerProcessor)
            : base(httpClientFactory, options, loggerProcessor, "event")
        {
            this.payloadTransformer = payloadTransformer;
        }

        public override ILogger CreateLogger(string categoryName)
        {
            return Loggers.GetOrAdd(
                categoryName,
                loggerName =>
                    new SplunkJsonLogger(categoryName, MessageQueue, payloadTransformer)
                    {
                        Options = CurrentOptions,
                        ScopeProvider = ScopeProvider
                    });
        }

        protected override async Task SendToSplunkAsync(IReadOnlyList<string> messages)
        {
            var formattedMessage = string.Join(" ", messages);
            var stringContent = new StringContent(formattedMessage, Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(string.Empty, stringContent).ConfigureAwait(false);
            await DebugSplunkResponseAsync(response, "json").ConfigureAwait(false);
        }
    }
}
