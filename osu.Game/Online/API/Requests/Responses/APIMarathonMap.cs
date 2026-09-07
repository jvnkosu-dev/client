// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIMarathonMap
    {
        [JsonProperty("beatmap_id")]
        public int BeatmapId { get; set; }

        [JsonProperty("checksum")]
        public string? Checksum { get; set; }

        [JsonProperty("ruleset_id")]
        public int RulesetId { get; set; }

        [JsonProperty("required_mods")]
        public APIMod[] RequiredMods { get; set; } = [];

        [JsonProperty("playlist_order")]
        public int PlaylistOrder { get; set; }

        [JsonProperty("star_rating")]
        public double StarRating { get; set; }
    }
}
