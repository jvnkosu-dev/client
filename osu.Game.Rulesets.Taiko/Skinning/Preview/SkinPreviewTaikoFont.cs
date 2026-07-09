// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Preview
{
    public partial class SkinPreviewTaikoFont : Container
    {
        private readonly ISkin skin;

        public SkinPreviewTaikoFont(ISkin skin)
        {
            this.skin = skin;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skinSource)
        {
            Child = createPreviewText(skinSource);
        }

        private Drawable createPreviewText(ISkinSource skinSource)
        {
            if (skin is ArgonSkin)
            {
                return new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Default.With(size: 24, weight: FontWeight.Bold),
                    Text = "300",
                };
            }

            if (skin is TrianglesSkin)
            {
                return new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Numeric.With(size: 24),
                    Text = "300",
                };
            }

            if (skin is LegacySkin && skinSource.HasFont(LegacyFont.Score))
            {
                return new LegacySpriteText(LegacyFont.Score)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "300",
                };
            }

            return new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Numeric.With(size: 24),
                Text = "300",
            };
        }
    }
}
