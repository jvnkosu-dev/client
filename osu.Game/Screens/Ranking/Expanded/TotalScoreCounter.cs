// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded
{
    /// <summary>
    /// A counter for the player's total score to be displayed in the <see cref="ExpandedPanelMiddleContent"/>.
    /// </summary>
    public partial class TotalScoreCounter : RollingCounter<long>
    {
        protected override double RollingDuration => AccuracyCircle.ACCURACY_TRANSFORM_DURATION;

        protected override Easing RollingEasing => AccuracyCircle.ACCURACY_TRANSFORM_EASING;

        private readonly bool playSamples;

        private readonly Bindable<double> tickPlaybackRate = new Bindable<double>();

        private ScoreInfo score;
        private Bindable<ScoringMode> scoringMode = new Bindable<ScoringMode>(ScoringMode.Standardised);
        private ScoringMode mode => scoringMode.Value;

        private double lastSampleTime;

        private DrawableSample sampleTick = null!;
        private ArgonCounterTextComponent counter = null!;

        public TotalScoreCounter(bool playSamples = false, ScoreInfo? score = null)
        {
            // Todo: AutoSize X removed here due to https://github.com/ppy/osu-framework/issues/3369
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            this.playSamples = playSamples;
            this.score = score ?? new ScoreInfo();
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager? config)
        {
            AddInternal(sampleTick = new DrawableSample(audio.Samples.Get(@"Results/score-tick-lesser")));
            scoringMode.BindTo(
                config?.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode)
            );
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoringMode.BindValueChanged(_ => updateWireframe(), true);


            if (playSamples)
                Current.BindValueChanged(_ => startTicking());
        }

        protected override LocalisableString FormatCount(long count) => count.ToString("N0", CultureInfo.CreateSpecificCulture("en-US")).Replace(',', '.'); // XXX: make this look okay

        private void updateWireframe()
        {
            string getWireframe(long sc) => (sc >= 100000)
                                                ? FormatCount(sc).ToString()
                                                : "###.###";

            long dispScore = Scoring.Legacy.ScoreInfoExtensions.GetDisplayScore(score, mode);
            counter.WireframeTemplate = getWireframe(dispScore);
        }

        protected override ArgonCounterTextComponent CreateText()
        {
            counter = new ArgonCounterTextComponent(Anchor.Centre);
            counter.WireframeOpacity.BindTo(new BindableFloat(0.25f));
            counter.WireframeTemplate = "###.###";

            return counter;
        }

        public override long DisplayedCount
        {
            get => base.DisplayedCount;
            set
            {
                if (base.DisplayedCount == value)
                    return;

                base.DisplayedCount = value;

                if (playSamples && Time.Current > lastSampleTime + tickPlaybackRate.Value)
                {
                    sampleTick?.Play();
                    lastSampleTime = Time.Current;
                }
            }
        }

        private void startTicking()
        {
            const double tick_debounce_rate_start = 10f;
            const double tick_debounce_rate_end = 100f;
            const double tick_volume_start = 0.5f;
            const double tick_volume_end = 1.0f;

            this.TransformBindableTo(tickPlaybackRate, tick_debounce_rate_start);
            this.TransformBindableTo(tickPlaybackRate, tick_debounce_rate_end, RollingDuration, Easing.OutSine);
            sampleTick.VolumeTo(tick_volume_start).Then().VolumeTo(tick_volume_end, RollingDuration, Easing.OutSine);
        }
    }
}
