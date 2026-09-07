// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osu.Game.Screens.Marathon;

namespace osu.Game.Tests.Visual.Marathon
{
    [TestFixture]
    public partial class TestSceneMarathonScreen : ScreenTestScene
    {
        public TestSceneMarathonScreen()
        {
            AddStep("show listing", () => LoadScreen(new MarathonScreen()));
            AddStep("show create", () => LoadScreen(new MarathonCreateScreen()));
            AddStep("show detail", () => LoadScreen(new MarathonDetailScreen(new APIMarathon
            {
                Name = "Test marathon",
                Description = "For visual test",
                RankMin = 1000,
                RankMax = 25000,
            })));
            AddStep("show play", () => LoadScreen(new MarathonPlayScreen(new APIMarathon
            {
                Name = "Test marathon",
                Description = "For visual test",
            })));
            AddStep("show summary", () => LoadScreen(new MarathonSummaryScreen(
                new APIMarathon { Name = "Test marathon" },
                createScores(),
                6)));
        }

        private static ScoreInfo[] createScores() => Enumerable.Range(0, 6).Select(i => new ScoreInfo(new BeatmapInfo
        {
            DifficultyName = $"Difficulty {i + 1}",
            Metadata = new BeatmapMetadata
            {
                Title = $"Test song {i + 1}",
                Artist = "Test artist",
            },
        })
        {
            Accuracy = 1 - i * 0.037,
            MaxCombo = 400 + i * 111,
            TotalScore = 900_000 - i * 42_000,
            Rank = i % 4 == 3 ? ScoreRank.D : ScoreRank.S - i % 3,
            Passed = i % 4 != 3,
        }).ToArray();
    }
}
