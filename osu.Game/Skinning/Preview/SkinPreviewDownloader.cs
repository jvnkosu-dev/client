// Copyright (c) jvnkosu! team, MIT license
// See the LICENCE file in the repository root for full license text.

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace osu.Game.Skinning.Preview
{
    /// <summary>
    /// HTTP client for downloading skin archives for preview without incrementing download metrics.
    /// </summary>
    internal static class SkinPreviewDownloader
    {
        /// <summary>
        /// Header sent with preview downloads so the server can serve the archive without recording a download.
        /// </summary>
        public const string PREVIEW_HEADER = "X-Skin-Preview";

        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Attempts to download a skin archive for preview. Preview URL is tried first, then download URL as fallback.
        /// Both attempts include <see cref="PREVIEW_HEADER"/>.
        /// </summary>
        public static async Task<byte[]?> DownloadAsync(string? previewUrl, string? downloadUrl)
        {
            if (!string.IsNullOrWhiteSpace(previewUrl))
            {
                byte[]? data = await attemptDownload(previewUrl).ConfigureAwait(false);
                if (data != null)
                    return data;
            }

            if (!string.IsNullOrWhiteSpace(downloadUrl) && !string.Equals(previewUrl, downloadUrl, StringComparison.OrdinalIgnoreCase))
            {
                byte[]? data = await attemptDownload(downloadUrl).ConfigureAwait(false);
                if (data != null)
                    return data;
            }

            return null;
        }

        private static async Task<byte[]?> attemptDownload(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation(PREVIEW_HEADER, "1");

            try
            {
                using var response = await client.SendAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }
    }
}
