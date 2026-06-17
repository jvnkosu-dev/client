using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class SkinCardNano : SkinCard
    {
        protected override Drawable IdleContent => idleBottomContent;
        protected override Drawable DownloadInProgressContent => downloadProgressBar;

        private const float height = 60;
        private const float width = 300;

        [Cached]
        private readonly BeatmapCardContent content;

        private SkinCollapsibleButtonContainer buttonContainer = null!;

        private FillFlowContainer idleBottomContent = null!;
        private BeatmapCardDownloadProgressBar downloadProgressBar = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public SkinCardNano(APIOnlineSkin skin)
            : base(skin, false)
        {
            content = new BeatmapCardContent(height);
            Action = DefaultAction;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = width;
            Height = height;

            Child = content.With(c =>
            {
                c.MainContent = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = height,
                    Children = new Drawable[]
                    {
                        buttonContainer = new SkinCollapsibleButtonContainer(Skin, DownloadTracker.State)
                        {
                            Width = Width,
                            ButtonsCollapsedWidth = 5,
                            ButtonsExpandedWidth = 30,
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
                                            Font = OsuFont.Default.With(size: 19, weight: FontWeight.SemiBold),
                                            RelativeSizeAxes = Axes.X,
                                        },
                                        new TruncatingSpriteText
                                        {
                                            Text = $"by {Skin.Creator}",
                                            Font = OsuFont.Default.With(size: 16, weight: FontWeight.SemiBold),
                                            Colour = colourProvider.Light1,
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
                                            Direction = FillDirection.Vertical,
                                            AlwaysPresent = true,
                                        },
                                        downloadProgressBar = new BeatmapCardDownloadProgressBar
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 6,
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
            buttonContainer.ShowDetails.Value = IsHovered;
        }
    }
}
