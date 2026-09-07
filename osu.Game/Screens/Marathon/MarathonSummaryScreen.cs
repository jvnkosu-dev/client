// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.Marathon
{
    /// <summary>
    /// Summary of a whole marathon run. Takes the place of the results screen for the final
    /// map, so it shows up the moment gameplay ends and keeps that map's background.
    /// Overall numbers are revealed first, then the per-map breakdown slides in alongside them.
    /// </summary>
    public partial class MarathonSummaryScreen : ResultsScreen
    {
        /// <summary>
        /// How long the overall stats stay centred before the map list is revealed.
        /// </summary>
        private const double list_reveal_delay = 3500;

        private const float overall_width = 420;
        private const float list_width = 480;
        private const float overall_shift = 270;
        private const string break_track = @"rankedplay_bgm.ogg";

        private readonly APIMarathon marathon;
        private readonly IReadOnlyList<ScoreInfo> scores;
        private readonly int poolSize;
        private readonly List<Drawable> overallReveal = new List<Drawable>();

        private OverlayColourProvider colourProvider = null!;
        private Container overallContainer = null!;
        private Container listContainer = null!;
        private MarathonActionButton showMapsButton = null!;
        private MarathonActionButton finishButton = null!;
        private FillFlowContainer listFlow = null!;
        private DrawableTrack? breakTrack;

        private bool listRevealed;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        public MarathonSummaryScreen(APIMarathon marathon, IReadOnlyList<ScoreInfo> scores, int poolSize)
            // the final map's score keeps the results screen showing that map's background.
            : base(scores.Count > 0 ? scores[^1] : null)
        {
            this.marathon = marathon;
            this.scores = scores;
            this.poolSize = poolSize;

            AllowRetry = false;
            AllowWatchingReplay = false;
            AllowBeatmapActions = false;
            IsLocalPlay = true;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OverlayColourProvider colourProvider)
        {
            this.colourProvider = colourProvider;

            AddInternal(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MinValue,
                // leave the button strip uncovered so it stays readable and clickable.
                Padding = new MarginPadding { Bottom = TwoLayerButton.SIZE_EXTENDED.Y },
                Children = new Drawable[]
                {
                    // dims the last map's background just enough to read over without hiding it.
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5.Opacity(0.6f),
                    },
                    overallContainer = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = overall_width,
                        AutoSizeAxes = Axes.Y,
                        Child = createOverallContent(),
                    },
                    listContainer = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreLeft,
                        X = overall_width / 2 - overall_shift + 40,
                        Width = list_width,
                        RelativeSizeAxes = Axes.Y,
                        Height = 0.82f,
                        Alpha = 0,
                        Child = new OsuScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ScrollbarVisible = false,
                            Child = listFlow = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 6),
                            },
                        },
                    },
                }
            });

            // both buttons share a slot so swapping between them doesn't shift anything.
            BottomPanelButtons.Add(new Container
            {
                Size = new Vector2(300, 30),
                Children = new Drawable[]
                {
                    showMapsButton = new MarathonActionButton(FontAwesome.Solid.AngleDoubleRight, MarathonStrings.ShowMapsButton)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = Vector2.One,
                        Action = revealList,
                    },
                    finishButton = new MarathonActionButton(FontAwesome.Solid.Check, MarathonStrings.FinishButton)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = Vector2.One,
                        Alpha = 0,
                        Action = () => this.Exit(),
                    },
                }
            });

            buildMapList();

            Track? track = audio.Tracks.Get(break_track);

            if (track != null)
            {
                AddInternal(breakTrack = new DrawableTrack(track)
                {
                    Looping = true,
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            VerticalScrollContent.Hide();
        }

        protected override Task<ScoreInfo[]> FetchScores() => Task.FromResult(Array.Empty<ScoreInfo>());

        private Drawable createOverallContent()
        {
            long totalScore = scores.Sum(s => s.TotalScore);
            double averageAccuracy = scores.Count > 0 ? scores.Average(s => s.Accuracy) : 0;
            int bestCombo = scores.Count > 0 ? scores.Max(s => s.MaxCombo) : 0;
            double totalPerformance = scores.Sum(s => s.PP ?? 0);
            int passed = scores.Count(s => s.Passed);

            var rows = new List<Drawable>
            {
                revealRow(new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = MarathonStrings.SummaryHeader,
                    Font = OsuFont.TorusAlternate.With(size: 20, weight: FontWeight.SemiBold),
                    Colour = colourProvider.Content2,
                }, 26),
                revealRow(new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 44,
                    Masking = true,
                    Child = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = marathon.Name,
                        Font = OsuFont.TorusAlternate.With(size: 32, weight: FontWeight.Bold),
                        Shadow = true,
                    },
                }, 44),
                revealRow(new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = MarathonStrings.MapsCleared(passed, poolSize),
                    Font = OsuFont.GetFont(size: 16),
                    Colour = colourProvider.Content2,
                    Margin = new MarginPadding { Bottom = 12 },
                }, 34),
                revealRow(bigStat(MarathonStrings.TotalScore, totalScore.ToLocalisableString(@"N0")), 62),
                revealRow(new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(28, 0),
                    Children = new[]
                    {
                        smallStat(MarathonStrings.AverageAccuracy, $"{averageAccuracy:0.00%}"),
                        smallStat(MarathonStrings.BestCombo, $"{bestCombo}x"),
                        smallStat(MarathonStrings.TotalPerformance, totalPerformance > 0 ? $"{totalPerformance:0}" : "-"),
                    }
                }, 50),
            };

            var grades = createGradeSummary();

            if (grades != null)
                rows.Add(revealRow(grades, 48));

            return new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 4),
                Children = rows.ToArray(),
            };
        }

        private Drawable? createGradeSummary()
        {
            var tallies = scores.GroupBy(s => s.Rank)
                                .OrderByDescending(g => g.Key)
                                .ToArray();

            if (tallies.Length == 0)
                return null;

            return new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(12, 0),
                Children = tallies.Select(group => (Drawable)new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(4, 0),
                    Children = new Drawable[]
                    {
                        new UpdateableRank(group.Key, false)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(44, 22),
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = $"×{group.Count()}",
                            Font = OsuFont.GetFont(size: 15, weight: FontWeight.SemiBold),
                        },
                    }
                }).ToArray(),
            };
        }

        private void buildMapList()
        {
            listFlow.Add(new OsuSpriteText
            {
                Text = MarathonStrings.PlayedMaps,
                Font = OsuFont.TorusAlternate.With(size: 18, weight: FontWeight.SemiBold),
                Colour = colourProvider.Content2,
                Margin = new MarginPadding { Bottom = 4, Left = 4 },
            });

            for (int i = 0; i < scores.Count; i++)
                listFlow.Add(new MarathonSummaryRow(i + 1, scores[i]));
        }

        private Drawable revealRow(Drawable content, float height)
        {
            content.Alpha = 0;
            content.Y = 26;
            overallReveal.Add(content);

            return new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = height,
                Child = content,
            };
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            musicController.AllowTrackControl.Value = false;
            musicController.Stop();

            breakTrack?.VolumeTo(0)
                      .VolumeTo(0.4, 800, Easing.OutQuint);
            breakTrack?.Start();

            double delay = 150;

            foreach (var target in overallReveal)
            {
                target.Delay(delay).FadeIn(400, Easing.OutQuint);
                target.Delay(delay).MoveToY(0, 700, Easing.OutQuint);
                delay += 110;
            }

            Scheduler.AddDelayed(revealList, list_reveal_delay);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            breakTrack?.Stop();
            musicController.AllowTrackControl.Value = true;
            musicController.EnsurePlayingSomething();
            return base.OnExiting(e);
        }

        private void revealList()
        {
            if (listRevealed || !this.IsCurrentScreen())
                return;

            listRevealed = true;

            // finish any pending reveal of the overall stats so nothing is left mid-animation.
            foreach (var target in overallReveal)
            {
                target.FinishTransforms();
                target.Alpha = 1;
                target.Y = 0;
            }

            overallContainer.MoveToX(-overall_shift, 800, Easing.OutQuint);

            listContainer.FadeIn(400, Easing.OutQuint);

            double delay = 250;

            foreach (var row in listFlow.Children.OfType<MarathonSummaryRow>())
            {
                row.Reveal(delay);
                delay += 80;
            }

            showMapsButton.FadeOut(200, Easing.OutQuint);
            finishButton.FadeIn(200, Easing.OutQuint);
        }

        private Drawable bigStat(LocalisableString label, LocalisableString value) => new FillFlowContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 2),
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
                    Font = OsuFont.TorusAlternate.With(size: 36, weight: FontWeight.Bold),
                },
            }
        };

        private Drawable smallStat(LocalisableString label, LocalisableString value) => new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 2),
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = label,
                    Font = OsuFont.GetFont(size: 11, weight: FontWeight.SemiBold),
                    Colour = Colour4.White.Opacity(0.6f),
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = value,
                    Font = OsuFont.TorusAlternate.With(size: 22, weight: FontWeight.SemiBold),
                },
            }
        };

        /// <summary>
        /// A single played map in the breakdown list.
        /// </summary>
        private partial class MarathonSummaryRow : CompositeDrawable
        {
            private readonly int position;
            private readonly ScoreInfo score;

            /// <summary>
            /// Everything visible lives in here so it can slide in without the surrounding flow
            /// overwriting the row's own position each frame.
            /// </summary>
            private Container content = null!;

            public MarathonSummaryRow(int position, ScoreInfo score)
            {
                this.position = position;
                this.score = score;

                RelativeSizeAxes = Axes.X;
                Height = 46;
            }

            public void Reveal(double delay)
            {
                content.Delay(delay).FadeIn(350, Easing.OutQuint);
                content.Delay(delay).MoveToX(0, 600, Easing.OutQuint);
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider, OsuColour colours)
            {
                string title = score.BeatmapInfo?.Metadata.Title ?? score.BeatmapInfo?.GetDisplayTitle() ?? "Unknown map";
                string difficulty = score.BeatmapInfo?.DifficultyName ?? string.Empty;
                string mods = string.Join(' ', score.Mods.Select(m => m.Acronym));

                InternalChild = content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 6,
                    Alpha = 0,
                    X = 60,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background4,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = 4,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = score.Passed ? colours.Lime1 : colours.Red1,
                            },
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = 14, Right = 12, Vertical = 5 },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.Absolute, 26),
                                new Dimension(GridSizeMode.Absolute, 48),
                                new Dimension(),
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Text = position.ToLocalisableString(@"0"),
                                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                                        Colour = colourProvider.Content2,
                                    },
                                    new UpdateableRank(score.Rank, false)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(40, 20),
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Direction = FillDirection.Vertical,
                                        Padding = new MarginPadding { Right = 10 },
                                        Children = new Drawable[]
                                        {
                                            new TruncatingSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Text = title,
                                                Font = OsuFont.GetFont(size: 15, weight: FontWeight.SemiBold),
                                            },
                                            new TruncatingSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Text = string.IsNullOrEmpty(mods) ? difficulty : $"{difficulty} +{mods}",
                                                Font = OsuFont.GetFont(size: 12),
                                                Colour = colourProvider.Content2,
                                            },
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(14, 0),
                                        Children = new Drawable[]
                                        {
                                            column($"{score.Accuracy:0.00%}", $"{score.MaxCombo}x", colourProvider),
                                            column(score.TotalScore.ToLocalisableString(@"N0"),
                                                score.Passed ? MarathonStrings.MapPassed : MarathonStrings.MapFailed,
                                                colourProvider,
                                                score.Passed ? colours.Lime1 : colours.Red1),
                                        }
                                    },
                                }
                            }
                        },
                    }
                };
            }

            private static Drawable column(LocalisableString top, LocalisableString bottom, OverlayColourProvider colourProvider, Colour4? bottomColour = null) => new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Text = top,
                        Font = OsuFont.TorusAlternate.With(size: 15, weight: FontWeight.SemiBold),
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Text = bottom,
                        Font = OsuFont.GetFont(size: 11, weight: FontWeight.SemiBold),
                        Colour = bottomColour ?? colourProvider.Content2,
                    },
                }
            };
        }
    }
}
