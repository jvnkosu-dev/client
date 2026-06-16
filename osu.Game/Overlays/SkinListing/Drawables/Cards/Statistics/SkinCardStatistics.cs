using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.Drawables.Cards.Statistics;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards.Statistics
{
    public static class SkinCardStatistics
    {
        public static IEnumerable<BeatmapCardStatistic> CreateFor(APIOnlineSkin skin, IBindable<SkinFavouriteState> favouriteState)
        {
            yield return new SkinDownloadCountStatistic(skin);
            yield return new SkinFavouritesStatistic(skin) { Current = { BindTarget = favouriteState } };

            var dateStatistic = SkinCardDateStatistic.CreateFor(skin);
            if (dateStatistic != null)
                yield return dateStatistic;
        }
    }
}
