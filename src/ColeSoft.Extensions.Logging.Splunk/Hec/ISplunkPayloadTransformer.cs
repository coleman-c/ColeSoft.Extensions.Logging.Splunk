namespace ColeSoft.Extensions.Logging.Splunk.Hec
{
    internal interface ISplunkPayloadTransformer
    {
        object Transform(LogData logData);
    }

    internal interface ISplunkRawPayloadTransformer : ISplunkPayloadTransformer
    {
    }

    internal interface ISplunkJsonPayloadTransformer : ISplunkPayloadTransformer
    {
    }
}