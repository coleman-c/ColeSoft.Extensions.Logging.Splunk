using System;
using ColeSoft.Extensions.Logging.Splunk.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ColeSoft.Extensions.Logging.Splunk.Hec.Json
{
    internal class SplunkJsonLogger : SplunkLogger
    {
        public SplunkJsonLogger(string name, BatchedSplunkLoggerProcessor loggerProcessor, ISplunkPayloadTransformer payloadTransformer)
            : base(name, loggerProcessor, payloadTransformer)
        {
        }

        protected override void WriteMessage(LogLevel logLevel, string logName, EventId eventId, string message, Exception exception)
        {
            var dateTime = DateTime.UtcNow;

            var splunkEventData = new SplunkEventData(
                PayloadTransformer.Transform(
                    new LogData
                    {
                        Timestamp = dateTime.FormatForSplunk(Options.TimestampFormat),
                        CategoryName = logName,
                        Scope = GetScopeInformation(),
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
                Options.Fields);

            LoggerProcessor.EnqueueMessage(JsonConvert.SerializeObject(splunkEventData));
        }
    }
}