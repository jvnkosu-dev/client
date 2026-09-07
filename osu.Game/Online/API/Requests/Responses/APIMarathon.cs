// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIMarathon
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("owner_username")]
        public string OwnerUsername { get; set; } = string.Empty;

        [JsonProperty("source_owner_id")]
        public int SourceOwnerId { get; set; }

        [JsonProperty("source_owner_username")]
        public string SourceOwnerUsername { get; set; } = string.Empty;

        [JsonProperty("tags")]
        public string[] Tags
        {
            get => tags;
            set => tags = value ?? [];
        }

        private string[] tags = [];

        [JsonProperty("rank_min")]
        public int RankMin { get; set; }

        [JsonProperty("rank_max")]
        public int RankMax { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("maps")]
        public APIMarathonMap[] Maps { get; set; } = [];

        public bool HasSourceCredit =>
            !string.IsNullOrEmpty(SourceOwnerUsername)
            && !string.Equals(SourceOwnerUsername, OwnerUsername, StringComparison.OrdinalIgnoreCase);
    }
}
