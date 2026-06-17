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
    public partial class SkinCardNormal : SkinCard
    {
        protected override Drawable IdleContent => idleBottomContent;
        protected override Drawable DownloadInProgressContent => downloadProgressBar;

        public const float HEIGHT = BeatmapCardNormal.HEIGHT;

        [Cached]
        private readonly BeatmapCardContent content;

        private SkinCardThumbnail thumbnail = null!;
        private SkinCollapsibleButtonContainer buttonContainer = null!;

        private FillFlowContainer idleBottomContent = null!;
        private FillFlowContainer<BeatmapCardStatistic> statisticsContainer = null!;
        private BeatmapCardDownloadProgressBar downloadProgressBar = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public SkinCardNormal(APIOnlineSkin skin, bool allowExpansion = true)
            : base(skin, false)
        {
            content = new BeatmapCardContent(HEIGHT);
            Action = DefaultAction;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = WIDTH;
            Height = HEIGHT;

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
                            Size = new Vector2(HEIGHT),
                            Padding = new MarginPadding { Right = CORNER_RADIUS },
                        },
                        buttonContainer = new SkinCollapsibleButtonContainer(Skin, DownloadTracker.State)
                        {
                            FavouriteState = { BindTarget = FavouriteState },
                            X = HEIGHT - CORNER_RADIUS,
                            Width = WIDTH - HEIGHT + CORNER_RADIUS,
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
                                        new SkinCardUploaderLine(Skin),
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
                                            Spacing = new Vector2(0, 2),
                                            AlwaysPresent = true,
                                            Children = new Drawable[]
                                            {
                                                statisticsContainer = new FillFlowContainer<BeatmapCardStatistic>
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(8, 0),
                                                    Alpha = 0,
                                                    AlwaysPresent = true,
                                                    ChildrenEnumerable = SkinCardStatistics.CreateFor(Skin, FavouriteState),
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
        }

        protected override void UpdateState()
        {
            base.UpdateState();

            bool showDetails = IsHovered || Expanded.Value;

            buttonContainer.ShowDetails.Value = showDetails;
            thumbnail.Dimmed.Value = showDetails;
            statisticsContainer.FadeTo(showDetails ? 1 : 0, TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
