﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Flurl;

namespace toofz.NecroDancer.Leaderboards.Steam.CommunityData
{
    public sealed class SteamCommunityDataClient : ISteamCommunityDataClient
    {
        static readonly XmlSerializer LeaderboardsEnvelopeSerializer = new XmlSerializer(typeof(LeaderboardsEnvelope));
        static readonly XmlSerializer LeaderboardEntriesEnvelopeSerializer = new XmlSerializer(typeof(LeaderboardEntriesEnvelope));

        public SteamCommunityDataClient(HttpMessageHandler handler)
        {
            http = new ProgressReporterHttpClient(handler) { BaseAddress = new Uri("http://steamcommunity.com/") };
        }

        readonly ProgressReporterHttpClient http;

        #region GetLeaderboards

        public Task<LeaderboardsEnvelope> GetLeaderboardsAsync(
            uint appId,
            IProgress<long> progress = default,
            CancellationToken cancellationToken = default)
        {
            return GetLeaderboardsAsync(appId.ToString(), progress, cancellationToken);
        }

        public async Task<LeaderboardsEnvelope> GetLeaderboardsAsync(
            string communityGameName,
            IProgress<long> progress = default,
            CancellationToken cancellationToken = default)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(SteamCommunityDataClient));
            if (communityGameName == null)
                throw new ArgumentNullException(nameof(communityGameName));

            var requestUri = $"stats/{communityGameName}/leaderboards/"
                .SetQueryParams(new
                {
                    xml = 1,
                });
            var response = await http.GetAsync(requestUri, progress, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return (LeaderboardsEnvelope)LeaderboardsEnvelopeSerializer.Deserialize(content);
        }

        #endregion

        #region GetLeaderboardEntries

        /// <summary>
        /// The maximum number of leaderboard entries allowed per request.
        /// </summary>
        public const int MaxLeaderboardEntriesPerRequest = 5001;

        public Task<LeaderboardEntriesEnvelope> GetLeaderboardEntriesAsync(
            uint appId,
            int leaderboardId,
            GetLeaderboardEntriesParams @params = default,
            IProgress<long> progress = default,
            CancellationToken cancellationToken = default)
        {
            return GetLeaderboardEntriesAsync(appId.ToString(), leaderboardId, @params, progress, cancellationToken);
        }

        public async Task<LeaderboardEntriesEnvelope> GetLeaderboardEntriesAsync(
            string communityGameName,
            int leaderboardId,
            GetLeaderboardEntriesParams @params = default,
            IProgress<long> progress = default,
            CancellationToken cancellationToken = default)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(SteamCommunityDataClient));
            if (communityGameName == null)
                throw new ArgumentNullException(nameof(communityGameName));

            var requestUri = $"stats/{communityGameName}/leaderboards/{leaderboardId}/"
                .SetQueryParams(new
                {
                    xml = 1,
                    start = @params.StartRange,
                    end = @params.EndRange,
                });
            var response = await http.GetAsync(requestUri, progress, cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return (LeaderboardEntriesEnvelope)LeaderboardEntriesEnvelopeSerializer.Deserialize(content);
        }

        #endregion

        #region IDisposable Implementation

        bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                http.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
