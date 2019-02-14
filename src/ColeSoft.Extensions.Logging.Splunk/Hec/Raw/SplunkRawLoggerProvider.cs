// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ColeSoft.Extensions.Logging.Splunk.Hec.Raw
{
    [ProviderAlias("Splunk")]
    internal class SplunkRawLoggerProvider : SplunkHecBaseProvider
    {
        private readonly ISplunkRawPayloadTransformer payloadTransformer;

        public SplunkRawLoggerProvider(IOptionsMonitor<SplunkLoggerOptions> options, ISplunkRawPayloadTransformer payloadTransformer)
            : base(options, "raw")
        {
            this.payloadTransformer = payloadTransformer;
        }

        public override ILogger CreateLogger(string categoryName)
        {
            return Loggers.GetOrAdd(
                categoryName,
                loggerName =>
                    new SplunkRawLogger(categoryName, MessageQueue, payloadTransformer)
                    {
                        Options = CurrentOptions,
                        ScopeProvider = ScopeProvider
                    });
        }

        protected override async Task SendToSplunk(IReadOnlyList<string> messages)
        {
            var builder = new StringBuilder();
            if (CurrentOptions.Source != null)
            {
                var tokenParameter = "source=" + Uri.EscapeDataString(CurrentOptions.Source);
                builder.Append($"{(builder.Length > 0 || HttpClient.BaseAddress.ToString().Contains("?") ? "&" : "?")}{tokenParameter}");
            }

            if (CurrentOptions.SourceType != null)
            {
                var tokenParameter = "sourcetype=" + Uri.EscapeDataString(CurrentOptions.SourceType);
                builder.Append($"{(builder.Length > 0 || HttpClient.BaseAddress.ToString().Contains("?") ? "&" : "?")}{tokenParameter}");
            }

            if (CurrentOptions.Index != null)
            {
                var tokenParameter = "index=" + Uri.EscapeDataString(CurrentOptions.Index);
                builder.Append($"{(builder.Length > 0 || HttpClient.BaseAddress.ToString().Contains("?") ? "&" : "?")}{tokenParameter}");
            }

            if (CurrentOptions.Host != null)
            {
                var tokenParameter = "host=" + Uri.EscapeDataString(CurrentOptions.Host);
                builder.Append($"{(builder.Length > 0 || HttpClient.BaseAddress.ToString().Contains("?") ? "&" : "?")}{tokenParameter}");
            }

            var formattedMessage = string.Join("\r\n", messages.Select(evt => evt.Trim()));
            var stringContent = new StringContent(formattedMessage);
            var response = await HttpClient.PostAsync(builder.ToString(), stringContent).ConfigureAwait(false);
            await DebugSplunkResponse(response, "raw").ConfigureAwait(false);
        }
    }
}
