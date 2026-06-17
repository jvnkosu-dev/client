using Humanizer;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables.Cards.Statistics;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards.Statistics
{
    public partial class SkinDownloadCountStatistic : BeatmapCardStatistic
    {
        public SkinDownloadCountStatistic(APIOnlineSkin skin)
        {
            Icon = FontAwesome.Solid.Download;
            Text = skin.DownloadCount.ToMetric(decimals: 1);
            TooltipText = LocalisableString.Interpolate($"Downloads: {skin.DownloadCount.ToLocalisableString(@"N0")}");
        }
    }
}
