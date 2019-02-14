using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ColeSoft.Extensions.Logging.Splunk.Hec.Raw
{
    internal class SplunkRawPayloadTransformer : ISplunkRawPayloadTransformer
    {
        private static readonly string LogLevelPadding = ": ";
        private static readonly string MessagePadding;
        private static readonly string NewLineWithMessagePadding;

        [ThreadStatic]
        private static StringBuilder logBuilder;

        private readonly Func<LogData, object> payloadCreator;

        static SplunkRawPayloadTransformer()
        {
            var logLevelString = LogLevel.Information.ToString();
            MessagePadding = new string(' ', logLevelString.Length + LogLevelPadding.Length);
            NewLineWithMessagePadding = Environment.NewLine + MessagePadding;
        }

        public SplunkRawPayloadTransformer()
        {
            payloadCreator = Default;
        }

        public SplunkRawPayloadTransformer(Func<LogData, object> payloadCreator)
        {
            this.payloadCreator = payloadCreator;
        }

        private static object Default(LogData arg)
        {
            var builder = logBuilder;
            logBuilder = null;

            if (builder == null)
            {
                builder = new StringBuilder();
            }

            // Example:
            // INFO: ConsoleApp.Program[10]
            //       Request received

            // category and event id
            builder.Append(arg.Timestamp);
            builder.Append(LogLevelPadding);
            builder.Append(arg.Level);
            builder.Append(LogLevelPadding);
            builder.Append(arg.CategoryName);
            builder.Append("[");
            builder.Append(arg.Event.ToString());
            builder.AppendLine("]");

            // scope information
            GetScopeInformation(builder, arg);

            if (!string.IsNullOrEmpty(arg.Message))
            {
                // message
                builder.Append(MessagePadding);

                var len = builder.Length;
                builder.AppendLine(arg.Message);
                builder.Replace(Environment.NewLine, NewLineWithMessagePadding, len, arg.Message.Length);
            }

            // Example:
            // System.InvalidOperationException
            //    at Namespace.Class.Function() in File:line X
            if (arg.Exception != null)
            {
                // exception message
                builder.AppendLine(arg.Exception.ToString());
            }

            var str = builder.ToString();

            builder.Clear();
            if (builder.Capacity > 1024)
            {
                builder.Capacity = 1024;
            }

            logBuilder = builder;

            return str;
        }

        private static void GetScopeInformation(StringBuilder stringBuilder, LogData arg)
        {
            if (arg.Scope != null && arg.Scope.Length > 0)
            {
                var initialLength = stringBuilder.Length;

                foreach (var scope in arg.Scope)
                {
                    var first = initialLength == stringBuilder.Length;
                    stringBuilder.Append(first ? "=> " : " => ").Append(scope);
                }

                if (stringBuilder.Length > initialLength)
                {
                    stringBuilder.Insert(initialLength, MessagePadding);
                    stringBuilder.AppendLine();
                }
            }
        }

        public object Transform(LogData logData)
        {
            return payloadCreator(logData);
        }
    }
}