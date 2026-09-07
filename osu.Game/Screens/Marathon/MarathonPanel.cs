// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Marathon
{
    public partial class MarathonPanel : OsuClickableContainer, IHasContextMenu
    {
        public readonly APIMarathon Marathon;

        public Action? EditAction { get; set; }
        public Action? DuplicateAction { get; set; }
        public Action? DeleteAction { get; set; }
        public Action? PlayAction { get; set; }

        private readonly UpdateableBeatmapBackgroundSprite beatmapBackground;
        private readonly Box hoverOverlay;
        private readonly ShearedButton playButton;
        private readonly FillFlowContainer buttonsFlow;
        private readonly MarathonStarRangeDisplay starRange;
        private readonly MarathonRankRangeDisplay rankRange;
        private ShearedButton? editButton;
        private OsuColour colours = null!;
        private readonly IBindable<APIUser> localUser = new Bindable<APIUser>();
        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public MarathonPanel(APIMarathon marathon)
        {
            Marathon = marathon;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 10;

            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Colour4.Black.Opacity(0.25f),
                Radius = 4,
            };

            var textFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(3),
                Padding = new MarginPadding { Right = 12 },
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            };

            textFlow.Add(new TruncatingSpriteText
            {
                RelativeSizeAxes = Axes.X,
                Text = string.IsNullOrEmpty(marathon.Name) ? "Untitled marathon" : marathon.Name,
                Font = OsuFont.GetFont(size: 22, weight: FontWeight.SemiBold),
                Shadow = true,
            });

            if (!string.IsNullOrEmpty(marathon.Description))
            {
                textFlow.Add(new TruncatingSpriteText
                {
                    RelativeSizeAxes = Axes.X,
                    Text = marathon.Description,
                    Font = OsuFont.GetFont(size: 16),
                    Shadow = true,
                });
            }

            starRange = new MarathonStarRangeDisplay();

            if (MarathonStarRangeDisplay.TryGetRange(marathon.Maps.Select(m => m.StarRating), out double minStars, out double maxStars))
                starRange.SetRange(minStars, maxStars);
            else
                starRange.ClearRange();

            rankRange = new MarathonRankRangeDisplay();
            rankRange.SetFrom(marathon);

            textFlow.Add(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(8),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = creditText(marathon),
                        Font = OsuFont.GetFont(size: 14),
                        Shadow = true,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    starRange,
                    rankRange,
                }
            });

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black,
                },
                beatmapBackground = new UpdateableBeatmapBackgroundSprite(BeatmapSetCoverType.Cover)
                {
                    RelativeSizeAxes = Axes.Both,
                    BackgroundLoadDelay = 0,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Shear = new Vector2(0.8f, 0),
                    Alpha = 0.65f,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Width = 0.45f,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(Color4.Black, new Color4(0f, 0f, 0f, 0.55f)),
                            Width = 0.35f,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.55f), new Color4(0, 0, 0, 0.2f)),
                            Width = 0.2f,
                        },
                    }
                },
                hoverOverlay = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White,
                    Alpha = 0,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = 16, Vertical = 14 },
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            textFlow,
                            buttonsFlow = new FillFlowContainer
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(8),
                                Children = new Drawable[]
                                {
                                    playButton = new ShearedButton
                                    {
                                        Width = 110,
                                        Height = 40,
                                        Text = MarathonStrings.PlayButton,
                                        TextSize = 16,
                                        Enabled = { Value = marathon.Maps.Length > 0 },
                                        Action = () => PlayAction?.Invoke(),
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            this.colours = colours;
            playButton.DarkerColour = colours.Lime1.Darken(0.6f);
            playButton.LighterColour = colours.Lime1;
            playButton.TextColour = Colour4.Black;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            localUser.BindTo(api.LocalUser);
            apiState.BindTo(api.State);
            localUser.BindValueChanged(_ => updateOwnerControls());
            apiState.BindValueChanged(_ => updateOwnerControls(), true);

            loadCover();
            resolveStarRange();
            rankRange.SetFrom(Marathon);
        }

        protected override void Dispose(bool isDisposing)
        {
            localUser.UnbindAll();
            apiState.UnbindAll();
            base.Dispose(isDisposing);
        }

        private void updateOwnerControls()
        {
            if (isOwner)
            {
                if (editButton != null)
                    return;

                editButton = new ShearedButton
                {
                    Width = 90,
                    Height = 40,
                    Text = MarathonStrings.EditButton,
                    TextSize = 16,
                    DarkerColour = colours.Blue3,
                    LighterColour = colours.Blue1,
                    Action = () => EditAction?.Invoke(),
                };

                buttonsFlow.Insert(0, editButton);
                return;
            }

            if (editButton == null)
                return;

            buttonsFlow.Remove(editButton, true);
            editButton = null;
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                var items = new[]
                {
                    new OsuMenuItem(MarathonStrings.PlayButton, MenuItemType.Highlighted, () => PlayAction?.Invoke()),
                    new OsuMenuItem(MarathonStrings.DuplicateButton, MenuItemType.Standard, () => DuplicateAction?.Invoke()),
                };

                if (!isOwner)
                    return items;

                return items.Concat([
                    new OsuMenuItem(MarathonStrings.EditButton, MenuItemType.Standard, () => EditAction?.Invoke()),
                    new OsuMenuItem(MarathonStrings.DeleteButton, MenuItemType.Destructive, () => DeleteAction?.Invoke()),
                ]).ToArray();
            }
        }

        private bool isOwner =>
            api.State.Value == APIState.Online
            && Marathon.OwnerId != 0
            && api.LocalUser.Value.Id == Marathon.OwnerId;

        protected override bool OnHover(HoverEvent e)
        {
            hoverOverlay.FadeTo(0.08f, 120, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverOverlay.FadeOut(120, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        private static string creditText(APIMarathon marathon)
        {
            string maps = $"{marathon.Maps.Length} maps";
            string owner = string.IsNullOrEmpty(marathon.OwnerUsername) ? maps : $"{marathon.OwnerUsername} · {maps}";

            if (!marathon.HasSourceCredit)
                return owner;

            return $"{owner} · originally {marathon.SourceOwnerUsername}";
        }

        private void resolveStarRange()
        {
            var stars = new List<double>();

            foreach (var map in Marathon.Maps)
            {
                if (map.StarRating > 0)
                {
                    stars.Add(map.StarRating);
                    continue;
                }

                if (string.IsNullOrEmpty(map.Checksum))
                    continue;

                var local = beatmaps.QueryBeatmap(b => b.MD5Hash == map.Checksum);
                if (local != null && local.StarRating > 0)
                    stars.Add(local.StarRating);
            }

            if (MarathonStarRangeDisplay.TryGetRange(stars, out double min, out double max))
                starRange.SetRange(min, max);
            else
                starRange.ClearRange();
        }

        private void loadCover()
        {
            var first = Marathon.Maps.OrderBy(m => m.PlaylistOrder).FirstOrDefault();
            if (first == null)
                return;

            if (!string.IsNullOrEmpty(first.Checksum))
            {
                var local = beatmaps.QueryBeatmap(b => b.MD5Hash == first.Checksum);
                if (local != null)
                {
                    beatmapBackground.Beatmap.Value = local;
                    return;
                }
            }

            if (first.BeatmapId <= 0)
                return;

            beatmapLookupCache.GetBeatmapAsync(first.BeatmapId).ContinueWith(task =>
            {
                if (!task.IsCompletedSuccessfully)
                    return;

                var beatmap = task.GetResultSafely();
                if (beatmap == null)
                    return;

                Schedule(() => beatmapBackground.Beatmap.Value = beatmap);
            });
        }
    }
}
