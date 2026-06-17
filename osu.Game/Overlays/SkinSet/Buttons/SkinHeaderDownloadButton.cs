using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet.Buttons;
using osu.Game.Overlays.SkinListing.Drawables.Cards;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Overlays.SkinSet.Buttons
{
    public partial class SkinHeaderDownloadButton : CompositeDrawable
    {
        private const int text_size = 12;

        private readonly APIOnlineSkin skin;
        private readonly SkinDownloadTracker downloadTracker;

        private ShakeContainer shakeContainer = null!;
        private HeaderButton button = null!;
        private FillFlowContainer textSprites = null!;

        private readonly IBindable<APIUser> localUser = new Bindable<APIUser>();

        public SkinHeaderDownloadButton(APIOnlineSkin skin, SkinDownloadTracker downloadTracker)
        {
            this.skin = skin;
            this.downloadTracker = downloadTracker;

            Width = 120;
            RelativeSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, SkinDownloader skinDownloader)
        {
            InternalChildren = new Drawable[]
            {
                shakeContainer = new ShakeContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 5,
                    Child = button = new HeaderButton { RelativeSizeAxes = Axes.Both },
                },
            };

            button.AddRange(new Drawable[]
            {
                new Container
                {
                    Padding = new MarginPadding { Horizontal = 10 },
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        textSprites = new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            AutoSizeDuration = 500,
                            AutoSizeEasing = Easing.OutQuint,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "Download",
                                    Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                                },
                            }
                        },
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Icon = FontAwesome.Solid.Download,
                            Size = new Vector2(18),
                        },
                    }
                },
            });

            button.Action = () =>
            {
                if (downloadTracker.State.Value != DownloadState.NotDownloaded)
                {
                    shakeContainer.Shake();
                    return;
                }

                skinDownloader.DownloadAndImport(skin);
            };

            localUser.BindTo(api.LocalUser);
            localUser.BindValueChanged(userChanged, true);
            button.Enabled.BindValueChanged(enabledChanged, true);

            downloadTracker.State.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case DownloadState.Downloading:
                        textSprites.Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "Downloading...",
                                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                            },
                        };
                        break;

                    case DownloadState.Importing:
                        textSprites.Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "Importing...",
                                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                            },
                        };
                        break;

                    case DownloadState.LocallyAvailable:
                        this.FadeOut(200);
                        break;

                    case DownloadState.NotDownloaded:
                        textSprites.Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "Download",
                                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
                            },
                        };
                        this.FadeIn(200);
                        break;
                }
            }, true);
        }

        private void userChanged(ValueChangedEvent<APIUser> e) => button.Enabled.Value = !(e.NewValue is GuestUser);

        private void enabledChanged(ValueChangedEvent<bool> e) => this.FadeColour(e.NewValue ? Color4.White : Color4.Gray, 200, Easing.OutQuint);
    }
}
