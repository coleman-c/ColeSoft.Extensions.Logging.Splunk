using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ColeSoft.Extensions.Logging.Splunk
{
    /// <summary>
    /// The captured data which can be passed to the logger.  Implementations can transform this by passing a
    /// user defined delegate to the logging configuration returning a new Object that will be logged.
    /// </summary>
    public class LogData
    {
        /// <summary>
        /// The formatted timestamp of the log event.  The default format is the number of
        /// seconds to 3 decimal places since the unix epoch time.  It can be overridden by specifying a
        /// <see cref="DateTime"/> format string to the <see cref="SplunkLoggerOptions.TimestampFormat"/> configuration option.
        /// <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings"/> and
        /// <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings"/>.
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// The category of the Logger that created this log event.  This is the value passed to <see cref="ILoggerFactory.CreateLogger"/>.
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// The stack of logging scopes that led up to this logging event.
        /// For this to be populated then <see cref="SplunkLoggerOptions.IncludeScopes"/> must be <c>true</c>.
        /// </summary>
        public string[] Scope { get; set; }

        /// <summary>
        /// The <see cref="LogLevel"/> that this event was logged with.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LogLevel Level { get; set; }

        /// <summary>
        /// The <see cref="EventId"/> that this event was logged with.
        /// </summary>
        public EventId Event { get; set; }

        /// <summary>
        /// The message that this event was logged with.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The <see cref="Exception"/>, if any, that was logged alongside this event.
        /// </summary>
        public Exception Exception { get; set; }
    }
}