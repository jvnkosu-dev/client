// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osu.Game.Skinning.Preview;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Preview
{
    public partial class CatchSkinPreviewGallery : Container
    {
        public CatchSkinPreviewGallery(ISkin skin, Ruleset ruleset)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Child = new FillFlowContainer
            {
                Direction = FillDirection.Full,
                Spacing = new Vector2(10),
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(0),
                Children = new Drawable[]
                {
                    new SkinPreviewCard("Catcher", skin, ruleset, () => new SkinPreviewCatchCatcher()),
                    new SkinPreviewCard("Fruit", skin, ruleset, () => new SkinPreviewCatchFruit()),
                    new SkinPreviewCard("Droplet", skin, ruleset, () => new SkinPreviewCatchDroplet()),
                    new SkinPreviewCard("Banana", skin, ruleset, () => new SkinPreviewCatchBanana()),
                    new SkinPreviewCard("Font", skin, ruleset, () => new SkinPreviewCatchFont(skin)),
                },
            };
        }
    }
}
