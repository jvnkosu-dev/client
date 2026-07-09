// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.SkinListing.Drawables;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class SkinCardThumbnail : Container
    {
        public BindableBool Dimmed { get; } = new BindableBool();

        public new MarginPadding Padding
        {
            get => foreground.Padding;
            set => foreground.Padding = value;
        }

        private readonly Box background;
        private readonly Container foreground;
        private readonly SkinCardHitCirclePreview hitCirclePreview;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public SkinCardThumbnail(APIOnlineSkin skin, bool keepLoaded = false)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f),
                },
                new UpdateableOnlineSkinCover(keepLoaded ? 0 : 500, keepLoaded ? double.MaxValue : 1000)
                {
                    RelativeSizeAxes = Axes.Both,
                    Skin = skin,
                },
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                foreground = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = hitCirclePreview = new SkinCardHitCirclePreview(skin)
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colourProvider.Background6;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Dimmed.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            bool shouldDim = Dimmed.Value;

            hitCirclePreview.SetDimmed(shouldDim);
            background.FadeColour(colourProvider.Background6.Opacity(shouldDim ? 0.6f : 0f), BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
