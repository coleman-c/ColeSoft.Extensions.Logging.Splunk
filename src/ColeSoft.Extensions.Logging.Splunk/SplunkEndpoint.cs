namespace ColeSoft.Extensions.Logging.Splunk
{
    /// <summary>
    /// Describes which Splunk HEC endpoint to use.  <see cref="http://dev.splunk.com/view/event-collector/SP-CAAAE6P"/>
    /// </summary>
    public enum SplunkEndpoint
    {
        /// <summary>
        /// Use the services/collector/event endpoint
        /// </summary>
        Json = 1,

        /// <summary>
        /// Use the services/collector/raw endpoint
        /// </summary>
        Raw
    }
}