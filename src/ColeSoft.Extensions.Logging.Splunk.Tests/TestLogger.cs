using System;
using Microsoft.Extensions.Logging;

namespace ColeSoft.Extensions.Logging.Splunk.Tests
{
    internal class TestLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // No op
        }
    }
}
