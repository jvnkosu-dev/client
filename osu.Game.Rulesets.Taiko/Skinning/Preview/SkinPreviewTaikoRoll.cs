// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Preview
{
    public partial class SkinPreviewTaikoRoll : Container
    {
        public SkinPreviewTaikoRoll()
        {
            RelativeSizeAxes = Axes.Both;

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(90, 48),
                Child = new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.DrumRollBody), _ => new ElongatedCirclePiece())
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }
    }
}
