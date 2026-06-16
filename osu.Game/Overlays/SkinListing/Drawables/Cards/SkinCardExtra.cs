using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Beatmaps.Drawables.Cards.Statistics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.SkinListing.Drawables.Cards.Statistics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class SkinCardExtra : SkinCard
    {
        protected override Drawable IdleContent => idleBottomContent;
        protected override Drawable DownloadInProgressContent => downloadProgressBar;

        private const float height = 112;

        [Cached]
        private readonly BeatmapCardContent content;

        protected override BeatmapCardContent? ExpansionContent => content;

        private SkinCardThumbnail thumbnail = null!;
        private SkinCollapsibleButtonContainer buttonContainer = null!;
        private FillFlowContainer contentFlow = null!;
        private FillFlowContainer headerFlow = null!;
        private FillFlowContainer<BeatmapCardStatistic> statisticsContainer = null!;
        private SkinCardDescription description = null!;

        private FillFlowContainer idleBottomContent = null!;
        private BeatmapCardDownloadProgressBar downloadProgressBar = null!;
        private TruncatingSpriteText versionText = null!;

        private string displayVersion = string.Empty;
        private bool descriptionOverflows;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private float lastAvailableHeight = -1;

        public SkinCardExtra(APIOnlineSkin skin, bool allowExpansion = true)
            : base(skin, false)
        {
            content = new BeatmapCardContent(height);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = WIDTH;
            Height = height;

            displayVersion = SkinIniVersionHelper.GetDisplayVersion(Skin.Version, Skin.Name);

            Child = content.With(c =>
            {
                c.MainContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        thumbnail = new SkinCardThumbnail(Skin.GetThumbnailRequestUrl())
                        {
                            Name = @"Left (thumbnail) area",
                            Size = new Vector2(height),
                            Padding = new MarginPadding { Right = CORNER_RADIUS },
                        },
                        buttonContainer = new SkinCollapsibleButtonContainer(Skin, DownloadTracker.State)
                        {
                            FavouriteState = { BindTarget = FavouriteState },
                            X = height - CORNER_RADIUS,
                            Width = WIDTH - height + CORNER_RADIUS,
                            ButtonsCollapsedWidth = CORNER_RADIUS,
                            ButtonsExpandedWidth = 24,
                            Children = new Drawable[]
                            {
                                contentFlow = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 2),
                                    Children = new Drawable[]
                                    {
                                        headerFlow = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Children = new Drawable[]
                                            {
                                                new TruncatingSpriteText
                                                {
                                                    Text = Skin.Name,
                                                    Font = OsuFont.Default.With(size: 18f, weight: FontWeight.SemiBold),
                                                    RelativeSizeAxes = Axes.X,
                                                },
                                                new TruncatingSpriteText
                                                {
                                                    Text = $"создал {Skin.Creator}",
                                                    Font = OsuFont.Default.With(size: 14f, weight: FontWeight.SemiBold),
                                                    Colour = colourProvider.Light1,
                                                    RelativeSizeAxes = Axes.X,
                                                },
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    Height = VERSION_LINE_HEIGHT,
                                                    Child = versionText = new TruncatingSpriteText
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        RelativeSizeAxes = Axes.X,
                                                        Text = displayVersion,
                                                        Shadow = false,
                                                        Font = OsuFont.GetFont(size: 11f, weight: FontWeight.SemiBold),
                                                        Colour = colourProvider.Content2,
                                                    },
                                                },
                                            }
                                        },
                                        statisticsContainer = new FillFlowContainer<BeatmapCardStatistic>
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(8, 0),
                                            ChildrenEnumerable = SkinCardStatistics.CreateFor(Skin, FavouriteState),
                                        },
                                        description = new SkinCardDescription(Skin.Description)
                                        {
                                            RelativeSizeAxes = Axes.X,
                                        },
                                    }
                                },
                                new Container
                                {
                                    Name = @"Bottom content",
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Children = new Drawable[]
                                    {
                                        idleBottomContent = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            AlwaysPresent = true,
                                        },
                                        downloadProgressBar = new BeatmapCardDownloadProgressBar
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 5,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            State = { BindTarget = DownloadTracker.State },
                                            Progress = { BindTarget = DownloadTracker.Progress },
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
                c.ExpandedContent = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = 8, Vertical = 10 },
                    Child = SkinCardDescription.CreateExpandedLabel(Skin.Description, colourProvider),
                };
                c.Expanded.BindTarget = Expanded;
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            versionText.Alpha = string.IsNullOrEmpty(displayVersion) ? 0 : 1;

            description.OverflowChanged += onDescriptionOverflowChanged;
            Schedule(updateDescriptionBounds);
        }

        protected override void Update()
        {
            base.Update();
            updateDescriptionBounds();
        }

        private void updateDescriptionBounds()
        {
            if (!IsLoaded)
                return;

            float availableHeight = contentFlow.DrawHeight - headerFlow.DrawHeight - statisticsContainer.DrawHeight - contentFlow.Spacing.Y * 2;
            availableHeight = Math.Max(0, availableHeight);

            if (Math.Abs(lastAvailableHeight - availableHeight) < 0.5f)
                return;

            lastAvailableHeight = availableHeight;
            description.MaxHeight = availableHeight;
        }

        private void onDescriptionOverflowChanged(bool overflows)
        {
            descriptionOverflows = overflows;
            SetExpansionEnabled(descriptionOverflows, content);
        }

        protected override void UpdateState()
        {
            base.UpdateState();

            bool showDetails = IsHovered || Expanded.Value;

            buttonContainer.ShowDetails.Value = showDetails;
            thumbnail.Dimmed.Value = showDetails;
            description.Alpha = Expanded.Value && descriptionOverflows ? 0 : 1;
        }
    }
}
