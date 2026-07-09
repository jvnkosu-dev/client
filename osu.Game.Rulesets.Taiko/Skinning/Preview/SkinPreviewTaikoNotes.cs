// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Preview
{
    public partial class SkinPreviewTaikoNotes : Container
    {
        public SkinPreviewTaikoNotes()
        {
            RelativeSizeAxes = Axes.Both;

            Child = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(8),
                Children = new Drawable[]
                {
                    new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.CentreHit), _ => new CentreHitCirclePiece())
                    {
                        RelativeSizeAxes = Axes.None,
                        Size = new Vector2(64),
                    },
                    new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.RimHit), _ => new RimHitCirclePiece())
                    {
                        RelativeSizeAxes = Axes.None,
                        Size = new Vector2(64),
                    },
                },
            };
        }
    }
}
