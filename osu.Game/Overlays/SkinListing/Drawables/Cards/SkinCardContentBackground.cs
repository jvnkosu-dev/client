using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.SkinListing.Drawables;
using osu.Game.Skinning.Preview;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class SkinCardContentBackground : CompositeDrawable
    {
        public BindableBool Dimmed { get; } = new BindableBool();

        private readonly APIOnlineSkin skin;
        private readonly bool keepLoaded;

        private Box background = null!;
        private Container cover = null!;
        private Container parallaxWrapper = null!;

        private float? lockedCoverWidth;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OnlineSkinPreviewProvider previewProvider { get; set; } = null!;

        public SkinCardContentBackground(APIOnlineSkin skin, bool keepLoaded = false)
        {
            this.skin = skin;
            this.keepLoaded = keepLoaded;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            string? thumbnailUrl = skin.GetThumbnailRequestUrl();

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background2,
                },
                cover = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.Gray(0.2f),
                        },
                        parallaxWrapper = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = new SkinDelayedLoadUnloadWrapper(() => new OnlineSkinSprite(thumbnailUrl)
                            {
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fill,
                            }, previewProvider, skin.OnlineID, keepLoaded ? 0 : 500, keepLoaded ? double.MaxValue : 1000)
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Dimmed.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        protected override void Update()
        {
            base.Update();

            if (lockedCoverWidth == null && cover.DrawWidth > 0)
                lockedCoverWidth = cover.DrawWidth;

            if (lockedCoverWidth == null || cover.DrawHeight <= 0)
                return;

            parallaxWrapper.Size = new Vector2(lockedCoverWidth.Value, cover.DrawHeight);
        }

        private void updateState() => Schedule(() =>
        {
            background.FadeColour(Dimmed.Value ? colourProvider.Background4 : colourProvider.Background2, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

            var gradient = ColourInfo.GradientHorizontal(Colour4.White.Opacity(0), Colour4.White.Opacity(0.2f));
            cover.FadeColour(gradient, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        });
    }
}
