using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ColeSoft.Extensions.Logging.Splunk.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ColeSoft.Extensions.Logging.Splunk.Hec
{
    internal abstract class SplunkProviderBase : ILoggerProvider, ISupportExternalScope
    {
#pragma warning disable CC0021 // Use nameof
        private static readonly string SplunkHeaderValue = "Splunk";
#pragma warning restore CC0021 // Use nameof

        private readonly IHttpClientProvider httpClientFactory;
        private readonly IOptionsMonitor<SplunkLoggerOptions> currentOptions;
        private readonly string endPointCustomization;
        private readonly IDisposable optionsReloadToken;
        private readonly SemaphoreSlim httpClientSemaphore = new SemaphoreSlim(1, 1);

        protected SplunkProviderBase(IHttpClientProvider httpClientFactory, IOptionsMonitor<SplunkLoggerOptions> options, ISplunkLoggerProcessor loggerProcessor, string endPointCustomization)
        {
            this.httpClientFactory = httpClientFactory;
            currentOptions = options;
            this.endPointCustomization = endPointCustomization;

            MessageQueue = loggerProcessor;
            MessageQueue.EmitAction = SendToSplunkInternalAsync;

            ReloadLoggerOptions(options.CurrentValue);

            optionsReloadToken = currentOptions.OnChange(ReloadLoggerOptions);
        }

        protected internal IExternalScopeProvider ScopeProvider { get; set; } = NullExternalScopeProvider.Instance;

        protected SplunkLoggerOptions CurrentOptions => currentOptions.CurrentValue;

        protected ConcurrentDictionary<string, SplunkLogger> Loggers { get; } = new ConcurrentDictionary<string, SplunkLogger>();

        protected ISplunkLoggerProcessor MessageQueue { get; }

        protected HttpClient HttpClient { get; set; }

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected static async Task DebugSplunkResponseAsync(HttpResponseMessage responseMessage, string loggerType)
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

            if (responseMessage.Content != null)
            {
                Debug.WriteLine(
                    $"[DEBUG] Splunk HEC {loggerType} Body: {await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false)}.");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                optionsReloadToken?.Dispose();
                (MessageQueue as IDisposable)?.Dispose();
                HttpClient.Dispose();
                httpClientSemaphore.Dispose();
            }
        }

        protected abstract Task SendToSplunkAsync(IReadOnlyList<string> messages);

        private static Uri GetSplunkCollectorUrl(SplunkLoggerOptions options, string endPointCustomization)
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

        private async Task SendToSplunkInternalAsync(IReadOnlyList<string> messages)
        {
            await httpClientSemaphore.WaitAsync();
            try
            {
                await SendToSplunkAsync(messages);
            }
            catch (Exception e)
            {
                Debug.Write(e, "Failed to send log messages to Splunk");
            }
            finally
            {
                httpClientSemaphore.Release();
            }
        }

        private HttpClient SetupHttpClient(SplunkLoggerOptions options)
        {
            var httpClient = httpClientFactory.CreateClient();

            httpClient.BaseAddress = GetSplunkCollectorUrl(options, endPointCustomization);

            if (options.Timeout > 0)
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(options.Timeout);
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(SplunkHeaderValue, options.AuthenticationToken);
            if (options.ChannelIdType == SplunkLoggerOptions.ChannelIdOption.RequestHeader)
            {
                httpClient.DefaultRequestHeaders.Add("x-splunk-request-channel", Guid.NewGuid().ToString());
            }

            if (options.CustomHeaders != null && options.CustomHeaders.Count > 0)
            {
                options.CustomHeaders.ToList().ForEach(keyValuePair =>
                    httpClient.DefaultRequestHeaders.Add(keyValuePair.Key, keyValuePair.Value));
            }

            return httpClient;
        }

        private void ReloadLoggerOptions(SplunkLoggerOptions options)
        {
            foreach (var logger in Loggers)
            {
                logger.Value.Options = options;
            }

            var newClient = SetupHttpClient(options);
            HttpClient oldClient;
            httpClientSemaphore.Wait();
            try
            {
                oldClient = HttpClient;
                HttpClient = newClient;
            }
            finally
            {
                httpClientSemaphore.Release();
            }

            oldClient?.Dispose();
        }
    }
}