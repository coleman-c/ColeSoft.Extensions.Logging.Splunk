using System;
using System.Collections.Generic;
using ColeSoft.Extensions.Logging.Splunk.Utils;
using Microsoft.Extensions.Logging;

namespace ColeSoft.Extensions.Logging.Splunk.Hec.Raw
{
    internal class SplunkRawLogger : SplunkLogger
    {
        public SplunkRawLogger(string name, ISplunkLoggerProcessor loggerProcessor, ISplunkPayloadTransformer payloadTransformer)
            : base(name, loggerProcessor, payloadTransformer)
        {
        }

        protected override void WriteMessage(LogLevel logLevel, string logName, EventId eventId, string message, Exception exception)
        {
            // Queue log message
            var splunkEventData = PayloadTransformer.Transform(
                new LogData
                {
                    Timestamp = DateTime.UtcNow.FormatForSplunk(Options.TimestampFormat),
                    CategoryName = logName,
                    Scope = GetTextualScopeInformation(),
                    Level = logLevel,
                    Event = eventId,
                    Message = message,
                    Exception = exception
                }).ToString();

            LoggerProcessor.EnqueueMessage(splunkEventData);
        }

        protected string[] GetTextualScopeInformation()
        {
            var scopeProvider = ScopeProvider;
            if (Options.IncludeScopes && scopeProvider != null)
            {
                var scopes = new List<string>();

                scopeProvider.ForEachScope<object>((scope, state) => { scopes.Add(scope.ToString()); }, null);

                return scopes.ToArray();
            }

            return Array.Empty<string>();
        }
    }
}