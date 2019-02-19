using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColeSoft.Extensions.Logging.Splunk.Utils
{
    internal interface ISplunkLoggerProcessor
    {
        Func<IReadOnlyList<string>, Task> EmitAction { get; set; }

        void EnqueueMessage(string message);
    }
}