using System;
using ColeSoft.Extensions.Logging.Splunk.Hec;
using ColeSoft.Extensions.Logging.Splunk.Hec.Json;
using ColeSoft.Extensions.Logging.Splunk.Hec.Raw;
using ColeSoft.Extensions.Logging.Splunk.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace ColeSoft.Extensions.Logging.Splunk
{
    public static class SplunkLoggerFactoryExtensions
    {
        /// <summary>
        /// Adds a Splunk HTTP Event Collector logger named 'Splunk' to the factory using the specified configuration. <br />
        /// The Json event endpoint, default payload structure and options from the existing Configuration will be used.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <returns>The Logging builder being configured.</returns>
        public static ILoggingBuilder AddSplunk(this ILoggingBuilder builder)
        {
            return builder.AddSplunk(SplunkEndpoint.Json);
        }

        /// <summary>
        /// Adds a Splunk HTTP Event Collector logger named 'Splunk' to the factory using the specified configuration. <br />
        /// The Json event endpoint and default payload structure will be used.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="configure">The delegate that configures the LoggerFactory.</param>
        /// <returns>The Logging builder being configured.</returns>
        public static ILoggingBuilder AddSplunk(this ILoggingBuilder builder, Action<SplunkLoggerOptions> configure)
        {
            return builder.AddSplunk(SplunkEndpoint.Json, configure, null);
        }

        /// <summary>
        /// Adds a Splunk HTTP Event Collector logger named 'Splunk' to the factory using the specified configuration. <br />
        /// The Json event endpoint and options from the existing Configuration will be used.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="payloadCreator">The delegate that returns an object to pass to the event end point.  Is the endpoint type is <see cref="SplunkEndpoint.Json"/> the object is serialized as Json, if  <see cref="SplunkEndpoint.Json"/> then  <see cref="object.ToString"/> is called.</param>
        /// <returns>The Logging builder being configured.</returns>
        public static ILoggingBuilder AddSplunk(this ILoggingBuilder builder, Func<LogData, object> payloadCreator)
        {
            return builder.AddSplunk(SplunkEndpoint.Json, c => { }, payloadCreator);
        }

        /// <summary>
        /// Adds a Splunk HTTP Event Collector logger named 'Splunk' to the factory using the specified configuration. <br />
        /// The Json event endpoint will be used .
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="configure">The delegate that configures the LoggerFactory.</param>
        /// <param name="payloadCreator">The delegate that returns an object to pass to the event end point.  Is the endpoint type is <see cref="SplunkEndpoint.Json"/> the object is serialized as Json, if  <see cref="SplunkEndpoint.Json"/> then  <see cref="object.ToString"/> is called.</param>
        /// <returns>The Logging builder being configured.</returns>
        public static ILoggingBuilder AddSplunk(this ILoggingBuilder builder, Action<SplunkLoggerOptions> configure, Func<LogData, object> payloadCreator)
        {
            return builder.AddSplunk(SplunkEndpoint.Json, configure, payloadCreator);
        }

        /// <summary>
        /// Adds a Splunk HTTP Event Collector logger named 'Splunk' to the factory using the specified configuration. <br />
        /// The default payload structure and options from the existing Configuration will be used.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="endpoint">The splunk endpoint type to use, Raw events are only supported in Splunk Enterprise 6.4.0, Splunk Light 6.4.0 and later as well as Splunk Cloud.</param>
        /// <returns>The Logging builder being configured.</returns>
        public static ILoggingBuilder AddSplunk(this ILoggingBuilder builder, SplunkEndpoint endpoint)
        {
            return builder.AddSplunk(endpoint, c => { }, null);
        }

        /// <summary>
        /// Adds a Splunk HTTP Event Collector logger named 'Splunk' to the factory using the specified configuration.  <br />
        /// The default payload structure will be used.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="endpoint">The splunk endpoint type to use, Raw events are only supported in Splunk Enterprise 6.4.0, Splunk Light 6.4.0 and later as well as Splunk Cloud.</param>
        /// <param name="configure">The delegate that configures the LoggerFactory.</param>
        /// <returns>The Logging builder being configured.</returns>
        public static ILoggingBuilder AddSplunk(this ILoggingBuilder builder, SplunkEndpoint endpoint, Action<SplunkLoggerOptions> configure)
        {
            return builder.AddSplunk(endpoint, configure, null);
        }

        /// <summary>
        /// Adds a Splunk HTTP Event Collector logger named 'Splunk' to the factory using the specified configuration. <br />
        /// The options from the existing Configuration will be used.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="endpoint">The splunk endpoint type to use, Raw events are only supported in Splunk Enterprise 6.4.0, Splunk Light 6.4.0 and later as well as Splunk Cloud.</param>
        /// <param name="payloadCreator">The delegate that returns an object to pass to the event end point.  Is the endpoint type is <see cref="SplunkEndpoint.Json"/> the object is serialized as Json, if  <see cref="SplunkEndpoint.Json"/> then  <see cref="object.ToString"/> is called.</param>
        /// <returns>The Logging builder being configured.</returns>
        public static ILoggingBuilder AddSplunk(this ILoggingBuilder builder, SplunkEndpoint endpoint, Func<LogData, object> payloadCreator)
        {
            return builder.AddSplunk(endpoint, c => { }, payloadCreator);
        }

        /// <summary>
        /// Adds a Splunk HTTP Event Collector logger named 'Splunk' to the factory using the specified configuration.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="endpoint">The splunk endpoint type to use, Raw events are only supported in Splunk Enterprise 6.4.0, Splunk Light 6.4.0 and later as well as Splunk Cloud.</param>
        /// <param name="configure">The delegate that configures the LoggerFactory.</param>
        /// <param name="payloadCreator">The delegate that returns an object to pass to the event end point.  Is the endpoint type is <see cref="SplunkEndpoint.Json"/> the object is serialized as Json, if  <see cref="SplunkEndpoint.Json"/> then  <see cref="object.ToString"/> is called.</param>
        /// <returns>The Logging builder being configured.</returns>
        public static ILoggingBuilder AddSplunk(this ILoggingBuilder builder, SplunkEndpoint endpoint, Action<SplunkLoggerOptions> configure, Func<LogData, object> payloadCreator)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            switch (endpoint)
            {
                case SplunkEndpoint.Json:
                    builder.AddSplunkJson(configure, payloadCreator);
                    break;
                case SplunkEndpoint.Raw:
                    builder.AddSplunkRaw(configure, payloadCreator);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, null);
            }

            builder.AddSplunkJson(payloadCreator);
            builder.Services.Configure(configure);

            return builder;
        }

        private static ILoggingBuilder AddSplunkJson(this ILoggingBuilder builder, Func<LogData, object> payloadCreator = null)
        {
            builder.AddConfiguration();

            builder.Services.AddSingleton<ISplunkJsonPayloadTransformer>(
                s =>
                    payloadCreator == null
                        ? new SplunkJsonPayloadTransformer(p => p)
                        : new SplunkJsonPayloadTransformer(payloadCreator));

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SplunkJsonLoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<SplunkLoggerOptions>, LoggerProviderConfigureOptions<SplunkLoggerOptions, SplunkJsonLoggerProvider>>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<SplunkLoggerOptions>, LoggerProviderOptionsChangeTokenSource<SplunkLoggerOptions, SplunkJsonLoggerProvider>>());

            return builder;
        }

        private static ILoggingBuilder AddSplunkJson(this ILoggingBuilder builder, Action<SplunkLoggerOptions> configure, Func<LogData, object> payloadCreator = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddSplunkJson(payloadCreator);
            builder.Services.Configure(configure);

            return builder;
        }

        private static ILoggingBuilder AddSplunkRaw(this ILoggingBuilder builder, Func<LogData, object> payloadCreator = null)
        {
            builder.AddConfiguration();

            builder.Services.AddSingleton<ISplunkRawPayloadTransformer>(
                s =>
                    payloadCreator == null
                        ? new SplunkRawPayloadTransformer()
                        : new SplunkRawPayloadTransformer(payloadCreator));

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SplunkRawLoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<SplunkLoggerOptions>, LoggerProviderConfigureOptions<SplunkLoggerOptions, SplunkRawLoggerProvider>>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<SplunkLoggerOptions>, LoggerProviderOptionsChangeTokenSource<SplunkLoggerOptions, SplunkRawLoggerProvider>>());

            return builder;
        }

        private static ILoggingBuilder AddSplunkRaw(this ILoggingBuilder builder, Action<SplunkLoggerOptions> configure, Func<LogData, object> payloadCreator = null)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddSplunkRaw(payloadCreator);
            builder.Services.Configure(configure);

            return builder;
        }
    }
}
