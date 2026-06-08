using Newtonsoft.Json;

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

        // Поле для будущей системы обновлений (на сервере пока нет, но в клиенте подготовим)
        [JsonProperty("last_updated")]
        public DateTimeOffset? LastUpdated { get; set; }
    }
}
