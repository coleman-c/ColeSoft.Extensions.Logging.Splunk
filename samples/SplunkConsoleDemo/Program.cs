using System;
using ColeSoft.Extensions.Logging.Splunk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SplunkConsoleDemo
{
    class Program
    {
        static void Main()
        {
            var services = new ServiceCollection();

            // Set up configuration sources.
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddSplunk(
                    SplunkEndpoint.Raw,
                    options =>
                    {
                        options.Host = Environment.MachineName;
                        options.AuthenticationToken = "92C168CF-C097-45F3-A3A8-128C3C509E9F";
                        options.SplunkCollectorUrl = "https://gbwyeon0085.dom1.e-ssi.net:8088/services/collector/";
                        options.ChannelIdType = SplunkLoggerOptions.ChannelIdOption.RequestHeader;
                    });
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var logger = serviceProvider.GetService<ILogger<Program>>();

                logger.LogInformation("A log message");
            }
        }
    }
}
