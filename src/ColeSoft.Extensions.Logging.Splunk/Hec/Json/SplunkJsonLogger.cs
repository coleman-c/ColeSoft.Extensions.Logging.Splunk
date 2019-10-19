using System;
using System.Collections.Generic;
using ColeSoft.Extensions.Logging.Splunk.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ColeSoft.Extensions.Logging.Splunk.Hec.Json
{
    internal class SplunkJsonLogger : SplunkLogger
    {
        public SplunkJsonLogger(string name, ISplunkLoggerProcessor loggerProcessor, ISplunkPayloadTransformer payloadTransformer)
            : base(name, loggerProcessor, payloadTransformer)
        {
        }

        protected KeyValuePair<string, object>[] GetStructuredScopeInformation()
        {
            var scopeProvider = ScopeProvider;
            if (Options.IncludeScopes && scopeProvider != null)
            {
                var scopes = new List<KeyValuePair<string, object>>();

                scopeProvider.ForEachScope<object>(
                    (scope, state) =>
                    {
                        if (scope is IEnumerable<KeyValuePair<string, object>> kvps)
                        {
                            if (Options.IncludeStructuredScopesAsFields)
                            {
                                scopes.AddRange(kvps);
                            }
                        }
                    },
                    null);

                return scopes.ToArray();
            }

            return Array.Empty<KeyValuePair<string, object>>();
        }

        protected string[] GetTextualScopeInformation()
        {
            var scopeProvider = ScopeProvider;
            if (Options.IncludeScopes && scopeProvider != null)
            {
                var scopes = new List<string>();

                scopeProvider.ForEachScope<object>(
                    (scope, state) =>
                    {
                        if ((!(scope is IEnumerable<KeyValuePair<string, object>>)) && Options.IncludeStructuredScopesAsText)
                        {
                            scopes.Add(scope.ToString());
                        }
                    },
                    null);

                return scopes.ToArray();
            }

            return Array.Empty<string>();
        }

        protected override void WriteMessage(LogLevel logLevel, string logName, EventId eventId, string message, Exception exception)
        {
            var dateTime = DateTime.UtcNow;

            var fieldsDictionary = new Dictionary<string, string>(
                Options.Fields);

            foreach (var kvp in GetStructuredScopeInformation())
            {
                fieldsDictionary.Add(kvp.Key, kvp.Value?.ToString());
            }

            var splunkEventData = new SplunkEventData(
                PayloadTransformer.Transform(
                    new LogData
                    {
                        Timestamp = dateTime.FormatForSplunk(Options.TimestampFormat),
                        CategoryName = logName,
                        Scope = GetTextualScopeInformation(),
                        Level = logLevel,
                        Event = eventId,
                        Message = message,
                        Exception = exception
                    }),
                dateTime.FormatForSplunk(null),  // JsonEventCollector must be in Epoch sec's format.
                Options.Host,
                Options.Index,
                Options.Source,
                Options.SourceType,
                fieldsDictionary);

            LoggerProcessor.EnqueueMessage(JsonConvert.SerializeObject(splunkEventData));
        }
    }
}