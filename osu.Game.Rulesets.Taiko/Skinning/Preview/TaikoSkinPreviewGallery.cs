// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;
using osu.Game.Skinning.Preview;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Preview
{
    public partial class TaikoSkinPreviewGallery : Container
    {
        public TaikoSkinPreviewGallery(ISkin skin, Ruleset ruleset)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Child = new TaikoSkinPreviewDependencyHost(new FillFlowContainer
            {
                Direction = FillDirection.Full,
                Spacing = new Vector2(10),
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(0),
                Children = new Drawable[]
                {
                    new SkinPreviewCard("Drums", skin, ruleset, () => new SkinPreviewTaikoDrums()),
                    new SkinPreviewCard("Notes", skin, ruleset, () => new SkinPreviewTaikoNotes()),
                    new SkinPreviewCard("Roll", skin, ruleset, () => new SkinPreviewTaikoRoll()),
                    new SkinPreviewCard("Spinner", skin, ruleset, () => new SkinPreviewTaikoSwell()),
                    new SkinPreviewCard("Font", skin, ruleset, () => new SkinPreviewTaikoFont(skin)),
                    new SkinPreviewCard("300", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Great, r => new DefaultJudgementPiece(r))),
                    new SkinPreviewCard("100", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Ok, r => new DefaultJudgementPiece(r))),
                    new SkinPreviewCard("Miss", skin, ruleset, () => new SkinPreviewJudgement(HitResult.Miss, r => new DefaultJudgementPiece(r))),
                },
            });
        }
    }
}
