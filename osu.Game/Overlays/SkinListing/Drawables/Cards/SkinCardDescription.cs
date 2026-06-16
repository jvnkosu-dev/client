using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class SkinCardDescription : CompositeDrawable
    {
        public Action<bool>? OverflowChanged;

        private readonly string descriptionText;

        private Container clipContainer = null!;
        private OsuSpriteText descriptionLabel = null!;

        private float maxHeight = float.PositiveInfinity;
        private float lastLayoutWidth;
        private bool lastOverflow;

        public float MaxHeight
        {
            get => maxHeight;
            set
            {
                if (Math.Abs(maxHeight - value) < 0.5f)
                    return;

                maxHeight = value;
                invalidateLayout();
            }
        }

        public static string Format(string? description) =>
            string.IsNullOrWhiteSpace(description) ? "Нет описания..." : description.Trim();

        public SkinCardDescription(string? description)
        {
            descriptionText = Format(description);
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChild = clipContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                Masking = true,
                Child = descriptionLabel = createLabel(colourProvider),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            invalidateLayout();
        }

        private void invalidateLayout()
        {
            if (!IsLoaded)
                return;

            Schedule(updateLayout);
        }

        private void updateLayout()
        {
            if (!IsLoaded)
                return;

            if (DrawWidth <= 0)
                return;

            if (Math.Abs(lastLayoutWidth - DrawWidth) > 0.5f)
            {
                descriptionLabel.Width = DrawWidth;
                lastLayoutWidth = DrawWidth;
            }

            float fullHeight = descriptionLabel.DrawHeight;

            if (float.IsPositiveInfinity(maxHeight))
            {
                clipContainer.Height = fullHeight;
                setOverflow(false);
                return;
            }

            bool overflows = fullHeight > maxHeight + 0.5f;
            clipContainer.Height = Math.Min(fullHeight, maxHeight);
            setOverflow(overflows);
        }

        private void setOverflow(bool overflows)
        {
            if (overflows == lastOverflow)
                return;

            lastOverflow = overflows;
            Schedule(() => OverflowChanged?.Invoke(overflows));
        }

        private OsuSpriteText createLabel(OverlayColourProvider colourProvider) =>
            new OsuSpriteText
            {
                AllowMultiline = true,
                Shadow = false,
                Font = OsuFont.Default.With(size: 12f),
                Colour = colourProvider.Content2,
                Text = descriptionText,
            };

        public static OsuSpriteText CreateExpandedLabel(string? description, OverlayColourProvider colourProvider) =>
            new OsuSpriteText
            {
                RelativeSizeAxes = Axes.X,
                AllowMultiline = true,
                Shadow = false,
                Font = OsuFont.Default.With(size: 12f),
                Colour = colourProvider.Content1,
                Text = Format(description),
            };
    }
}
