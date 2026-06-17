// Copyright (c) jvnkosu! team, MIT license
// See the LICENCE file in the repository root for full license text.

using System.Collections.Generic;
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

        [JsonProperty("download_count")]
        public int DownloadCount { get; set; }

        [JsonProperty("favourite_count")]
        public int FavouriteCount { get; set; }

        [JsonProperty("has_favourited")]
        public bool HasFavourited { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("last_updated")]
        public DateTimeOffset? LastUpdated { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; } = string.Empty;

        [JsonProperty("uploaded_by")]
        public string UploadedBy { get; set; } = string.Empty;

        [JsonProperty("modified_modes")]
        public List<string> ModifiedModes { get; set; } = new List<string>();

        [JsonProperty("engine_type")]
        public string EngineType { get; set; } = string.Empty;

        /// <summary>
        /// Returns the username of the user who uploaded this skin to the server, if available.
        /// </summary>
        public string? GetUploaderDisplayName() => string.IsNullOrWhiteSpace(UploadedBy) ? null : UploadedBy.Trim();

        /// <summary>
        /// Returns the URL to fetch this skin's thumbnail image.
        /// Uses the API thumbnail route when available to avoid nginx static file permission issues.
        /// </summary>
        public string? GetThumbnailRequestUrl()
        {
            if (!string.IsNullOrEmpty(ThumbnailUrl))
                return ThumbnailUrl;

            if (OnlineID <= 0 || string.IsNullOrEmpty(DownloadUrl))
                return null;

            const string download_segment = "/api/skins/download/";
            int index = DownloadUrl.IndexOf(download_segment, StringComparison.Ordinal);

            if (index < 0)
                return null;

            return DownloadUrl[..index] + $"/api/skins/thumbnail/{OnlineID}";
        }
    }
}
