using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet.Buttons;
using osu.Game.Overlays.Notifications;
using osuTK;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Overlays.SkinSet.Buttons
{
    public partial class SkinHeaderFavouriteButton : HeaderButton
    {
        public readonly Bindable<APIOnlineSkin?> Skin = new Bindable<APIOnlineSkin?>();

        private readonly BindableBool favourited = new BindableBool();

        private PostSkinFavouriteRequest? request;
        private LoadingLayer loading = null!;

        private readonly IBindable<APIUser> localUser = new Bindable<APIUser>();

        public override LocalisableString TooltipText
        {
            get
            {
                if (!Enabled.Value)
                    return string.Empty;

                return favourited.Value ? "Unfavourite" : "Favourite";
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(IAPIProvider api, INotificationOverlay notifications)
        {
            SpriteIcon icon;

            AddRange(new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Regular.Heart,
                    Size = new Vector2(18),
                    Shadow = false,
                },
                loading = new LoadingLayer(true, false),
            });

            Action = () =>
            {
                var skin = Skin.Value;

                if (skin == null || skin.OnlineID <= 0)
                    return;

                loading.Show();

                request?.Cancel();

                var action = favourited.Value ? SkinFavouriteAction.UnFavourite : SkinFavouriteAction.Favourite;
                request = new PostSkinFavouriteRequest(skin.OnlineID, action);

                request.Success += () =>
                {
                    bool newFavourited = action == SkinFavouriteAction.Favourite;
                    favourited.Value = newFavourited;
                    skin.HasFavourited = newFavourited;
                    skin.FavouriteCount += newFavourited ? 1 : -1;
                    loading.Hide();
                };

                request.Failure += e =>
                {
                    notifications?.Post(new SimpleNotification
                    {
                        Text = e.Message,
                        Icon = FontAwesome.Solid.Times,
                    });

                    Logger.Error(e, $"Failed to {action.ToString().ToLowerInvariant()} skin: {e.Message}");
                    loading.Hide();
                };

                api.Queue(request);
            };

            favourited.ValueChanged += favourited => icon.Icon = favourited.NewValue ? FontAwesome.Solid.Heart : FontAwesome.Regular.Heart;

            localUser.BindTo(api.LocalUser);
            localUser.BindValueChanged(_ => updateEnabled());

            Skin.BindValueChanged(setInfo =>
            {
                updateEnabled();
                favourited.Value = setInfo.NewValue?.HasFavourited ?? false;
            }, true);
        }

        private void updateEnabled() => Enabled.Value = !(localUser.Value is GuestUser) && Skin.Value?.OnlineID > 0;

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            Width = DrawHeight;
        }
    }
}
