// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections
{
    /// <summary>
    /// Song-select-style info card for the current skin: title/stats on top, Details tab below.
    /// </summary>
    public partial class SkinSettingsInfoCard : CompositeDrawable
    {
        private const float corner_radius = 10;
        private const float content_padding = 12;
        /// <summary>
        /// Extends wedges past the left edge (like song select's CORNER_RADIUS_HIDE_OFFSET)
        /// so the sheared left border is clipped out of view.
        /// </summary>
        private const float left_edge_extension = 20;

        private readonly Bindable<Skin> currentSkin = new Bindable<Skin>();

        private TruncatingSpriteText titleText = null!;
        private TruncatingSpriteText creatorText = null!;
        private StatisticPill downloadsPill = null!;
        private FavouritePill favouritesPill = null!;

        private MetadataField uploadedBy = null!;
        private MetadataField version = null!;
        private MetadataField submitted = null!;
        private MetadataField lastUpdated = null!;
        private MetadataField tags = null!;

        private int lookupGeneration;
        private CancellationTokenSource? lookupCancellation;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Resolved]
        private SkinLookupCache skinLookupCache { get; set; } = null!;

        [Resolved]
        private OsuGame? game { get; set; }

        public SkinSettingsInfoCard()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding
            {
                Left = SettingsPanel.CONTENT_PADDING.Left - left_edge_extension,
                Right = SettingsPanel.CONTENT_PADDING.Right,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            // Match song select wedges: shear the stack, counter-shear content so text stays upright.
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 4),
                Shear = OsuGame.SHEAR,
                Children = new Drawable[]
                {
                    new ShearAligningWrapper(createTitleCard(colourProvider)),
                    new ShearAligningWrapper(createDetailsHeader()),
                    new ShearAligningWrapper(createDetailsCard()),
                    new ShearAligningWrapper(createActionButtons()),
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentSkin.BindTo(skins.CurrentSkin);
            currentSkin.BindValueChanged(_ => updateDisplay(), true);
        }

        protected override void Dispose(bool isDisposing)
        {
            lookupCancellation?.Cancel();
            lookupCancellation?.Dispose();
            base.Dispose(isDisposing);
        }

        private static MarginPadding contentPadding => new MarginPadding
        {
            Left = content_padding + left_edge_extension,
            Right = content_padding,
            Top = content_padding,
            Bottom = content_padding,
        };

        private Drawable createTitleCard(OverlayColourProvider colourProvider)
        {
            return new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = corner_radius,
                Children = new Drawable[]
                {
                    new CardBackground(),
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Shear = -OsuGame.SHEAR,
                        Padding = contentPadding,
                        Spacing = new Vector2(0, 4),
                        Children = new Drawable[]
                        {
                            titleText = new TruncatingSpriteText
                            {
                                RelativeSizeAxes = Axes.X,
                                Font = OsuFont.Style.Title,
                            },
                            creatorText = new TruncatingSpriteText
                            {
                                RelativeSizeAxes = Axes.X,
                                Font = OsuFont.Style.Heading2,
                                Colour = colourProvider.Content2,
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(2, 0),
                                Margin = new MarginPadding { Top = 4 },
                                Children = new Drawable[]
                                {
                                    downloadsPill = new StatisticPill(FontAwesome.Solid.Download, extendLeft: true)
                                    {
                                        TooltipText = "Download count",
                                    },
                                    favouritesPill = new FavouritePill(),
                                },
                            },
                        },
                    },
                },
            };
        }

        private Drawable createDetailsHeader()
        {
            return new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 30,
                Padding = new MarginPadding
                {
                    Left = content_padding + left_edge_extension,
                    Right = 4,
                },
                Shear = -OsuGame.SHEAR,
                Children = new Drawable[]
                {
                    new DetailsTabHeader
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    new SkinSection.ViewOnSkinListingButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        AutoSizeAxes = Axes.X,
                        Height = 30,
                    },
                },
            };
        }

        private Drawable createActionButtons()
        {
            return new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 30,
                Padding = new MarginPadding
                {
                    Left = content_padding + left_edge_extension,
                    Right = 4,
                },
                Shear = -OsuGame.SHEAR,
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(1, 0),
                    Children = new Drawable[]
                    {
                        new SkinSection.EditSkinButton { AutoSizeAxes = Axes.X, Height = 30 },
                        new SkinSection.RenameSkinButton { AutoSizeAxes = Axes.X, Height = 30 },
                        new SkinSection.ExportSkinButton { AutoSizeAxes = Axes.X, Height = 30 },
                        new SkinSection.DeleteSkinButton { AutoSizeAxes = Axes.X, Height = 30 },
                    },
                },
            };
        }

        private Drawable createDetailsCard()
        {
            return new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = corner_radius,
                Children = new Drawable[]
                {
                    new CardBackground(),
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Shear = -OsuGame.SHEAR,
                        Padding = contentPadding,
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                ColumnDimensions = new[]
                                {
                                    new Dimension(),
                                    new Dimension(),
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        uploadedBy = new MetadataField("Uploaded by"),
                                        version = new MetadataField("Version"),
                                    },
                                },
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                ColumnDimensions = new[]
                                {
                                    new Dimension(),
                                    new Dimension(),
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        submitted = new MetadataField(SongSelectStrings.Submitted),
                                        lastUpdated = new MetadataField("Last updated"),
                                    },
                                },
                            },
                            tags = new MetadataField("Skin Tags"),
                        },
                    },
                },
            };
        }

        private void updateDisplay()
        {
            lookupCancellation?.Cancel();
            lookupCancellation = new CancellationTokenSource();
            int generation = ++lookupGeneration;

            var skin = currentSkin.Value;
            string displayName = SkinIniVersionHelper.GetDisplayName(skin.Name);
            string creator = skin.SkinInfo.PerformRead(s => s.Creator) ?? string.Empty;

            titleText.Text = string.IsNullOrWhiteSpace(displayName) ? skin.SkinInfo.ToString() : displayName;
            creatorText.Text = string.IsNullOrWhiteSpace(creator) ? "-" : creator;

            string localVersion = SkinIniVersionHelper.GetSkinVersion(skin, useDefaultIfMissing: false);
            version.SetText(string.IsNullOrWhiteSpace(localVersion) ? "-" : localVersion);

            downloadsPill.SetValue(null);
            favouritesPill.SetSkin(null);
            uploadedBy.SetText("-");
            submitted.SetDate(null);
            lastUpdated.SetDate(null);
            tags.SetTags(Array.Empty<string>());

            if (!SkinIniVersionHelper.TryGetOnlineSkinId(skin, out int onlineSkinId))
                return;

            var token = lookupCancellation.Token;

            if (skinLookupCache.TryGetCached(onlineSkinId, out var cached))
            {
                applyOnlineSkin(cached, generation);
                return;
            }

            Task.Run(async () =>
            {
                var result = await skinLookupCache.GetSkinAsync(onlineSkinId, token).ConfigureAwait(false);

                Schedule(() =>
                {
                    if (generation != lookupGeneration || token.IsCancellationRequested)
                        return;

                    if (result != null)
                        applyOnlineSkin(result, generation);
                });
            }, token);
        }

        private void applyOnlineSkin(APIOnlineSkin online, int generation)
        {
            if (generation != lookupGeneration)
                return;

            if (!string.IsNullOrWhiteSpace(online.Name))
                titleText.Text = SkinIniVersionHelper.GetDisplayName(online.Name);

            if (!string.IsNullOrWhiteSpace(online.Creator))
                creatorText.Text = online.Creator;

            downloadsPill.SetValue(online.DownloadCount);
            favouritesPill.SetSkin(online);

            string? uploader = online.GetUploaderDisplayName();
            if (string.IsNullOrWhiteSpace(uploader))
                uploadedBy.SetText("-");
            else
                uploadedBy.SetUploader(uploader);

            string displayVersion = SkinIniVersionHelper.GetDisplayVersion(online.Version, online.Name);
            if (!string.IsNullOrWhiteSpace(displayVersion))
                version.SetText(displayVersion);

            submitted.SetDate(online.CreatedAt);
            lastUpdated.SetDate(online.LastUpdated ?? online.CreatedAt);

            string[] tagList = string.IsNullOrWhiteSpace(online.Tags)
                ? Array.Empty<string>()
                : online.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            tags.SetTags(tagList, tag => game?.SearchSkin(tag));
        }

        private partial class CardBackground : Box
        {
            public CardBackground()
            {
                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                // Match FormControlBackground Normal (skin selector fill).
                Colour = colourProvider.Background4.Darken(0.1f);
            }
        }

        private partial class DetailsTabHeader : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                AutoSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = SongSelectStrings.Details,
                        Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                        Colour = colourProvider.Content1,
                    },
                    new Circle
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Y = 4,
                        Colour = colourProvider.Highlight1,
                    },
                };
            }
        }

        private partial class StatisticPill : CompositeDrawable, IHasTooltip
        {
            private readonly IconUsage icon;
            private readonly float leftPadding;
            private OsuSpriteText valueText = null!;

            public LocalisableString TooltipText { get; set; }

            public StatisticPill(IconUsage icon, bool extendLeft = false)
            {
                this.icon = icon;
                leftPadding = extendLeft ? content_padding + left_edge_extension : 10f;

                AutoSizeAxes = Axes.Both;

                if (extendLeft)
                    Margin = new MarginPadding { Left = -leftPadding };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Masking = true;
                CornerRadius = 5;
                Shear = OsuGame.SHEAR;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.2f,
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(4, 0),
                        Shear = -OsuGame.SHEAR,
                        Margin = new MarginPadding { Left = leftPadding, Right = 10f, Vertical = 5f },
                        Children = new Drawable[]
                        {
                            new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Icon = icon,
                                Size = new Vector2(OsuFont.Style.Heading2.Size),
                                Colour = colourProvider.Content2,
                            },
                            valueText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.Style.Heading2,
                                Colour = colourProvider.Content2,
                                Margin = new MarginPadding { Bottom = 2f },
                                Text = "-",
                            },
                        },
                    },
                };
            }

            public void SetValue(int? value)
            {
                valueText.Text = value?.ToLocalisableString(@"N0") ?? (LocalisableString)"-";
            }
        }

        private partial class FavouritePill : OsuClickableContainer, IHasTooltip
        {
            private Box background = null!;
            private Box hoverLayer = null!;
            private SpriteIcon icon = null!;
            private OsuSpriteText valueText = null!;
            private LoadingSpinner loadingSpinner = null!;

            private APIOnlineSkin? onlineSkin;
            private PostSkinFavouriteRequest? favouriteRequest;
            private bool isFavourite;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [Resolved]
            private IAPIProvider api { get; set; } = null!;

            public override LocalisableString TooltipText
            {
                get
                {
                    if (onlineSkin == null)
                        return string.Empty;

                    return isFavourite ? "Unfavourite" : "Favourite";
                }
            }

            public FavouritePill()
            {
                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Masking = true;
                CornerRadius = 5;
                Shear = OsuGame.SHEAR;

                AddRange(new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.2f),
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(4, 0),
                        Shear = -OsuGame.SHEAR,
                        Margin = new MarginPadding { Left = 10, Right = 10, Vertical = 5f },
                        Children = new Drawable[]
                        {
                            icon = new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Icon = FontAwesome.Regular.Heart,
                                Size = new Vector2(OsuFont.Style.Heading2.Size),
                                Colour = colourProvider.Content2,
                            },
                            new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.X,
                                Height = 20,
                                Children = new Drawable[]
                                {
                                    loadingSpinner = new LoadingSpinner
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Size = new Vector2(12f),
                                    },
                                    valueText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Font = OsuFont.Style.Heading2,
                                        Colour = colourProvider.Content2,
                                        Margin = new MarginPadding { Bottom = 2f },
                                        Text = "-",
                                    },
                                },
                            },
                        },
                    },
                    hoverLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Colour = Colour4.White.Opacity(0.1f),
                        Blending = BlendingParameters.Additive,
                    },
                });

                Action = toggleFavourite;
                Enabled.Value = false;
            }

            protected override bool OnHover(HoverEvent e)
            {
                if (Enabled.Value)
                    hoverLayer.FadeIn(200, Easing.OutQuint);

                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                hoverLayer.FadeOut(200, Easing.OutQuint);
            }

            public void SetSkin(APIOnlineSkin? skin)
            {
                if (favouriteRequest?.CompletionState == APIRequestCompletionState.Waiting)
                    favouriteRequest.Cancel();

                onlineSkin = skin;
                loadingSpinner.Hide();
                valueText.Show();

                if (skin == null)
                {
                    Enabled.Value = false;
                    isFavourite = false;
                    valueText.Text = "-";
                    updateVisuals();
                    return;
                }

                Enabled.Value = api.IsLoggedIn && skin.OnlineID > 0;
                isFavourite = skin.HasFavourited;
                valueText.Text = skin.FavouriteCount.ToLocalisableString(@"N0");
                updateVisuals();
            }

            private void updateVisuals()
            {
                background.FadeColour(isFavourite ? colours.Pink4.Darken(1f).Opacity(0.5f) : Color4.Black.Opacity(0.2f), 300, Easing.OutQuint);
                valueText.FadeColour(isFavourite ? colours.Pink1 : colourProvider.Content2, 300, Easing.OutQuint);
                icon.FadeColour(isFavourite ? colours.Pink1 : colourProvider.Content2, 300, Easing.OutQuint);
                icon.Icon = isFavourite ? FontAwesome.Solid.Heart : FontAwesome.Regular.Heart;
            }

            private void toggleFavourite()
            {
                if (onlineSkin == null || onlineSkin.OnlineID <= 0)
                    return;

                var skin = onlineSkin;
                var action = isFavourite ? SkinFavouriteAction.UnFavourite : SkinFavouriteAction.Favourite;

                favouriteRequest?.Cancel();
                favouriteRequest = new PostSkinFavouriteRequest(skin.OnlineID, action);

                loadingSpinner.Show();
                valueText.Hide();

                favouriteRequest.Success += () =>
                {
                    bool favourited = action == SkinFavouriteAction.Favourite;
                    skin.HasFavourited = favourited;
                    skin.FavouriteCount += favourited ? 1 : -1;

                    if (ReferenceEquals(skin, onlineSkin))
                        SetSkin(skin);
                };

                favouriteRequest.Failure += e =>
                {
                    Logger.Error(e, $"Failed to {action.ToString().ToLowerInvariant()} skin: {e.Message}");

                    if (ReferenceEquals(skin, onlineSkin))
                        SetSkin(skin);
                };

                api.Queue(favouriteRequest);
            }
        }

        private partial class MetadataField : FillFlowContainer
        {
            private readonly TruncatingSpriteText contentText;
            private readonly DrawableDate contentDate;
            private readonly LinkFlowContainer contentLink;
            private readonly LinkFlowContainer contentTags;

            public MetadataField(LocalisableString label)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Direction = FillDirection.Vertical;
                Spacing = new Vector2(0, 2);
                Padding = new MarginPadding { Right = 8 };

                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = label,
                        Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            contentText = new TruncatingSpriteText
                            {
                                RelativeSizeAxes = Axes.X,
                                Font = OsuFont.Style.Caption2,
                                Colour = Color4.White.Opacity(0.75f),
                            },
                            contentDate = new DrawableDate(DateTimeOffset.Now)
                            {
                                Font = OsuFont.Style.Caption2,
                                Colour = Color4.White.Opacity(0.75f),
                                Alpha = 0,
                            },
                            contentLink = new LinkFlowContainer(t =>
                            {
                                t.Font = OsuFont.Style.Caption2;
                                t.Colour = Color4.White.Opacity(0.75f);
                            })
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Alpha = 0,
                            },
                            contentTags = new LinkFlowContainer(t =>
                            {
                                t.Font = OsuFont.Style.Caption2;
                                t.Colour = Color4.White.Opacity(0.75f);
                            })
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Alpha = 0,
                            },
                        },
                    },
                };
            }

            public void SetText(LocalisableString text)
            {
                contentText.Text = text;
                contentText.Show();
                contentDate.Hide();
                contentLink.Hide();
                contentTags.Hide();
            }

            public void SetUploader(string username)
            {
                contentLink.Clear();
                contentLink.AddUserLink(new APIUser { Username = username }, t =>
                {
                    t.Font = OsuFont.Style.Caption2;
                    t.Colour = Color4.White.Opacity(0.75f);
                });

                contentLink.Show();
                contentText.Hide();
                contentDate.Hide();
                contentTags.Hide();
            }

            public void SetDate(DateTimeOffset? date)
            {
                if (date == null)
                {
                    SetText("-");
                    return;
                }

                contentDate.Date = date.Value;
                contentDate.Show();
                contentText.Hide();
                contentLink.Hide();
                contentTags.Hide();
            }

            public void SetTags(string[] tagList, Action<string>? onTagClick = null)
            {
                contentTags.Clear();

                if (tagList.Length == 0)
                {
                    SetText("-");
                    return;
                }

                for (int i = 0; i < tagList.Length; i++)
                {
                    string tag = tagList[i];

                    if (onTagClick != null)
                        contentTags.AddLink(tag, () => onTagClick(tag));
                    else
                        contentTags.AddText(tag);

                    if (i < tagList.Length - 1)
                        contentTags.AddText(" ");
                }

                contentTags.Show();
                contentText.Hide();
                contentDate.Hide();
                contentLink.Hide();
            }
        }
    }
}
