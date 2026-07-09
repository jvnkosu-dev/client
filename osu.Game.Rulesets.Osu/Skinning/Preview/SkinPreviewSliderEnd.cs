// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Preview
{
    /// <summary>
    /// Displays a slider end circle at a fixed, non-animated state.
    /// </summary>
    public partial class SkinPreviewSliderEnd : Container
    {
        public SkinPreviewSliderEnd()
        {
            RelativeSizeAxes = Axes.Both;

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.SliderTailHitCircle), _ => Empty())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }
    }
}
