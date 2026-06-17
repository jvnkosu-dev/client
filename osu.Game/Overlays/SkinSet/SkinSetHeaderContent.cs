using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;
using osu.Game.Overlays.SkinListing.Drawables.Cards;
using osu.Game.Overlays.SkinSet.Buttons;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinSetHeaderContent : CompositeDrawable
    {
        public readonly Bindable<APIOnlineSkin?> Skin = new Bindable<APIOnlineSkin?>();

        private const float transition_duration = 200;
        private const float buttons_height = 45;
        private const float buttons_spacing = 5;

        private readonly Container coverContainer;
        private readonly Box coverGradient;
        private readonly MetadataFlowContainer title;
        private readonly MetadataFlowContainer artist;
        private readonly SkinAuthorInfo author;
        private readonly FillFlowContainer fadeContent;
        private readonly LoadingSpinner loading;
        private readonly SkinHeaderFavouriteButton favouriteButton;
        private readonly FillFlowContainer actionButtonsContainer;
        private readonly SkinSetStatistics statistics;

        private SkinDownloadTracker? downloadTracker;

        public SkinSetHeaderContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            coverContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                            },
                            coverGradient = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding
                        {
                            Vertical = SkinSetOverlay.Y_PADDING,
                            Left = WaveOverlayContainer.HORIZONTAL_PADDING,
                            Right = WaveOverlayContainer.HORIZONTAL_PADDING,
                        },
                        Children = new Drawable[]
                        {
                            fadeContent = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    statistics = new SkinSetStatistics
                                    {
                                        Margin = new MarginPadding { Bottom = 10 },
                                        Skin = { BindTarget = Skin },
                                    },
                                    title = new MetadataFlowContainer(s =>
                                    {
                                        s.Font = OsuFont.GetFont(size: 30, weight: FontWeight.SemiBold, italics: true);
                                    })
                                    {
                                        Margin = new MarginPadding { Top = 15 },
                                    },
                                    artist = new MetadataFlowContainer(s =>
                                    {
                                        s.Font = OsuFont.GetFont(size: 20, weight: FontWeight.Medium, italics: true);
                                    })
                                    {
                                        Margin = new MarginPadding { Bottom = 20 },
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Child = author = new SkinAuthorInfo(),
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = buttons_height,
                                        Margin = new MarginPadding { Top = 10 },
                                        Children = new Drawable[]
                                        {
                                            favouriteButton = new SkinHeaderFavouriteButton
                                            {
                                                Skin = { BindTarget = Skin }
                                            },
                                            actionButtonsContainer = new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding { Left = buttons_height + buttons_spacing },
                                                Spacing = new Vector2(buttons_spacing),
                                            },
                                        }
                                    },
                                },
                            },
                        }
                    },
                    loading = new LoadingSpinner
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(1.5f),
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            coverGradient.Colour = ColourInfo.GradientVertical(colourProvider.Background6.Opacity(0.3f), colourProvider.Background6.Opacity(0.8f));

            Skin.BindValueChanged(s => updateSkin(s.NewValue), true);
        }

        private void updateSkin(APIOnlineSkin? newSkin)
        {
            downloadTracker?.RemoveAndDisposeImmediately();
            actionButtonsContainer.Clear();

            if (newSkin == null)
            {
                fadeContent.Hide();
                favouriteButton.FadeOut(transition_duration);
                loading.Show();
                coverContainer.Clear();
                author.Skin = null;
                return;
            }

            fadeContent.FadeIn(500, Easing.OutQuint);
            loading.Hide();
            favouriteButton.FadeIn(transition_duration);

            title.Clear();
            title.AddText(SkinIniVersionHelper.GetDisplayName(newSkin.Name));

            artist.Clear();

            if (!string.IsNullOrWhiteSpace(newSkin.Creator))
                artist.AddText(newSkin.Creator);

            author.Skin = newSkin;

            coverContainer.Child = new OnlineSkinSprite(newSkin.GetThumbnailRequestUrl())
            {
                RelativeSizeAxes = Axes.Both,
            };

            downloadTracker = new SkinDownloadTracker(newSkin);
            downloadTracker.State.BindValueChanged(_ => updateActionButtons(newSkin));
            AddInternal(downloadTracker);

            updateActionButtons(newSkin);
        }

        private void updateActionButtons(APIOnlineSkin skin)
        {
            actionButtonsContainer.Clear();

            if (downloadTracker == null)
                return;

            switch (downloadTracker.State.Value)
            {
                case DownloadState.LocallyAvailable:
                    actionButtonsContainer.Add(new SkinSetDownloadButton(skin, downloadTracker)
                    {
                        Width = 50,
                        RelativeSizeAxes = Axes.Y,
                    });
                    break;

                case DownloadState.Downloading:
                case DownloadState.Importing:
                case DownloadState.NotDownloaded:
                    actionButtonsContainer.Add(new SkinHeaderDownloadButton(skin, downloadTracker));
                    break;
            }
        }

        public partial class MetadataFlowContainer : LinkFlowContainer
        {
            public MetadataFlowContainer(Action<SpriteText>? defaultCreationParameters = null)
                : base(defaultCreationParameters)
            {
                TextAnchor = Anchor.CentreLeft;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            protected override DrawableLinkCompiler CreateLinkCompiler(ITextPart textPart) => new MetadataLinkCompiler(textPart);

            public partial class MetadataLinkCompiler : DrawableLinkCompiler
            {
                public MetadataLinkCompiler(ITextPart part)
                    : base(part)
                {
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    IdleColour = Color4.White;
                }
            }
        }
    }
}
