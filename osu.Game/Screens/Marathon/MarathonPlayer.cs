// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Screens;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Marathon
{
    /// <summary>
    /// Solo play inside a marathon. Scores submit as usual; results auto-continue
    /// to the next map after a short break.
    /// </summary>
    public partial class MarathonPlayer : SoloPlayer
    {
        private readonly MarathonPlayScreen session;

        private bool recorded;

        public MarathonPlayer(MarathonPlayScreen session)
            : base(new PlayerConfiguration
            {
                ShowLeaderboard = true,
                AllowRestart = true,
            })
        {
            this.session = session;
        }

        protected override ResultsScreen CreateResults(ScoreInfo score)
        {
            record(score);
            return session.CreateResultsFor(score);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            // a failed map never reaches results, but should still show up in the run summary.
            if (GameplayState.HasFailed)
                record(Score.ScoreInfo.DeepClone());

            return base.OnExiting(e);
        }

        private void record(ScoreInfo score)
        {
            if (recorded)
                return;

            recorded = true;
            session.RecordResult(score);
        }
    }
}
