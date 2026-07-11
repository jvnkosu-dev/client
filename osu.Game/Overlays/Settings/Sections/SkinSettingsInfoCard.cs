// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
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
using osu.Game.IO;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
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

        private MarqueeContainer titleText = null!;
        private MarqueeContainer creatorText = null!;
        private SkinOriginStatusPill originPill = null!;
        private StatisticPill downloadsPill = null!;
        private FavouritePill favouritesPill = null!;
        private Sprite titleCover = null!;
        private Box titleCoverDim = null!;
        private SkinSection.UpdateSkinButton updateButton = null!;
        private APIOnlineSkin? lastOnlineSkin;

        private MetadataField uploadedBy = null!;
        private MetadataField modes = null!;
        private MetadataField version = null!;
        private MetadataField skinType = null!;
        private MetadataField submitted = null!;
        private MetadataField lastUpdated = null!;
        private MetadataField tags = null!;

        private int lookupGeneration;
        private CancellationTokenSource? lookupCancellation;
        private OverlayColourProvider colourProvider = null!;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Resolved]
        private SkinLookupCache skinLookupCache { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private OsuGame? game { get; set; }

        [Resolved]
        private SettingsOverlay? settingsOverlay { get; set; }

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
            this.colourProvider = colourProvider;

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

            skins.SourceChanged += onSkinSourceChanged;

            settingsOverlay?.State.BindValueChanged(onSettingsVisibilityChanged);
        }

        private void onSettingsVisibilityChanged(ValueChangedEvent<Visibility> state)
        {
            if (state.NewValue != Visibility.Visible)
                return;

            // Re-check local hash + fresh listing metadata whenever settings open.
            refreshUpdateCheck();
        }

        private void refreshUpdateCheck()
        {
            if (currentSkin.Value != null)
            {
                int onlineSkinId = SkinManager.GetListingId(currentSkin.Value);
                if (onlineSkinId > 0)
                    skinLookupCache.Invalidate(onlineSkinId);
            }

            updateDisplay();
        }

        private void onSkinSourceChanged() => Schedule(() =>
        {
            if (currentSkin.Value == null)
                return;

            applyLocalMetadata(currentSkin.Value);
            updateTitleCoverFromLocalSkin(currentSkin.Value);
            updateOriginPill(currentSkin.Value);

            if (lastOnlineSkin != null)
                updateUpdateButton(lastOnlineSkin);
        });

        protected override void Dispose(bool isDisposing)
        {
            skins.SourceChanged -= onSkinSourceChanged;
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
                    titleCover = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 1.25f,
                        Height = 1.25f,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        FillMode = FillMode.Fill,
                        // Parent wedge is sheared; counter so the thumbnail keeps its aspect.
                        Shear = -OsuGame.SHEAR,
                        Alpha = 0,
                    },
                    titleCoverDim = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(
                            colourProvider.Background6.Opacity(0.15f),
                            colourProvider.Background6.Opacity(0.45f)),
                        Alpha = 0,
                    },
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
                            originPill = new SkinOriginStatusPill
                            {
                                TextSize = OsuFont.Style.Caption1.Size,
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = OsuFont.Style.Title.Size,
                                Margin = new MarginPadding { Bottom = -4f },
                                Child = titleText = new MarqueeContainer
                                {
                                    OverflowSpacing = 50,
                                },
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = OsuFont.Style.Heading2.Size,
                                Child = creatorText = new MarqueeContainer
                                {
                                    OverflowSpacing = 50,
                                },
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
                    updateButton = new SkinSection.UpdateSkinButton
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Margin = new MarginPadding
                        {
                            Right = content_padding,
                            Bottom = content_padding,
                        },
                        // Cancel parent wedge shear; button applies its own Content.Shear like song select.
                        Shear = -OsuGame.SHEAR,
                        Alpha = 0,
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
                                        modes = new MetadataField("Mode"),
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
                                        version = new MetadataField("Version"),
                                        skinType = new MetadataField("Skin Type"),
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
                                        submitted = new MetadataField("Uploaded"),
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

            applyLocalMetadata(skin);
            updateTitleCoverFromLocalSkin(skin);

            downloadsPill.SetValue(null);
            favouritesPill.SetSkin(null);
            hideUpdateButton();
            lastOnlineSkin = null;

            // Backfill realm OnlineSkinId from skin.ini for skins installed before that field existed.
            if (SkinIniVersionHelper.TryGetOnlineSkinId(skin, out int configOnlineId))
                skins.EnsureListingLink(skin.SkinInfo, configOnlineId);

            // Pill uses only realm-persisted fields (like beatmap OnlineID + LocallyModified).
            updateOriginPill(skin);

            int onlineSkinId = SkinManager.GetListingId(skin);
            if (onlineSkinId <= 0)
            {
                uploadedBy.SetText("-");
                submitted.SetDate(null);
                lastUpdated.SetDate(null);
                return;
            }

            uploadedBy.SetLoading();
            submitted.SetLoading();
            lastUpdated.SetLoading();

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

                    // Failed lookup must not flip Server → Local (connection blips, etc.).
                    if (result != null)
                        applyOnlineSkin(result, generation);
                    else
                    {
                        uploadedBy.SetText("-");
                        submitted.SetDate(null);
                        lastUpdated.SetDate(null);
                    }
                });
            }, token);
        }

        /// <summary>
        /// Fills title + details wedges from local skin files / skin.ini (never from listing).
        /// </summary>
        private void applyLocalMetadata(Skin skin)
        {
            string displayName = SkinIniVersionHelper.GetDisplayName(skin.Name);
            string creator = skin.SkinInfo.PerformRead(s => s.Creator) ?? string.Empty;

            string title = string.IsNullOrWhiteSpace(displayName) ? skin.SkinInfo.ToString() : displayName;
            string creatorDisplay = string.IsNullOrWhiteSpace(creator) || creator == @"Unknown" ? "-" : creator;

            titleText.CreateContent = () => new OsuSpriteText
            {
                Text = title,
                Shadow = true,
                Font = OsuFont.Style.Title,
            };
            creatorText.CreateContent = () => new OsuSpriteText
            {
                Text = creatorDisplay,
                Shadow = true,
                Font = OsuFont.Style.Heading2,
                Colour = colourProvider.Content2,
            };

            string localVersion = SkinIniVersionHelper.GetSkinVersion(skin, useDefaultIfMissing: false);
            version.SetText(string.IsNullOrWhiteSpace(localVersion) ? "-" : localVersion);

            skinType.SetText(skin.SkinInfo.PerformRead(s =>
                SkinEngineTypeHelper.GetDisplayName(SkinEngineTypeHelper.FromSkinInfo(s))));

            setModes(SkinIniVersionHelper.ParseModifiedModes(skin.Configuration.ModifiedModes).ToArray());

            string[] tagList = string.IsNullOrWhiteSpace(skin.Configuration.Tags)
                ? Array.Empty<string>()
                : skin.Configuration.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            tags.SetTags(tagList, tag => game?.SearchSkin(tag));
        }

        private void applyOnlineSkin(APIOnlineSkin online, int generation)
        {
            if (generation != lookupGeneration)
                return;

            // Listing-only fields (not present in skin.ini the same way).
            downloadsPill.SetValue(online.DownloadCount);
            favouritesPill.SetSkin(online);

            string? uploader = online.GetUploaderDisplayName();
            if (string.IsNullOrWhiteSpace(uploader))
                uploadedBy.SetText("-");
            else
                uploadedBy.SetUploader(uploader);

            submitted.SetDate(online.CreatedAt);
            lastUpdated.SetDate(online.LastUpdated ?? online.CreatedAt);

            lastOnlineSkin = online;
            skins.EnsureListingLink(currentSkin.Value.SkinInfo, online.OnlineID);
            skins.EnsureOnlineHashBaseline(currentSkin.Value.SkinInfo);

            // Server-origin skins keep OnlineHash aligned with local content. Update detection uses
            // LocallyModified + SkinVersion (not LastUpdated / hash drift after our own upload).
            if (!currentSkin.Value.SkinInfo.PerformRead(s => s.LocallyModified))
                skins.AlignOnlineHashWithLocal(currentSkin.Value.SkinInfo);

            updateOriginPill(currentSkin.Value);
            updateUpdateButton(online);
        }

        /// <summary>
        /// Origin pill reads listing link via direct realm Find (not Skin.SkinInfo Live).
        /// </summary>
        private void updateOriginPill(Skin skin)
        {
            bool showServer = skins.IsServerOrigin(skin.SkinInfo.ID);

            // Recover missing realm link from skin.ini when possible (upgrade / stale live).
            if (!showServer && SkinIniVersionHelper.TryGetOnlineSkinId(skin, out int configOnlineId))
            {
                skins.EnsureListingLink(skin.SkinInfo, configOnlineId);
                showServer = skins.IsServerOrigin(skin.SkinInfo.ID);
            }

            originPill.SetOrigin(showServer ? SkinOriginStatus.Server : SkinOriginStatus.Local);
        }

        private void updateUpdateButton(APIOnlineSkin online)
        {
            var skin = currentSkin.Value;

            if (!SkinUpdateHelper.IsUpdateAvailable(skin, online))
            {
                hideUpdateButton();
                return;
            }

            updateButton.SetTarget(online, skin.SkinInfo);
            updateButton.FadeIn(200, Easing.OutQuint);
        }

        private void hideUpdateButton()
        {
            updateButton.ClearTarget();
            updateButton.FadeOut(150, Easing.OutQuint);
        }

        private void setModes(IReadOnlyCollection<string> modeList)
        {
            string display = SkinModifiedModesHelper.FormatForDisplay(modeList, rulesets);
            modes.SetText(string.IsNullOrWhiteSpace(display) ? "-" : display);
        }

        private void updateTitleCoverFromLocalSkin(Skin skin)
        {
            var resources = (IStorageResourceProvider)skins;
            var texture = SkinBackgroundHelper.GetTexture(skin, resources.Renderer, resources);

            if (texture == null)
            {
                clearTitleCover();
                return;
            }

            titleCover.Texture = null;
            titleCover.Texture = texture;
            titleCover.FadeTo(0.35f, 400, Easing.OutQuint);
            titleCoverDim.FadeTo(0.6f, 400, Easing.OutQuint);
        }

        private void clearTitleCover()
        {
            titleCover.Texture = null;
            titleCover.FadeOut(200, Easing.OutQuint);
            titleCoverDim.FadeOut(200, Easing.OutQuint);
        }

        private enum SkinOriginStatus
        {
            Local,
            Server,
        }

        /// <summary>
        /// Beatmap-wedge-style status pill: Local (red) vs Server (green).
        /// </summary>
        private partial class SkinOriginStatusPill : CircularContainer
        {
            private readonly OsuSpriteText statusText;
            private readonly Box background;

            private SkinOriginStatus origin = SkinOriginStatus.Local;

            private OverlayColourProvider colourProvider = null!;

            public float TextSize
            {
                init => statusText.Font = statusText.Font.With(size: value);
            }

            public SkinOriginStatusPill()
            {
                AutoSizeAxes = Axes.Both;
                Masking = true;

                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    statusText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(weight: FontWeight.Bold),
                        Padding = new MarginPadding { Horizontal = 6, Vertical = 1 },
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                this.colourProvider = colourProvider;
                // Don't paint default Local here — wait for SetOrigin from updateOriginPill,
                // otherwise a later SetOrigin(Server) before IsLoaded can be overwritten visually.
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                updateState();
            }

            public void SetOrigin(SkinOriginStatus value)
            {
                if (origin == value && IsLoaded)
                    return;

                origin = value;
                updateState();
            }

            private void updateState()
            {
                if (!IsLoaded || colourProvider == null)
                    return;

                switch (origin)
                {
                    default:
                    case SkinOriginStatus.Local:
                        background.Colour = Color4.OrangeRed;
                        statusText.Text = SongSelectStrings.LocallyModified.ToUpper();
                        break;

                    case SkinOriginStatus.Server:
                        background.Colour = Color4Extensions.FromHex(@"b3ff66");
                        statusText.Text = CommonStrings.Server.ToUpper();
                        break;
                }

                statusText.Colour = colourProvider.Background3;
            }
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
            private readonly LoadingSpinner contentLoading;

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
                        // Fixed height so loading spinner ↔ text never shifts rows or wedge size.
                        Height = OsuFont.Style.Caption2.Size,
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
                            contentLoading = new LoadingSpinner
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(10),
                            },
                        },
                    },
                };
            }

            private void clear()
            {
                contentText.Hide();
                contentDate.Hide();
                contentLink.Hide();
                contentTags.Hide();
                contentLoading.Hide();
            }

            public void SetLoading()
            {
                clear();
                contentLoading.Show();
            }

            public void SetText(LocalisableString text)
            {
                clear();
                contentText.Text = text;
                contentText.Show();
            }

            public void SetUploader(string username)
            {
                clear();
                contentLink.Clear();
                contentLink.AddUserLink(new APIUser { Username = username }, t =>
                {
                    t.Font = OsuFont.Style.Caption2;
                    t.Colour = Color4.White.Opacity(0.75f);
                });
                contentLink.Show();
            }

            public void SetDate(DateTimeOffset? date)
            {
                if (date == null)
                {
                    SetText("-");
                    return;
                }

                clear();
                contentDate.Date = date.Value;
                contentDate.Show();
            }

            public void SetTags(string[] tagList, Action<string>? onTagClick = null)
            {
                if (tagList.Length == 0)
                {
                    SetText("-");
                    return;
                }

                clear();
                contentTags.Clear();

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
            }
        }
    }
}
