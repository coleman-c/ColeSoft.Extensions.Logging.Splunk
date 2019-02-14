using System;

namespace ColeSoft.Extensions.Logging.Splunk.Utils
{
    internal static class DateTimeExtensions
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string FormatForSplunk(this DateTime dateTime, string formatString)
        {
            return string.IsNullOrWhiteSpace(formatString)
                ? (dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds.ToString("#.000")
                : dateTime.ToString(formatString);
        }
    }
}
