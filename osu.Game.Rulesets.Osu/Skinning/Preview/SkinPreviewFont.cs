// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning.Preview
{
    /// <summary>
    /// Displays the font used by HUD elements such as score and accuracy counters.
    /// </summary>
    public partial class SkinPreviewFont : Container
    {
        private readonly ISkin skin;

        public SkinPreviewFont(ISkin skin)
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
                    Text = "3G",
                };
            }

            if (skin is TrianglesSkin)
            {
                return new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Numeric.With(size: 24),
                    Text = "3G",
                };
            }

            if (skin is LegacySkin && skinSource.HasFont(LegacyFont.Score))
            {
                return new LegacySpriteText(LegacyFont.Score)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "3G",
                };
            }

            return new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Numeric.With(size: 24),
                Text = "3G",
            };
        }
    }
}
