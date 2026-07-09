// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Preview
{
    /// <summary>
    /// Displays a hit circle together with its approach circle at a fixed, non-animated state.
    /// </summary>
    public partial class SkinPreviewHitCircle : Container
    {
        public SkinPreviewHitCircle()
        {
            RelativeSizeAxes = Axes.Both;

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.HitCircle), _ => new MainCirclePiece())
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(0.5f),
                    },
                    new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.ApproachCircle), _ => new DefaultApproachCircle())
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.9f,
                        Scale = new Vector2(0.6f),
                    },
                },
            };
        }
    }
}
