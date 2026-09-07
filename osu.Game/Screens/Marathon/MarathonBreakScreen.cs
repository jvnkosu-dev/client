// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.Marathon
{
    /// <summary>
    /// Intermediate marathon results. Solo ranking stays unchanged; this screen
    /// is only used between maps in a run. The final map goes to
    /// <see cref="MarathonSummaryScreen"/> instead.
    /// </summary>
    public partial class MarathonBreakScreen : ResultsScreen
    {
        public const double BREAK_DURATION = 12000;

        private const double skip_after = 4000;
        private const float content_width = 560;
        private const string break_track = @"rankedplay_bgm.ogg";

        private readonly MarathonPlayScreen session;
        private readonly List<Drawable> revealTargets = new List<Drawable>();

        private OsuSpriteText countdownText = null!;
        private MarathonScoreCounter scoreCounter = null!;
        private UpdateableRank rankDisplay = null!;
        private MarathonActionButton continueButton = null!;
        private DrawableTrack? breakTrack;
        private double startTime;
        private bool continued;
        private bool restoreMusic = true;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        public MarathonBreakScreen(ScoreInfo score, MarathonPlayScreen session)
            : base(score)
        {
            this.session = session;

            AllowRetry = false;
            AllowWatchingReplay = false;
            AllowBeatmapActions = false;
            IsLocalPlay = true;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OverlayColourProvider colourProvider)
        {
            var metadata = Score?.BeatmapInfo?.Metadata;
            string title = metadata?.Title ?? Score?.BeatmapInfo?.GetDisplayTitle() ?? "Unknown map";
            string artist = metadata?.Artist ?? string.Empty;

            AddInternal(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MinValue,
                // leave the button strip uncovered so it stays readable and clickable.
                Padding = new MarginPadding { Bottom = TwoLayerButton.SIZE_EXTENDED.Y },
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5.Opacity(0.82f),
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = content_width,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 6),
                        Children = new[]
                        {
                            reveal(new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = MarathonStrings.MapProgress(session.CurrentIndex + 1, session.TotalMaps),
                                Font = OsuFont.TorusAlternate.With(size: 20, weight: FontWeight.SemiBold),
                                Colour = colourProvider.Content2,
                            }, 26),
                            reveal(countdownText = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Font = OsuFont.TorusAlternate.With(size: 16),
                                Colour = colourProvider.Content1,
                            }, 22),
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 96,
                                Margin = new MarginPadding { Vertical = 4 },
                                Child = rankDisplay = new UpdateableRank(Score?.Rank)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(160, 80),
                                    Alpha = 0,
                                },
                            },
                            reveal(new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = title,
                                Font = OsuFont.GetFont(size: 28, weight: FontWeight.SemiBold),
                                Shadow = true,
                            }, 36),
                            reveal(new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = artist,
                                Font = OsuFont.GetFont(size: 16),
                                Colour = colourProvider.Content2,
                            }, string.IsNullOrEmpty(artist) ? 0 : 22),
                            reveal(scoreCounter = new MarathonScoreCounter
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Margin = new MarginPadding { Top = 8 },
                            }, 54),
                            reveal(new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(24, 0),
                                Children = new Drawable[]
                                {
                                    stat("ACC", Score != null ? $"{Score.Accuracy:0.00%}" : "-"),
                                    stat("COMBO", Score != null ? $"{Score.MaxCombo}x" : "-"),
                                    stat("PP", Score?.PP != null ? $"{Score.PP:0}" : "-"),
                                }
                            }, 48),
                        }
                    }
                }
            });

            BottomPanelButtons.Add(continueButton = new MarathonActionButton(FontAwesome.Solid.Forward, MarathonStrings.NextMapButton)
            {
                Width = 300,
                Alpha = 0,
                Action = requestContinue,
            });

            Track? track = audio.Tracks.Get(break_track);

            if (track != null)
            {
                AddInternal(breakTrack = new DrawableTrack(track)
                {
                    Looping = true,
                });
            }
        }

        /// <summary>
        /// Wraps a drawable in a fixed-height row so it can be moved freely during the reveal
        /// animation without the surrounding flow re-laying out around it.
        /// </summary>
        private Drawable reveal(Drawable content, float height)
        {
            content.Alpha = 0;
            content.Y = 24;
            revealTargets.Add(content);

            return new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = height,
                Child = content,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            VerticalScrollContent.Hide();
        }

        protected override Task<ScoreInfo[]> FetchScores() => Task.FromResult(Array.Empty<ScoreInfo>());

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            startTime = Time.Current;

            musicController.AllowTrackControl.Value = false;
            musicController.Stop();

            breakTrack?.VolumeTo(0)
                      .VolumeTo(0.4, 800, Easing.OutQuint);
            breakTrack?.Start();

            animateIn();

            Scheduler.AddDelayed(() => continueButton.FadeIn(300, Easing.OutQuint), skip_after);
            Scheduler.AddDelayed(requestContinue, BREAK_DURATION);
        }

        private void animateIn()
        {
            double delay = 0;

            foreach (var target in revealTargets)
            {
                target.Delay(delay).FadeIn(400, Easing.OutQuint);
                target.Delay(delay).MoveToY(0, 700, Easing.OutQuint);
                delay += 90;
            }

            const double rank_delay = 250;

            rankDisplay.Delay(rank_delay).FadeIn(300, Easing.OutQuint);
            rankDisplay.ScaleTo(1.6f).Delay(rank_delay).ScaleTo(1, 900, Easing.OutElasticHalf);

            Scheduler.AddDelayed(() => scoreCounter.Current.Value = Score?.TotalScore ?? 0, 700);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            breakTrack?.Stop();
            musicController.AllowTrackControl.Value = true;

            if (restoreMusic)
                musicController.EnsurePlayingSomething();

            return base.OnExiting(e);
        }

        protected override void Update()
        {
            base.Update();

            int remaining = Math.Max(0, (int)Math.Ceiling((BREAK_DURATION - (Time.Current - startTime)) / 1000.0));
            countdownText.Text = MarathonStrings.NextMapIn(remaining);
        }

        private void requestContinue()
        {
            if (continued || !this.IsCurrentScreen())
                return;

            continued = true;
            restoreMusic = false;
            session.ContinueRequested = true;
            this.Exit();
        }

        private static Drawable stat(LocalisableString label, string value) => new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(2),
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = label,
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                    Colour = Colour4.White.Opacity(0.6f),
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = value,
                    Font = OsuFont.TorusAlternate.With(size: 22, weight: FontWeight.SemiBold),
                }
            }
        };

        private partial class MarathonScoreCounter : RollingCounter<long>
        {
            protected override double RollingDuration => 1200;

            protected override Easing RollingEasing => Easing.OutQuint;

            protected override LocalisableString FormatCount(long count) => count.ToLocalisableString(@"N0");

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Font = OsuFont.TorusAlternate.With(size: 36, weight: FontWeight.Bold),
            };
        }
    }
}
