// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osu.Game.Skinning.Preview;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning.Preview
{
    public partial class ManiaSkinPreviewGallery : Container
    {
        public ManiaSkinPreviewGallery(ISkin skin, Ruleset ruleset)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Child = new ManiaSkinPreviewDependencyHost(new FillFlowContainer
            {
                Direction = FillDirection.Full,
                Spacing = new Vector2(10),
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(0),
                Children = new Drawable[]
                {
                    new SkinPreviewCard("5K stage", skin, ruleset, () => new SkinPreviewManiaStage()),
                    new SkinPreviewCard("Font", skin, ruleset, () => new SkinPreviewManiaFont(skin)),
                    new SkinPreviewCard("Perfect", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Perfect, r => new DefaultManiaJudgementPiece(r), suppressGameplayPositioning: true)),
                    new SkinPreviewCard("Great", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Great, r => new DefaultManiaJudgementPiece(r), suppressGameplayPositioning: true)),
                    new SkinPreviewCard("Good", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Good, r => new DefaultManiaJudgementPiece(r), suppressGameplayPositioning: true)),
                    new SkinPreviewCard("Ok", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Ok, r => new DefaultManiaJudgementPiece(r), suppressGameplayPositioning: true)),
                    new SkinPreviewCard("Meh", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Meh, r => new DefaultManiaJudgementPiece(r), suppressGameplayPositioning: true)),
                    new SkinPreviewCard("Miss", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Miss, r => new DefaultManiaJudgementPiece(r), suppressGameplayPositioning: true)),
                },
            });
        }
    }
}
