// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Skinning.Preview
{
    /// <summary>
    /// A minimal beatmap used when rendering skin element previews.
    /// </summary>
    public static class SkinPreviewBeatmap
    {
        public static IBeatmap Create(Ruleset ruleset) => ruleset.CreatePreviewBeatmap();

        public static IBeatmap CreateDefault(Ruleset ruleset) => new Beatmap
        {
            BeatmapInfo = new BeatmapInfo
            {
                Ruleset = ruleset.RulesetInfo,
                Difficulty = new BeatmapDifficulty
                {
                    CircleSize = 5,
                    OverallDifficulty = 8,
                },
            },
        };
    }
}
