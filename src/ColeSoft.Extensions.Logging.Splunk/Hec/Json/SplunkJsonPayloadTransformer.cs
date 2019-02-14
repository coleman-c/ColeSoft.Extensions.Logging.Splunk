using System;

namespace ColeSoft.Extensions.Logging.Splunk.Hec.Json
{
    internal class SplunkJsonPayloadTransformer : ISplunkJsonPayloadTransformer
    {
        private readonly Func<LogData, object> payloadCreator;

        public SplunkJsonPayloadTransformer()
        {
            payloadCreator = data => data;
        }

        public SplunkJsonPayloadTransformer(Func<LogData, object> payloadCreator)
        {
            this.payloadCreator = payloadCreator;
        }

        public object Transform(LogData logData)
        {
            return payloadCreator(logData);
        }
    }
}