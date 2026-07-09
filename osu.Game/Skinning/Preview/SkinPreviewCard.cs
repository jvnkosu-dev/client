// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Skinning.Preview
{
    /// <summary>
    /// A labelled card that scales a skin preview drawable to fit.
    /// </summary>
    public partial class SkinPreviewCard : CompositeDrawable
    {
        public const float CARD_SIZE = 110;

        private readonly string title;
        private readonly ISkin skin;
        private readonly Ruleset ruleset;
        private readonly Func<Drawable> createPreview;

        private Container previewHost = null!;
        private Container previewScaler = null!;

        public SkinPreviewCard(string title, ISkin skin, Ruleset ruleset, Func<Drawable> createPreview)
        {
            this.title = title;
            this.skin = skin;
            this.ruleset = ruleset;
            this.createPreview = createPreview;

            Size = new Vector2(CARD_SIZE);
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(6),
                    Spacing = new Vector2(4),
                    Children = new Drawable[]
                    {
                        previewHost = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = CARD_SIZE - 28,
                            Masking = true,
                            Child = previewScaler = new SkinElementPreviewContainer(skin, ruleset, createPreview())
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                        },
                        new OsuSpriteText
                        {
                            Text = title,
                            Font = OsuFont.Default.With(size: 11),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                    },
                },
            };
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // Keep the scaler centred; only Scale is used to fit content.
            // Position offsets previously pushed everything into the top-left corner.
            previewScaler.Position = Vector2.Zero;

            if (!tryGetContentBounds(previewScaler, out var bounds) && !tryGetDirectChildBounds(previewScaler, out bounds))
            {
                previewScaler.Scale = Vector2.One;
                return;
            }

            float bestScale = Math.Min(
                previewHost.DrawWidth / bounds.Size.X,
                previewHost.DrawHeight / bounds.Size.Y);

            // Avoid upscaling tiny elements like fonts.
            bestScale = Math.Min(bestScale, 1f);

            if (!float.IsFinite(bestScale) || bestScale <= 0)
                bestScale = 1f;

            previewScaler.Scale = new Vector2(bestScale);
        }

        private static bool tryGetContentBounds(Drawable root, out (Vector2 Centre, Vector2 Size) bounds)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            bool hasContent = false;

            foreach (var drawable in root.ChildrenOfType<Drawable>())
            {
                // Relative-sized containers only fill their parent and do not represent
                // the intrinsic size of the skinned content we want to fit.
                if (drawable.RelativeSizeAxes != Axes.None)
                    continue;

                if (!drawable.IsPresent || drawable.Alpha <= 0 || drawable.DrawWidth <= 0 || drawable.DrawHeight <= 0)
                    continue;

                foreach (var vertex in drawable.ScreenSpaceDrawQuad.GetVertices())
                {
                    var local = root.ToLocalSpace(vertex);
                    minX = Math.Min(minX, local.X);
                    minY = Math.Min(minY, local.Y);
                    maxX = Math.Max(maxX, local.X);
                    maxY = Math.Max(maxY, local.Y);
                    hasContent = true;
                }
            }

            if (!hasContent)
            {
                bounds = default;
                return false;
            }

            var size = new Vector2(maxX - minX, maxY - minY);

            if (size.X <= 0 || size.Y <= 0 || !float.IsFinite(size.X) || !float.IsFinite(size.Y))
            {
                bounds = default;
                return false;
            }

            bounds = (new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f), size);
            return true;
        }

        /// <summary>
        /// Fallback when skinned content is entirely relative-sized (e.g. mania stage):
        /// use the direct preview child's laid-out size.
        /// </summary>
        private static bool tryGetDirectChildBounds(Drawable root, out (Vector2 Centre, Vector2 Size) bounds)
        {
            bounds = default;

            if (root is not Container { Child: Drawable child })
                return false;

            if (!child.IsPresent || child.Alpha <= 0 || child.DrawWidth <= 0 || child.DrawHeight <= 0)
                return false;

            bounds = (child.DrawRectangle.Centre, child.DrawRectangle.Size);
            return true;
        }
    }
}
