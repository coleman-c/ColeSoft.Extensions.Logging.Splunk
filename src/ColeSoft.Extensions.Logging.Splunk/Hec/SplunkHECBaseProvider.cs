using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ColeSoft.Extensions.Logging.Splunk.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ColeSoft.Extensions.Logging.Splunk.Hec
{
    internal abstract class SplunkHecBaseProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly IOptionsMonitor<SplunkLoggerOptions> options;
        private readonly string endPointCustomization;
        private readonly IDisposable optionsReloadToken;

        protected SplunkHecBaseProvider(IOptionsMonitor<SplunkLoggerOptions> options, string endPointCustomization)
        {
            this.options = options;
            this.endPointCustomization = endPointCustomization;

            MessageQueue = new BatchedSplunkLoggerProcessor(options, SendToSplunk);

            optionsReloadToken = this.options.OnChange(ReloadLoggerOptions);

            ReloadLoggerOptions(options.CurrentValue);
        }

        protected SplunkLoggerOptions CurrentOptions => options.CurrentValue;

        protected ConcurrentDictionary<string, SplunkLogger> Loggers { get; } = new ConcurrentDictionary<string, SplunkLogger>();

        protected BatchedSplunkLoggerProcessor MessageQueue { get; }

        protected HttpClient HttpClient { get; set; } = new HttpClient();

        protected IExternalScopeProvider ScopeProvider { get; set; } = NullExternalScopeProvider.Instance;

        public abstract ILogger CreateLogger(string categoryName);

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            ScopeProvider = scopeProvider;

            foreach (var logger in Loggers)
            {
                logger.Value.ScopeProvider = ScopeProvider;
            }
        }

        public void Dispose()
        {
            optionsReloadToken?.Dispose();
            MessageQueue.Dispose();
            HttpClient.Dispose();
        }

        protected abstract Task SendToSplunk(IReadOnlyList<string> messages);

        protected async Task DebugSplunkResponse(HttpResponseMessage responseMessage, string loggerType)
        {
            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Request completed successfully.");
                    break;
                case System.Net.HttpStatusCode.Created:
                    Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Create request completed successfully.");
                    break;
                case System.Net.HttpStatusCode.BadRequest:
                    Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Request error. See response body for details.");
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Authentication failure, invalid access credentials.");
                    break;
                case System.Net.HttpStatusCode.PaymentRequired:
                    Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: In-use Splunk Enterprise license disables this feature.");
                    break;
                case System.Net.HttpStatusCode.Forbidden:
                    Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Insufficient permission.");
                    break;
                case System.Net.HttpStatusCode.NotFound:
                    Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Requested endpoint does not exist.");
                    break;
                case System.Net.HttpStatusCode.Conflict:
                    Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Invalid operation for this endpoint. See response body for details.");
                    break;
                case System.Net.HttpStatusCode.InternalServerError:
                    Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Unspecified internal server error. See response body for details.");
                    break;
                case System.Net.HttpStatusCode.ServiceUnavailable:
                    Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Feature is disabled in configuration file.");
                    break;
                default:
                    Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Unknown error code: {responseMessage.StatusCode}.");
                    break;
            }

            Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Body: {await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false)}.");
        }

        private void ReloadLoggerOptions(SplunkLoggerOptions options)
        {
            foreach (var logger in Loggers)
            {
                logger.Value.Options = options;
            }

            SetupHttpClient(options, endPointCustomization);
        }

        private void SetupHttpClient(SplunkLoggerOptions options, string endPointCustomization)
        {
            HttpClient = new HttpClient()
            {
                BaseAddress = GetSplunkCollectorUrl(options, endPointCustomization)
            };

            if (options.Timeout > 0)
            {
                HttpClient.Timeout = TimeSpan.FromMilliseconds(options.Timeout);
            }

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Splunk", options.AuthenticationToken);
            if (options.ChannelIdType == SplunkLoggerOptions.ChannelIdOption.RequestHeader)
            {
                HttpClient.DefaultRequestHeaders.Add("x-splunk-request-channel", Guid.NewGuid().ToString());
            }

            if (options.CustomHeaders != null && options.CustomHeaders.Count > 0)
            {
                options.CustomHeaders.ToList().ForEach(keyValuePair =>
                    HttpClient.DefaultRequestHeaders.Add(keyValuePair.Key, keyValuePair.Value));
            }
        }

        private Uri GetSplunkCollectorUrl(SplunkLoggerOptions options, string endPointCustomization)
        {
            var splunkCollectorUrl = options.SplunkCollectorUrl;
            if (!splunkCollectorUrl.EndsWith("/", StringComparison.InvariantCulture))
            {
                splunkCollectorUrl += "/";
            }

            if (!string.IsNullOrWhiteSpace(endPointCustomization))
            {
                splunkCollectorUrl += endPointCustomization;
            }

            if (options.ChannelIdType == SplunkLoggerOptions.ChannelIdOption.QueryString)
            {
                splunkCollectorUrl += $"?channel={Uri.EscapeDataString(Guid.NewGuid().ToString())}";
            }

            if (options.UseAuthTokenAsQueryString)
            {
                var tokenParameter = "token=" + Uri.EscapeDataString(options.AuthenticationToken);
                splunkCollectorUrl += $"{(splunkCollectorUrl.Contains("?") ? "&" : "?")}{tokenParameter}";
            }

            return new Uri(splunkCollectorUrl);
        }
    }
}