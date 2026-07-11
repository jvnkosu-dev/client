using System;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables.Cards.Statistics;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Utils;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards.Statistics
{
    public partial class SkinCardDateStatistic : BeatmapCardStatistic
    {
        private readonly DateTimeOffset dateTime;

        private SkinCardDateStatistic(DateTimeOffset dateTime)
        {
            this.dateTime = dateTime;

            Icon = FontAwesome.Regular.Clock;
            Text = dateTime.ToLocalisedMediumDate();
            TooltipText = LocalisableString.Interpolate($"Updated: {dateTime.ToLocalisedMediumDate()}");
        }

        public override object TooltipContent => dateTime;
        public override ITooltip GetCustomTooltip() => new DateTooltip();

        public static SkinCardDateStatistic? CreateFor(APIOnlineSkin skin)
        {
            var displayDate = skin.LastUpdated ?? skin.CreatedAt;

            if (displayDate == null)
                return null;

            return new SkinCardDateStatistic(displayDate.Value);
        }
    }
}
