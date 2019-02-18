using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace ColeSoft.Extensions.Logging.Splunk.Utils
{
    internal class LoggerProviderConfigureOptions<TOptions, TProvider> : ConfigureFromConfigurationOptions<TOptions>
        where TOptions : class
    {
        public LoggerProviderConfigureOptions(ILoggerProviderConfiguration<TProvider> providerConfiguration)
            : base(providerConfiguration.Configuration)
        {
        }
    }
}
