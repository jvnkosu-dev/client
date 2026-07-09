// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Skinning.Preview
{
    /// <summary>
    /// Displays a judgement skin element in a static preview state.
    /// </summary>
    public partial class SkinPreviewJudgement : Container
    {
        private readonly HitResult result;
        private readonly Func<HitResult, Drawable> createDefaultPiece;
        private readonly bool suppressGameplayPositioning;

        private SkinnableDrawable judgementBody = null!;

        public SkinPreviewJudgement(HitResult result, Func<HitResult, Drawable> createDefaultPiece, bool suppressGameplayPositioning = false)
        {
            this.result = result;
            this.createDefaultPiece = createDefaultPiece;
            this.suppressGameplayPositioning = suppressGameplayPositioning;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = judgementBody = new SkinnableDrawable(
                new SkinComponentLookup<HitResult>(result),
                _ => createDefaultPiece(result),
                confineMode: ConfineMode.NoScaling)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                CentreComponent = true,
            };

            judgementBody.OnSkinChanged += resetStaticState;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            resetStaticState();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!suppressGameplayPositioning)
                return;

            var drawable = judgementBody.Drawable;

            if (drawable == null)
                return;

            drawable.Anchor = Anchor.Centre;
            drawable.Origin = Anchor.Centre;
            drawable.Y = 0;
        }

        private void resetStaticState()
        {
            if (judgementBody.Drawable == null)
                return;

            judgementBody.Drawable.ClearTransforms(true);
            judgementBody.Drawable.Alpha = 1;
            judgementBody.Drawable.Scale = Vector2.One;

            if (suppressGameplayPositioning)
            {
                judgementBody.Drawable.Anchor = Anchor.Centre;
                judgementBody.Drawable.Origin = Anchor.Centre;
                judgementBody.Drawable.Y = 0;
            }
        }
    }
}
