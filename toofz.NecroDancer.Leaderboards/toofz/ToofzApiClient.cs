﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Microsoft.ApplicationInsights;
using toofz.NecroDancer.Leaderboards.Logging;

namespace toofz.NecroDancer.Leaderboards.toofz
{
    public sealed class ToofzApiClient : IToofzApiClient
    {
        private static readonly ILog Log = LogProvider.GetLogger(typeof(ToofzApiClient));

        /// <summary>
        /// Initializes a new instance of the <see cref="ToofzApiClient"/> class with a specific handler.
        /// </summary>
        /// <param name="handler">The HTTP handler stack to use for sending requests.</param>
        /// <param name="disposeHandler">
        /// true if the inner handler should be disposed of by <see cref="Dispose"/>,
        /// false if you intend to reuse the inner handler.
        /// </param>
        /// <param name="telemetryClient">The telemetry client to use for reporting telemetry.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="telemetryClient"/> is null.
        /// </exception>
        public ToofzApiClient(HttpMessageHandler handler, bool disposeHandler, TelemetryClient telemetryClient)
        {
            http = new ProgressReporterHttpClient(handler, disposeHandler, telemetryClient);
        }

        private readonly ProgressReporterHttpClient http;

        /// <summary>
        /// Gets or sets the base address of Uniform Resource Identifier (URI) of the Internet 
        /// resource used when sending requests.
        /// </summary>
        /// <returns>
        /// Returns <see cref="Uri"/>.The base address of Uniform Resource Identifier (URI) of the 
        /// Internet resource used when sending requests.
        /// </returns>
        public Uri BaseAddress
        {
            get => http.BaseAddress;
            set => http.BaseAddress = value;
        }

        #region Players

        public async Task<PlayersEnvelope> GetPlayersAsync(
            GetPlayersParams @params = default,
            IProgress<long> progress = default,
            CancellationToken cancellationToken = default)
        {
            var requestUri = "players";
            requestUri = requestUri.SetQueryParams(new
            {
                q = @params.Query,
                offset = @params.Offset,
                limit = @params.Limit,
                sort = @params.Sort,
            });

            var response = await http.GetAsync("Get players to update", requestUri, progress, cancellationToken).ConfigureAwait(false);

            return await response.Content.ReadAsAsync<PlayersEnvelope>().ConfigureAwait(false);
        }

        public async Task<BulkStoreDTO> PostPlayersAsync(
            IEnumerable<Player> players,
            CancellationToken cancellationToken = default)
        {
            if (players == null)
                throw new ArgumentNullException(nameof(players));

            var response = await http.PostAsJsonAsync("players", players, cancellationToken).ConfigureAwait(false);

            return await response.Content.ReadAsAsync<BulkStoreDTO>().ConfigureAwait(false);
        }

        #endregion

        #region Replays

        public async Task<ReplaysEnvelope> GetReplaysAsync(
            GetReplaysParams @params = default,
            IProgress<long> progress = default,
            CancellationToken cancellationToken = default)
        {
            var requestUri = "replays";
            requestUri = requestUri.SetQueryParams(new
            {
                version = @params.Version,
                error = @params.ErrorCode,
                offset = @params.Offset,
                limit = @params.Limit,
            });

            var response = await http.GetAsync("Get replays to update", requestUri, progress, cancellationToken).ConfigureAwait(false);

            return await response.Content.ReadAsAsync<ReplaysEnvelope>().ConfigureAwait(false);
        }

        public async Task<BulkStoreDTO> PostReplaysAsync(
            IEnumerable<Replay> replays,
            CancellationToken cancellationToken = default)
        {
            if (replays == null)
                throw new ArgumentNullException(nameof(replays));

            var response = await http.PostAsJsonAsync("replays", replays, cancellationToken).ConfigureAwait(false);

            return await response.Content.ReadAsAsync<BulkStoreDTO>().ConfigureAwait(false);
        }

        #endregion

        #region IDisposable Implementation

        private bool disposed;

        public void Dispose()
        {
            if (disposed) { return; }

            http.Dispose();

            disposed = true;
        }

        #endregion
    }
}