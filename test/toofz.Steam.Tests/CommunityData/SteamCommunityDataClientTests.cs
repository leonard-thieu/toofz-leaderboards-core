﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using RichardSzalay.MockHttp;
using toofz.Steam.CommunityData;
using toofz.Steam.Tests.Properties;
using Xunit;

namespace toofz.Steam.Tests.CommunityData
{
    public class SteamCommunityDataClientTests
    {
        public SteamCommunityDataClientTests()
        {
            steamCommunityDataClient = new SteamCommunityDataClient(handler, telemetryClient);
        }

        private MockHttpMessageHandler handler = new MockHttpMessageHandler();
        private TelemetryClient telemetryClient = new TelemetryClient();
        private SteamCommunityDataClient steamCommunityDataClient;

        public class IsTransientMethod
        {
            [DisplayTheory(nameof(HttpRequestStatusException), nameof(HttpRequestStatusException.StatusCode))]
            [InlineData(408)]
            [InlineData(429)]
            [InlineData(500)]
            [InlineData(502)]
            [InlineData(503)]
            [InlineData(504)]
            public void ExIsHttpRequestStatusExceptionAndStatusCodeIsTransient_ReturnsTrue(HttpStatusCode statusCode)
            {
                // Arrange
                var ex = new HttpRequestStatusException(statusCode, new Uri("http://example.org"));

                // Act
                var isTransient = SteamCommunityDataClient.IsTransient(ex);

                // Assert
                Assert.True(isTransient);
            }

            [DisplayFact(nameof(HttpRequestStatusException), nameof(HttpRequestStatusException.StatusCode))]
            public void ExIsHttpRequestStatusExceptionAndStatusCodeIsNotTransient_ReturnsFalse()
            {
                // Arrange
                var statusCode = HttpStatusCode.Forbidden;
                var ex = new HttpRequestStatusException(statusCode, new Uri("http://example.org"));

                // Act
                var isTransient = SteamCommunityDataClient.IsTransient(ex);

                // Assert
                Assert.False(isTransient);
            }

            [DisplayTheory(nameof(IOException), nameof(IOException.InnerException), nameof(SocketException), nameof(SocketException.SocketErrorCode))]
            [InlineData(SocketError.ConnectionReset)]
            [InlineData(SocketError.TimedOut)]
            public void ExIsIOExceptionAndInnerExceptionIsSocketExceptionAndSocketErrorCodeIsTransient_ReturnsTrue(int errorCode)
            {
                // Arrange
                var innerException = new SocketException(errorCode);
                var ex = new IOException(null, innerException);

                // Act
                var isTransient = SteamCommunityDataClient.IsTransient(ex);

                // Assert
                Assert.True(isTransient);
            }

            [DisplayFact(nameof(IOException), nameof(IOException.InnerException), nameof(SocketException), nameof(SocketException.SocketErrorCode))]
            public void ExIsIOExceptionAndInnerExceptionIsSocketExceptionAndSocketErrorCodeIsNotTransient_ReturnsFalse()
            {
                // Arrange
                var innerException = new SocketException((int)SocketError.SocketNotSupported);
                var ex = new IOException(null, innerException);

                // Act
                var isTransient = SteamCommunityDataClient.IsTransient(ex);

                // Assert
                Assert.False(isTransient);
            }

            [DisplayFact(nameof(IOException), nameof(IOException.InnerException), nameof(SocketException))]
            public void ExIsIOExceptionAndInnerExceptionIsNotSocketException_ReturnsFalse()
            {
                // Arrange
                var ex = new IOException(null);

                // Act
                var isTransient = SteamCommunityDataClient.IsTransient(ex);

                // Assert
                Assert.False(isTransient);
            }

            [DisplayTheory(nameof(HttpRequestException), nameof(HttpRequestException.InnerException), nameof(WebException), nameof(WebException.Status))]
            [InlineData(WebExceptionStatus.ConnectFailure)]
            [InlineData(WebExceptionStatus.SendFailure)]
            [InlineData(WebExceptionStatus.PipelineFailure)]
            [InlineData(WebExceptionStatus.RequestCanceled)]
            [InlineData(WebExceptionStatus.ConnectionClosed)]
            [InlineData(WebExceptionStatus.KeepAliveFailure)]
            [InlineData(WebExceptionStatus.UnknownError)]
            public void ExIsHttpRequestExceptionAndInnerExceptionIsWebExceptionAndStatusIsTransient_ReturnsTrue(WebExceptionStatus status)
            {
                // Arrange
                var inner = new WebException(null, status);
                var ex = new HttpRequestException(null, inner);

                // Act
                var isTransient = SteamCommunityDataClient.IsTransient(ex);

                // Assert
                Assert.True(isTransient);
            }

            [DisplayFact(nameof(HttpRequestException), nameof(HttpRequestException.InnerException), nameof(WebException), nameof(WebException.Status))]
            public void ExIsHttpRequestExceptionAndInnerExceptionIsWebExceptionAndStatusIsNotTransient_ReturnsFalse()
            {
                // Arrange
                var status = WebExceptionStatus.NameResolutionFailure;
                var inner = new WebException(null, status);
                var ex = new HttpRequestException(null, inner);

                // Act
                var isTransient = SteamCommunityDataClient.IsTransient(ex);

                // Assert
                Assert.False(isTransient);
            }

            [DisplayFact(nameof(HttpRequestException), nameof(HttpRequestException.InnerException), nameof(WebException))]
            public void ExIsHttpRequestExceptionAndInnerExceptionIsNotWebException_ReturnsFalse()
            {
                // Arrange
                var inner = new Exception();
                var ex = new HttpRequestException(null, inner);

                // Act
                var isTransient = SteamCommunityDataClient.IsTransient(ex);

                // Assert
                Assert.False(isTransient);
            }

            [DisplayFact]
            public void ReturnsFalse()
            {
                // Arrange
                var ex = new Exception();

                // Act
                var isTransient = SteamCommunityDataClient.IsTransient(ex);

                // Assert
                Assert.False(isTransient);
            }
        }

        public class Constructor
        {
            [DisplayFact(nameof(SteamCommunityDataClient))]
            public void ReturnsSteamCommunityDataClient()
            {
                // Arrange
                var handler = new MockHttpMessageHandler();
                var telemetryClient = new TelemetryClient();

                // Act
                var client = new SteamCommunityDataClient(handler, telemetryClient);

                // Assert
                Assert.IsAssignableFrom<SteamCommunityDataClient>(client);
            }
        }

        public class GetLeaderboardsAsyncMethod : SteamCommunityDataClientTests
        {
            [DisplayFact(nameof(ObjectDisposedException))]
            public async Task Disposed_ThrowsObjectDisposedException()
            {
                // Arrange
                steamCommunityDataClient.Dispose();
                var communityGameName = 247080U.ToString();

                // Act -> Assert
                await Assert.ThrowsAsync<ObjectDisposedException>(() =>
                {
                    return steamCommunityDataClient.GetLeaderboardsAsync(communityGameName);
                });
            }

            [DisplayFact(nameof(ArgumentNullException))]
            public async Task CommunityGameNameIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string communityGameName = null;

                // Act -> Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() =>
                {
                    return steamCommunityDataClient.GetLeaderboardsAsync(communityGameName);
                });
            }

            [DisplayFact]
            public async Task ReturnsLeaderboards()
            {
                // Arrange
                handler
                    .When("http://steamcommunity.com/stats/247080/leaderboards/?xml=1")
                    .Respond(new StringContent(Resources.Leaderboards, Encoding.UTF8, "text/xml"));
                var communityGameName = 247080U.ToString();

                // Act
                var leaderboardsEnvelope = await steamCommunityDataClient.GetLeaderboardsAsync(communityGameName);

                // Assert
                Assert.Equal(411, leaderboardsEnvelope.Leaderboards.Count);
                var leaderboard = leaderboardsEnvelope.Leaderboards.First();
                Assert.Equal("http://steamcommunity.com/stats/247080/leaderboards/2047387/?xml=1", leaderboard.Url);
                Assert.Equal(2047387, leaderboard.LeaderboardId);
                Assert.Equal("DLC HARDCORE All Chars DLC_PROD", leaderboard.Name);
                Assert.Equal("All Characters (DLC) Score (Amplified)", leaderboard.DisplayName);
                Assert.Equal(317, leaderboard.EntryCount);
                Assert.Equal(2, leaderboard.SortMethod);
                Assert.Equal(1, leaderboard.DisplayType);
            }
        }

        public class GetLeaderboardEntriesAsyncMethod : SteamCommunityDataClientTests
        {
            [DisplayFact(nameof(ObjectDisposedException))]
            public async Task Disposed_ThrowsObjectDisposedException()
            {
                // Arrange
                steamCommunityDataClient.Dispose();
                var communityGameName = 247080U.ToString();
                var leaderboardId = 2047387;

                // Act -> Assert
                await Assert.ThrowsAsync<ObjectDisposedException>(() =>
                {
                    return steamCommunityDataClient.GetLeaderboardEntriesAsync(communityGameName, leaderboardId);
                });
            }

            [DisplayFact(nameof(ArgumentNullException))]
            public async Task CommunityGameNameIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string communityGameName = null;
                var leaderboardId = 2047387;

                // Act -> Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() =>
                {
                    return steamCommunityDataClient.GetLeaderboardEntriesAsync(communityGameName, leaderboardId);
                });
            }

            [DisplayFact]
            public async Task ReturnsLeaderboardEntries()
            {
                // Arrange
                handler
                    .When("http://steamcommunity.com/stats/247080/leaderboards/2047387/?xml=1")
                    .Respond(new StringContent(Resources.LeaderboardEntries, Encoding.UTF8, "text/xml"));
                var communityGameName = 247080U.ToString();
                var leaderboardId = 2047387;

                // Act
                var leaderboardEntriesEnvelope = await steamCommunityDataClient.GetLeaderboardEntriesAsync(communityGameName, leaderboardId);

                // Assert
                var entries = leaderboardEntriesEnvelope.Entries;
                Assert.Equal(317, entries.Count);
                var entry = entries.First();
                Assert.Equal(76561197998799529, entry.SteamId);
                Assert.Equal(134377, entry.Score);
                Assert.Equal(1, entry.Rank);
                Assert.Equal(849347241492683863UL, entry.UgcId);
                Assert.Equal("0b00000001000000", entry.Details);
            }
        }

        public class DisposeMethod
        {
            private SimpleHttpMessageHandler handler = new SimpleHttpMessageHandler();
            private TelemetryClient telemetryClient = new TelemetryClient();

            [DisplayFact(nameof(HttpClient))]
            public void DisposesHttpClient()
            {
                // Arrange
                var client = new SteamCommunityDataClient(handler, true, telemetryClient);

                // Act
                client.Dispose();

                // Assert
                Assert.Equal(1, handler.DisposeCount);
            }

            [DisplayFact(nameof(HttpClient))]
            public void DisposeMoreThanOnce_OnlyDisposesHttpClientOnce()
            {
                // Arrange
                var client = new SteamCommunityDataClient(handler, true, telemetryClient);

                // Act
                client.Dispose();
                client.Dispose();

                // Assert
                Assert.Equal(1, handler.DisposeCount);
            }
        }
    }
}
