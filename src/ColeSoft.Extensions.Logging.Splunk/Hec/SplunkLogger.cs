using System;
using System.Collections.Generic;
using ColeSoft.Extensions.Logging.Splunk.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace ColeSoft.Extensions.Logging.Splunk.Hec
{
    internal abstract class SplunkLogger : ILogger
    {
        private readonly string name;

        protected SplunkLogger(string name, ISplunkLoggerProcessor loggerProcessor, ISplunkPayloadTransformer payloadTransformer)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            LoggerProcessor = loggerProcessor;
            PayloadTransformer = payloadTransformer;
        }

        internal IExternalScopeProvider ScopeProvider { get; set; }

        internal SplunkLoggerOptions Options { get; set; }

        protected ISplunkPayloadTransformer PayloadTransformer { get; }

        protected ISplunkLoggerProcessor LoggerProcessor { get; }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, name, eventId, message, exception);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;

        protected abstract void WriteMessage(LogLevel logLevel, string logName, EventId eventId, string message, Exception exception);
    }
}
