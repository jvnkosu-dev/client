// Copyright (c) jvnkosu! team, MIT license
// See the LICENCE file in the repository root for full license text.

using Newtonsoft.Json;
using osu.Game.Skinning;

using System;

namespace osu.Game.Online.API.Requests
{
    public class APIOnlineSkin
    {
        [JsonProperty("id")]
        public int OnlineID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("creator")]
        public string Creator { get; set; } = string.Empty;

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; } = string.Empty;

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("version")]
        public string? Version { get; set; }

        // Поле для будущей системы обновлений (на сервере пока нет, но в клиенте подготовим)
        [JsonProperty("last_updated")]
        public DateTimeOffset? LastUpdated { get; set; }

        /// <summary>
        /// Returns the URL to fetch this skin's thumbnail image.
        /// Uses the API thumbnail route when available to avoid nginx static file permission issues.
        /// </summary>
        public string? GetThumbnailRequestUrl()
        {
            if (string.IsNullOrEmpty(ThumbnailUrl))
                return null;

            if (OnlineID > 0 && !string.IsNullOrEmpty(DownloadUrl))
            {
                const string download_segment = "/api/skins/download/";
                int index = DownloadUrl.IndexOf(download_segment, StringComparison.Ordinal);

                if (index >= 0)
                    return DownloadUrl[..index] + $"/api/skins/thumbnail/{OnlineID}";
            }

            return ThumbnailUrl;
        }
    }
}
