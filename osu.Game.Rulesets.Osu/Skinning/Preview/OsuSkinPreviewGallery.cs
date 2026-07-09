// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osu.Game.Skinning.Preview;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Preview
{
    /// <summary>
    /// A grid gallery of osu!-specific skin element previews.
    /// </summary>
    public partial class OsuSkinPreviewGallery : Container
    {
        public OsuSkinPreviewGallery(ISkin skin, Ruleset ruleset)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Child = new SkinPreviewDependencyHost(new FillFlowContainer
            {
                Direction = FillDirection.Full,
                Spacing = new Vector2(10),
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(0),
                Children = new Drawable[]
                {
                    new SkinPreviewCard("Hit circle", skin, ruleset, () => new SkinPreviewHitCircle()),
                    new SkinPreviewCard("Font", skin, ruleset, () => new SkinPreviewFont(skin)),
                    new SkinPreviewCard("300", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Great, r => new DefaultJudgementPiece(r))),
                    new SkinPreviewCard("100", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Ok, r => new DefaultJudgementPiece(r))),
                    new SkinPreviewCard("50", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Meh, r => new DefaultJudgementPiece(r))),
                    new SkinPreviewCard("Miss", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Miss, r => new DefaultJudgementPiece(r))),
                },
            });
        }
    }
}
