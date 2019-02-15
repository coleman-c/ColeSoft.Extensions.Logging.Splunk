using System;
using System.IO;
using System.Text;
using System.Threading;
using ColeSoft.Extensions.Logging.Splunk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SplunkConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            // Set up configuration sources.
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddSplunk(options =>
                {
                    options.Host = Environment.MachineName;
                    options.AuthenticationToken = "92C168CF-C097-45F3-A3A8-128C3C509E9F";
                    options.SplunkCollectorUrl = "https://gbwyeon0085.dom1.e-ssi.net:8088/services/collector/";
                });
            });

            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();

            logger.LogInformation("A log message");

            // TODO - Bug with messages not being written on program exit.
            Thread.Sleep(5000);
        }
    }
}
