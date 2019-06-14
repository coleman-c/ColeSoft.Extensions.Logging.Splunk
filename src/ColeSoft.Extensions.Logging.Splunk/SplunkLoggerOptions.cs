using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ColeSoft.Extensions.Logging.Splunk
{
    /// <summary>
    /// Configuration options for the Splunk HEC Logging Provider, a Log Provider for the <see cref="Microsoft.Extensions.Logging"/> framework.
    /// </summary>
    public class SplunkLoggerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplunkLoggerOptions"/> class.  Default values are used.
        /// </summary>
        public SplunkLoggerOptions()
        {
            ChannelIdType = ChannelIdOption.None;
            Timeout = 1500;
            IncludeScopes = false;
            UseAuthTokenAsQueryString = false;
            BatchInterval = 1000;
            BatchSize = 50;
            Fields = new Dictionary<string, string>();
        }

        /// <summary>
        /// Specifies where, if at all, to include the channel identifier in the query to the endpoint.
        /// </summary>
        public enum ChannelIdOption
        {
            /// <summary>
            /// No channel Id will be sent
            /// </summary>
            None,

            /// <summary>
            /// Channel Id will be sent in the query string
            /// </summary>
            QueryString,

            /// <summary>
            /// Channel Id will be sent in the request header
            /// </summary>
            RequestHeader
        }

        /// <summary>
        /// Gets or sets whether to include scope information for log events.  The default value is <c>false</c>.
        /// </summary>
        public bool IncludeScopes { get; set; }

        /// <summary>
        /// Gets or sets the Url for the splunk collector.
        /// </summary>
        /// <remarks>This must be specified.</remarks>
        /// <example>https://http-inputs-&lt;customer&gt;.splunkcloud.com/services/collector.</example>
        /// <example>https://my-server:8088/services/collector.</example>
        [Required]
        public string SplunkCollectorUrl { get; set; }

        /// <summary>
        /// Gets or sets the HEC authentication token.
        /// </summary>
        /// <remarks>This must be specified.  See <see href="http://dev.splunk.com/view/event-collector/SP-CAAAE6P#auth"/> for more information.</remarks>
        [Required]
        public string AuthenticationToken { get; set; }

        /// <summary>
        /// Gets or sets indication to use or not hec token authentication at query string.
        /// </summary>
        /// <remarks>Only applicable for Splunk Cloud.  See <see href="http://dev.splunk.com/view/event-collector/SP-CAAAE6P#auth"/> for more information.</remarks>
        public bool UseAuthTokenAsQueryString { get; set; }

        /// <summary>
        /// Gets or sets the timeout (in milliseconds) used for http HEC requests when sending data to the Splunk instance.  The default value is 1500.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets any custom header to be applied at HEC calls.
        /// </summary>
        /// <value>The custom headers.</value>
        public Dictionary<string, string> CustomHeaders { get; set; }

        /// <summary>
        /// Gets or sets where, if at all, to include the channel identifier in the query to the endpoint.  The default value is <see cref="ChannelIdOption.None"/>.
        /// </summary>
        public ChannelIdOption ChannelIdType { get; set; }

        /// <summary>
        /// The host value to assign to the event data. This is typically the hostname of the client from which you're sending data.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the name of the index by which the event data is to be indexed.
        /// The index you specify here must within the list of allowed indexes if the token has the indexes parameter set.
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// Gets or sets the source value to assign to the event data.
        /// For example, if you're sending data from an app you're developing, you could set this key to the name of the app.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the sourcetype value to assign to the event data.
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// The format string used to format the timestamp within the <see cref="LogData"/>.  The default value of null will result in
        /// the number of seconds to 3 decimal places since the unix epoch time being used.
        /// Any other format string here will be passed to the <see cref="System.DateTime.ToString(string)"/> method.
        /// </summary>
        /// <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings"/> and
        /// <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings"/>
        public string TimestampFormat { get; set; }

        /// <summary>
        /// The frequency, in  milliseconds, with which to try and send events to the HEC endpoint.
        /// A value of 0 will only result in sends when greater than <see cref="BatchSize"/> events have been collected.
        /// The default value is 1000.
        /// </summary>
        public int BatchInterval { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BatchSize"/>.  Once <see cref="BatchSize"/> items are collected they will be sent regardless of time till the next <see cref="BatchInterval"/>.
        /// Also, The maximum number of items to send in a single batch.  The default value is 50.
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Gets or Sets any explicit custom fields to be defined at index time.
        /// Requests containing the "fields" property must be sent to the <see cref="SplunkEndpoint.Json"/>, or they will not be indexed.
        /// </summary>
        public Dictionary<string, string> Fields { get; set; }
    }
}