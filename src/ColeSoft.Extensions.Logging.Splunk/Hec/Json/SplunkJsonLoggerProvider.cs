using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ColeSoft.Extensions.Logging.Splunk.Hec.Json
{
#pragma warning disable CC0021 // Use nameof
    [ProviderAlias("Splunk")]
#pragma warning restore CC0021 // Use nameof
    internal class SplunkJsonLoggerProvider : SplunkHecBaseProvider
    {
        private readonly ISplunkJsonPayloadTransformer payloadTransformer;

        public SplunkJsonLoggerProvider(IOptionsMonitor<SplunkLoggerOptions> options, ISplunkJsonPayloadTransformer payloadTransformer)
            : base(options, "event")
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

        protected override async Task SendToSplunk(IReadOnlyList<string> messages)
        {
            var formattedMessage = string.Join(" ", messages);
            var stringContent = new StringContent(formattedMessage, Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(string.Empty, stringContent).ConfigureAwait(false);
            await DebugSplunkResponse(response, "json").ConfigureAwait(false);
        }
    }
}
