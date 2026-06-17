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
        private const float bottom_content_spacing = 2f;

        [Cached]
        private readonly BeatmapCardContent content;

        private SkinCardThumbnail thumbnail = null!;
        private SkinCollapsibleButtonContainer buttonContainer = null!;

        private GridContainer statisticsContainer = null!;
        private FillFlowContainer idleBottomContent = null!;
        private BeatmapCardDownloadProgressBar downloadProgressBar = null!;
        private TruncatingSpriteText versionText = null!;

        private string displayVersion = string.Empty;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public SkinCardExtra(APIOnlineSkin skin, bool allowExpansion = true)
            : base(skin, false)
        {
            content = new BeatmapCardContent(height);
            Action = DefaultAction;
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
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new TruncatingSpriteText
                                        {
                                            Text = SkinIniVersionHelper.GetDisplayName(Skin.Name),
                                            Font = OsuFont.Default.With(size: 18f, weight: FontWeight.SemiBold),
                                            RelativeSizeAxes = Axes.X,
                                        },
                                        new TruncatingSpriteText
                                        {
                                            Text = $"by {Skin.Creator}",
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
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, bottom_content_spacing),
                                            AlwaysPresent = true,
                                            Children = new Drawable[]
                                            {
                                                new SkinCardUploaderLine(Skin),
                                                statisticsContainer = new GridContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    RowDimensions = new[]
                                                    {
                                                        new Dimension(GridSizeMode.AutoSize),
                                                    },
                                                    ColumnDimensions = new[]
                                                    {
                                                        new Dimension(GridSizeMode.AutoSize),
                                                        new Dimension(GridSizeMode.AutoSize),
                                                        new Dimension()
                                                    },
                                                    Content = new[]
                                                    {
                                                        new Drawable[3],
                                                    }
                                                },
                                                new SkinCardModifiedModesDisplay(Skin),
                                            }
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
                c.ExpandedContent = new Container();
                c.Expanded.BindTarget = Expanded;
            });

            createStatistics();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            versionText.Alpha = string.IsNullOrEmpty(displayVersion) ? 0 : 1;
        }

        private void createStatistics()
        {
            BeatmapCardStatistic withMargin(BeatmapCardStatistic original)
            {
                original.Margin = new MarginPadding { Right = 8 };
                return original;
            }

            statisticsContainer.Content[0][0] = withMargin(new SkinDownloadCountStatistic(Skin));
            statisticsContainer.Content[0][1] = withMargin(new SkinFavouritesStatistic(Skin) { Current = { BindTarget = FavouriteState } });

            var dateStatistic = SkinCardDateStatistic.CreateFor(Skin);
            if (dateStatistic != null)
                statisticsContainer.Content[0][2] = withMargin(dateStatistic);
        }

        protected override void UpdateState()
        {
            base.UpdateState();

            bool showDetails = IsHovered || Expanded.Value;

            buttonContainer.ShowDetails.Value = showDetails;
            thumbnail.Dimmed.Value = showDetails;
        }
    }
}
