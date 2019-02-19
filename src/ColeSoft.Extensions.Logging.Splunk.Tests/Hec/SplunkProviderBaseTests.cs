using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using ColeSoft.Extensions.Logging.Splunk.Hec;
using ColeSoft.Extensions.Logging.Splunk.Hec.Raw;
using ColeSoft.Extensions.Logging.Splunk.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

#pragma warning disable CC0022 // Should dispose object

namespace ColeSoft.Extensions.Logging.Splunk.Tests.Hec
{
    public class SplunkProviderBaseTests
    {
        [Fact]
        public void SetScopeProvider_WhenInvoked_SetsProvider()
        {
            // arrange
            var httpClientFactory = Mock.Of<IHttpClientProvider>();

            Mock.Get(httpClientFactory).Setup(s => s.CreateClient()).Returns(new HttpClient());

            var optionsMonitor = Mock.Of<IOptionsMonitor<SplunkLoggerOptions>>();
            Mock.Get(optionsMonitor).SetupGet(s => s.CurrentValue)
                .Returns(new SplunkLoggerOptions
                    { SplunkCollectorUrl = "https://server/collector/events/", AuthenticationToken = "AuthToken" });
            Mock.Get(optionsMonitor).Setup(s => s.OnChange(It.IsAny<Action<SplunkLoggerOptions, string>>()))
                .Returns(Mock.Of<IDisposable>());
            var splunkLoggerProcessor = Mock.Of<ISplunkLoggerProcessor>();
            var rawPayloadTransformer = Mock.Of<ISplunkRawPayloadTransformer>();
            var externalScopeProvider = Mock.Of<IExternalScopeProvider>();
            Mock.Get(externalScopeProvider).Setup(s => s.Push("state1")).Verifiable();

            using (var actor = new SplunkRawLoggerProvider(httpClientFactory, optionsMonitor,  rawPayloadTransformer, splunkLoggerProcessor))
            {
                // act
                var cat1Logger = actor.CreateLogger("Cat1");
                actor.SetScopeProvider(externalScopeProvider);
                cat1Logger.BeginScope("state1");

                // assert
                Assert.Same(externalScopeProvider, actor.ScopeProvider);
                Mock.Get(externalScopeProvider).Verify();
            }
        }

        [Fact]
        public void SplunkLoggerOptions_WhenSplunkCollectorUrlDoesNotEndWithASlash_HasOneAdded()
        {
            // arrange
            var mockedHttpHandler = new MockHttpMessageHandler();
            var httpClientFactory = Mock.Of<IHttpClientProvider>();
            Mock.Get(httpClientFactory).Setup(s => s.CreateClient())
                .Returns(new HttpClient(mockedHttpHandler));
            var optionsMonitor = Mock.Of<IOptionsMonitor<SplunkLoggerOptions>>();
            Mock.Get(optionsMonitor).Setup(s => s.OnChange(It.IsAny<Action<SplunkLoggerOptions, string>>()))
                .Returns(Mock.Of<IDisposable>());
            var splunkLoggerProcessor = Mock.Of<ISplunkLoggerProcessor>();
            Mock.Get(optionsMonitor).SetupGet(s => s.CurrentValue)
                .Returns(new SplunkLoggerOptions
                    { SplunkCollectorUrl = "https://server/collector/events", AuthenticationToken = "AuthToken" });
            var rawPayloadTransformer = Mock.Of<ISplunkRawPayloadTransformer>();
            Mock.Get(rawPayloadTransformer).Setup(s => s.Transform(It.IsAny<LogData>())).Returns("Message");
            var externalScopeProvider = Mock.Of<IExternalScopeProvider>();
            Mock.Get(externalScopeProvider).Setup(s => s.Push("state1")).Verifiable();

            using (var actor = new TestLoggerProvider(httpClientFactory, optionsMonitor, splunkLoggerProcessor, "test"))
            {
                // act
                // assert
                Assert.StartsWith("https://server/collector/events/", actor.GetHttpClientForTestInspection().BaseAddress.ToString());
            }
        }

        [Fact]
        public void SplunkLoggerOptions_Always_HasCollectorEndpointSet()
        {
            // arrange
            var mockedHttpHandler = new MockHttpMessageHandler();
            var httpClientFactory = Mock.Of<IHttpClientProvider>();
            Mock.Get(httpClientFactory).Setup(s => s.CreateClient())
                .Returns(new HttpClient(mockedHttpHandler));
            var optionsMonitor = Mock.Of<IOptionsMonitor<SplunkLoggerOptions>>();
            Mock.Get(optionsMonitor).Setup(s => s.OnChange(It.IsAny<Action<SplunkLoggerOptions, string>>()))
                .Returns(Mock.Of<IDisposable>());
            var splunkLoggerProcessor = Mock.Of<ISplunkLoggerProcessor>();
            Mock.Get(splunkLoggerProcessor).Setup(s => s.EnqueueMessage(It.IsAny<string>()));
            Mock.Get(optionsMonitor).SetupGet(s => s.CurrentValue)
                .Returns(new SplunkLoggerOptions
                    { SplunkCollectorUrl = "https://server/collector/events", AuthenticationToken = "AuthToken" });
            var rawPayloadTransformer = Mock.Of<ISplunkRawPayloadTransformer>();
            Mock.Get(rawPayloadTransformer).Setup(s => s.Transform(It.IsAny<LogData>())).Returns("Message");
            var externalScopeProvider = Mock.Of<IExternalScopeProvider>();
            Mock.Get(externalScopeProvider).Setup(s => s.Push("state1")).Verifiable();

            using (var actor = new TestLoggerProvider(httpClientFactory, optionsMonitor, splunkLoggerProcessor, "test"))
            {
                // act
                // assert
                Assert.StartsWith("https://server/collector/events/test", actor.GetHttpClientForTestInspection().BaseAddress.ToString());
            }
        }

        [Fact]
        public void SplunkLoggerOptions_WhenHaChannelIdOptionQueryString_SetsGuidInQueryParameter()
        {
            // arrange
            var mockedHttpHandler = new MockHttpMessageHandler();
            var httpClientFactory = Mock.Of<IHttpClientProvider>();
            Mock.Get(httpClientFactory).Setup(s => s.CreateClient())
                .Returns(new HttpClient(mockedHttpHandler));
            var optionsMonitor = Mock.Of<IOptionsMonitor<SplunkLoggerOptions>>();
            Mock.Get(optionsMonitor).Setup(s => s.OnChange(It.IsAny<Action<SplunkLoggerOptions, string>>()))
                .Returns(Mock.Of<IDisposable>());
            var splunkLoggerProcessor = Mock.Of<ISplunkLoggerProcessor>();
            Mock.Get(optionsMonitor).SetupGet(s => s.CurrentValue)
                .Returns(
                    new SplunkLoggerOptions
                    {
                        SplunkCollectorUrl = "https://server/collector/events",
                        AuthenticationToken = "AuthToken",
                        ChannelIdType = SplunkLoggerOptions.ChannelIdOption.QueryString
                    });
            var rawPayloadTransformer = Mock.Of<ISplunkRawPayloadTransformer>();
            Mock.Get(rawPayloadTransformer).Setup(s => s.Transform(It.IsAny<LogData>())).Returns("Message");
            var externalScopeProvider = Mock.Of<IExternalScopeProvider>();
            Mock.Get(externalScopeProvider).Setup(s => s.Push("state1")).Verifiable();

            using (var actor = new TestLoggerProvider(httpClientFactory, optionsMonitor, splunkLoggerProcessor, "test"))
            {
                // act
                // assert
                Assert.Contains("?channel=", actor.GetHttpClientForTestInspection().BaseAddress.ToString());
            }
        }

        [Fact]
        public void SplunkLoggerOptions_WhenUseAuthTokenAsQueryStringIsTrue_SetsTokenInQueryParameterAtStart()
        {
            // arrange
            var mockedHttpHandler = new MockHttpMessageHandler();
            var httpClientFactory = Mock.Of<IHttpClientProvider>();
            Mock.Get(httpClientFactory).Setup(s => s.CreateClient())
                .Returns(new HttpClient(mockedHttpHandler));
            var optionsMonitor = Mock.Of<IOptionsMonitor<SplunkLoggerOptions>>();
            Mock.Get(optionsMonitor).Setup(s => s.OnChange(It.IsAny<Action<SplunkLoggerOptions, string>>()))
                .Returns(Mock.Of<IDisposable>());
            var splunkLoggerProcessor = Mock.Of<ISplunkLoggerProcessor>();
            Mock.Get(optionsMonitor).SetupGet(s => s.CurrentValue)
                .Returns(
                    new SplunkLoggerOptions
                    {
                        SplunkCollectorUrl = "https://server/collector/events",
                        AuthenticationToken = "AuthToken",
                        ChannelIdType = SplunkLoggerOptions.ChannelIdOption.QueryString,
                        UseAuthTokenAsQueryString = true
                    });
            var rawPayloadTransformer = Mock.Of<ISplunkRawPayloadTransformer>();
            Mock.Get(rawPayloadTransformer).Setup(s => s.Transform(It.IsAny<LogData>())).Returns("Message");
            var externalScopeProvider = Mock.Of<IExternalScopeProvider>();
            Mock.Get(externalScopeProvider).Setup(s => s.Push("state1")).Verifiable();

            using (var actor = new TestLoggerProvider(httpClientFactory, optionsMonitor, splunkLoggerProcessor, "test"))
            {
                // act
                // assert
                Assert.Contains("&token=AuthToken", actor.GetHttpClientForTestInspection().BaseAddress.ToString());
            }
        }

        [Fact]
        public void SplunkLoggerOptions_WhenUseAuthTokenAsQueryStringIsTrue_SetsTokenInQueryParameterInMiddle()
        {
            // arrange
            var mockedHttpHandler = new MockHttpMessageHandler();
            var httpClientFactory = Mock.Of<IHttpClientProvider>();
            Mock.Get(httpClientFactory).Setup(s => s.CreateClient())
                .Returns(new HttpClient(mockedHttpHandler));
            var optionsMonitor = Mock.Of<IOptionsMonitor<SplunkLoggerOptions>>();
            Mock.Get(optionsMonitor).Setup(s => s.OnChange(It.IsAny<Action<SplunkLoggerOptions, string>>()))
                .Returns(Mock.Of<IDisposable>());
            var splunkLoggerProcessor = Mock.Of<ISplunkLoggerProcessor>();
            Mock.Get(optionsMonitor).SetupGet(s => s.CurrentValue)
                .Returns(
                    new SplunkLoggerOptions
                    {
                        SplunkCollectorUrl = "https://server/collector/events",
                        AuthenticationToken = "AuthToken",
                        UseAuthTokenAsQueryString = true
                    });
            var rawPayloadTransformer = Mock.Of<ISplunkRawPayloadTransformer>();
            Mock.Get(rawPayloadTransformer).Setup(s => s.Transform(It.IsAny<LogData>())).Returns("Message");
            var externalScopeProvider = Mock.Of<IExternalScopeProvider>();
            Mock.Get(externalScopeProvider).Setup(s => s.Push("state1")).Verifiable();

            using (var actor = new TestLoggerProvider(httpClientFactory, optionsMonitor, splunkLoggerProcessor, "test"))
            {
                // act
                // assert
                Assert.Contains("?token=AuthToken", actor.GetHttpClientForTestInspection().BaseAddress.ToString());
            }
        }

        [Fact]
        public void SplunkLoggerOptions_WhenTimeoutGreaterThanZero_SetsTimeout()
        {
            // arrange
            var mockedHttpHandler = new MockHttpMessageHandler();
            var httpClientFactory = Mock.Of<IHttpClientProvider>();
            Mock.Get(httpClientFactory).Setup(s => s.CreateClient())
                .Returns(new HttpClient(mockedHttpHandler));
            var optionsMonitor = Mock.Of<IOptionsMonitor<SplunkLoggerOptions>>();
            Mock.Get(optionsMonitor).Setup(s => s.OnChange(It.IsAny<Action<SplunkLoggerOptions, string>>()))
                .Returns(Mock.Of<IDisposable>());
            var splunkLoggerProcessor = Mock.Of<ISplunkLoggerProcessor>();
            Mock.Get(optionsMonitor).SetupGet(s => s.CurrentValue)
                .Returns(
                    new SplunkLoggerOptions
                    {
                        SplunkCollectorUrl = "https://server/collector/events",
                        AuthenticationToken = "AuthToken",
                        Timeout = 1000
                    });
            var rawPayloadTransformer = Mock.Of<ISplunkRawPayloadTransformer>();
            Mock.Get(rawPayloadTransformer).Setup(s => s.Transform(It.IsAny<LogData>())).Returns("Message");
            var externalScopeProvider = Mock.Of<IExternalScopeProvider>();
            Mock.Get(externalScopeProvider).Setup(s => s.Push("state1")).Verifiable();

            using (var actor = new TestLoggerProvider(httpClientFactory, optionsMonitor, splunkLoggerProcessor, "test"))
            {
                // act

                // assert
                Assert.Equal(TimeSpan.FromMilliseconds(1000), actor.GetHttpClientForTestInspection().Timeout);
            }
        }

        [Fact]
        public void SplunkLoggerOptions_WhenCustomHeadersAreSet_AddsHeaders()
        {
            // arrange
            var mockedHttpHandler = new MockHttpMessageHandler();
            var httpClientFactory = Mock.Of<IHttpClientProvider>();
            Mock.Get(httpClientFactory).Setup(s => s.CreateClient())
                .Returns(new HttpClient(mockedHttpHandler));
            var optionsMonitor = Mock.Of<IOptionsMonitor<SplunkLoggerOptions>>();
            Mock.Get(optionsMonitor).Setup(s => s.OnChange(It.IsAny<Action<SplunkLoggerOptions, string>>()))
                .Returns(Mock.Of<IDisposable>());
            var splunkLoggerProcessor = Mock.Of<ISplunkLoggerProcessor>();
            Mock.Get(optionsMonitor).SetupGet(s => s.CurrentValue)
                .Returns(
                    new SplunkLoggerOptions
                    {
                        SplunkCollectorUrl = "https://server/collector/events",
                        AuthenticationToken = "AuthToken",
                        CustomHeaders = new Dictionary<string, string> { { "custom1", "value1" } }
                    });
            var rawPayloadTransformer = Mock.Of<ISplunkRawPayloadTransformer>();
            Mock.Get(rawPayloadTransformer).Setup(s => s.Transform(It.IsAny<LogData>())).Returns("Message");
            var externalScopeProvider = Mock.Of<IExternalScopeProvider>();
            Mock.Get(externalScopeProvider).Setup(s => s.Push("state1")).Verifiable();

            using (var actor = new TestLoggerProvider(httpClientFactory, optionsMonitor, splunkLoggerProcessor, "test"))
            {
                // act
                // assert
                Assert.Single(actor.GetHttpClientForTestInspection()
                    .DefaultRequestHeaders
                    .Where(h => h.Key == "custom1" && h.Value.Single() == "value1"));
            }
        }

        [Fact]
        public void SplunkLoggerOptions_WhenChannelIdOptionRequestHeader_AddsSplunkHeaders()
        {
            // arrange
            var mockedHttpHandler = new MockHttpMessageHandler();
            var httpClientFactory = Mock.Of<IHttpClientProvider>();
            Mock.Get(httpClientFactory).Setup(s => s.CreateClient())
                .Returns(new HttpClient(mockedHttpHandler));
            var optionsMonitor = Mock.Of<IOptionsMonitor<SplunkLoggerOptions>>();
            Mock.Get(optionsMonitor).Setup(s => s.OnChange(It.IsAny<Action<SplunkLoggerOptions, string>>()))
                .Returns(Mock.Of<IDisposable>());
            var splunkLoggerProcessor = Mock.Of<ISplunkLoggerProcessor>();
            Mock.Get(optionsMonitor).SetupGet(s => s.CurrentValue)
                .Returns(
                    new SplunkLoggerOptions
                    {
                        SplunkCollectorUrl = "https://server/collector/events",
                        AuthenticationToken = "AuthToken",
                        ChannelIdType = SplunkLoggerOptions.ChannelIdOption.RequestHeader
                    });
            var rawPayloadTransformer = Mock.Of<ISplunkRawPayloadTransformer>();
            Mock.Get(rawPayloadTransformer).Setup(s => s.Transform(It.IsAny<LogData>())).Returns("Message");
            var externalScopeProvider = Mock.Of<IExternalScopeProvider>();
            Mock.Get(externalScopeProvider).Setup(s => s.Push("state1")).Verifiable();

            using (var actor = new TestLoggerProvider(httpClientFactory, optionsMonitor, splunkLoggerProcessor, "test"))
            {
                // act
                var headers = actor.GetHttpClientForTestInspection()
                    .DefaultRequestHeaders
                    .Where(h => h.Key == "x-splunk-request-channel" && Guid.TryParse(h.Value.Single(), out _));

                // assert
                Assert.Single(headers);
            }
        }
    }
}
