// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.Marathon
{
    public static class MarathonMaps
    {
        public static PlaylistItem[] ToPlaylistItems(IEnumerable<APIMarathonMap> maps) =>
            maps.OrderBy(m => m.PlaylistOrder).Select((map, index) => new PlaylistItem(new APIBeatmap
            {
                OnlineID = map.BeatmapId,
                Checksum = map.Checksum ?? string.Empty,
                StarRating = map.StarRating,
            })
            {
                ID = index + 1,
                RulesetID = map.RulesetId,
                RequiredMods = map.RequiredMods,
            }).ToArray();
    }
}
