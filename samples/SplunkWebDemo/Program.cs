using System;
using ColeSoft.Extensions.Logging.Splunk;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SplunkWebDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.AddSplunk(options => options.Host = Environment.MachineName))
                .CaptureStartupErrors(false)
                .UseSetting("detailedErrors", "true")
                .UseStartup<Startup>();
    }
}
