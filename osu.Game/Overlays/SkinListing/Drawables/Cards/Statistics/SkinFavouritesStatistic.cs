using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables.Cards.Statistics;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards.Statistics
{
    public partial class SkinFavouritesStatistic : BeatmapCardStatistic, IHasCurrentValue<SkinFavouriteState>
    {
        private readonly BindableWithCurrent<SkinFavouriteState> current;

        public Bindable<SkinFavouriteState> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public SkinFavouritesStatistic(APIOnlineSkin skin)
        {
            current = new BindableWithCurrent<SkinFavouriteState>(new SkinFavouriteState(skin.HasFavourited, skin.FavouriteCount));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            current.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            Icon = current.Value.Favourited ? FontAwesome.Solid.Heart : FontAwesome.Regular.Heart;
            Text = current.Value.FavouriteCount.ToMetric(decimals: 1);
            TooltipText = LocalisableString.Interpolate($"Favourited: {current.Value.FavouriteCount.ToLocalisableString(@"N0")}");
        }
    }
}
