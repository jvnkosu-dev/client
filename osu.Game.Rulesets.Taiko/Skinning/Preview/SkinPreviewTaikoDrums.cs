// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Preview
{
    public partial class SkinPreviewTaikoDrums : Container
    {
        public SkinPreviewTaikoDrums()
        {
            RelativeSizeAxes = Axes.Both;

            Child = new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.InputDrum), _ => new DefaultInputDrum())
            {
                RelativeSizeAxes = Axes.Both,
            };
        }
    }
}
