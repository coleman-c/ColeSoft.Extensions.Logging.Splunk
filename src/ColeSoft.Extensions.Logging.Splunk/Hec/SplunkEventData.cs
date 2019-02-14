// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace ColeSoft.Extensions.Logging.Splunk.Hec
{
    /// <summary>
    /// Class representing an "Event" consumed by the Splunk HTTP Event Collector (HEC).
    /// </summary>
    internal class SplunkEventData
    {
        public SplunkEventData(
            object eventData,
            string timestamp,
            string host = null,
            string index = null,
            string source = null,
            string sourceType = null,
            Dictionary<string, string> fields = null)
        {
            Timestamp = timestamp;
            Event = eventData;
            Host = host;
            Index = index;
            Source = source;
            SourceType = sourceType;
            Fields = fields;
        }

        /// <summary>
        /// Event timestamp in epoch format.
        /// </summary>
        [JsonProperty(PropertyName = "time")]
        public string Timestamp { get; private set; }

        /// <summary>
        /// Event metadata host.
        /// </summary>
        [JsonProperty(PropertyName = "host", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Host { get; private set; }

        /// <summary>
        /// Event metadata index.
        /// </summary>
        [JsonProperty(PropertyName = "index", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Index { get; private set; }

        /// <summary>
        /// Event metadata source.
        /// </summary>
        [JsonProperty(PropertyName = "source", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Source { get; private set; }

        /// <summary>
        /// Event metadata sourcetype.
        /// </summary>
        [JsonProperty(PropertyName = "sourcetype", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SourceType { get; private set; }

        /// <summary>
        /// Event data.
        /// </summary>
        [JsonProperty(PropertyName = "event", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object Event { get; private set; }

        /// <summary>
        /// Fields data.
        /// </summary>
        [JsonProperty(PropertyName = "fields", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, string> Fields { get; private set; }
    }
}
