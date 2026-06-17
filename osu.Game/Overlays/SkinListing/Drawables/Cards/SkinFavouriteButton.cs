using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class SkinFavouriteButton : BeatmapCardIconButton, IHasCurrentValue<SkinFavouriteState>
    {
        private readonly BindableWithCurrent<SkinFavouriteState> current;
        private readonly APIOnlineSkin skin;

        private PostSkinFavouriteRequest? favouriteRequest;

        public Bindable<SkinFavouriteState> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public SkinFavouriteButton(APIOnlineSkin skin)
        {
            this.skin = skin;
            current = new BindableWithCurrent<SkinFavouriteState>(new SkinFavouriteState(skin.HasFavourited, skin.FavouriteCount));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            TooltipText = "Favourite";
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Action = toggleFavouriteStatus;
            current.BindValueChanged(_ => updateState(), true);
        }

        private void toggleFavouriteStatus()
        {
            var actionType = current.Value.Favourited ? SkinFavouriteAction.UnFavourite : SkinFavouriteAction.Favourite;

            favouriteRequest?.Cancel();
            favouriteRequest = new PostSkinFavouriteRequest(skin.OnlineID, actionType);

            SetLoading(true);

            favouriteRequest.Success += () =>
            {
                bool favourited = actionType == SkinFavouriteAction.Favourite;

                current.Value = new SkinFavouriteState(favourited, current.Value.FavouriteCount + (favourited ? 1 : -1));
                SetLoading(false);
            };
            favouriteRequest.Failure += e =>
            {
                Logger.Error(e, $"Failed to {actionType.ToString().ToLowerInvariant()} skin: {e.Message}");
                SetLoading(false);
            };

            api.Queue(favouriteRequest);
        }

        private void updateState()
        {
            if (current.Value.Favourited)
            {
                Icon.Icon = FontAwesome.Solid.Heart;
                TooltipText = "Unfavourite";
            }
            else
            {
                Icon.Icon = FontAwesome.Regular.Heart;
                TooltipText = "Favourite";
            }
        }
    }
}
