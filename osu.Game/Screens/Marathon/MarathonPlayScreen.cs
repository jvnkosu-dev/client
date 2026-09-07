// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Marathon
{
    /// <summary>
    /// Downloads a marathon map pool, then plays the maps in order.
    /// Each map submits like a normal solo play; a short break after results
    /// is used so stats can settle before the next map starts.
    /// </summary>
    public partial class MarathonPlayScreen : OsuScreen
    {
        public override string Title => Marathon.Name;

        public override bool ShowFooter => true;

        public readonly APIMarathon Marathon;

        /// <summary>
        /// Zero-based index of the map currently being played (or about to be played).
        /// </summary>
        public int CurrentIndex { get; private set; }

        public int TotalMaps => playableMaps.Count;

        /// <summary>
        /// Results recorded so far, in play order.
        /// </summary>
        public IReadOnlyList<ScoreInfo> Results => results.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToArray();

        /// <summary>
        /// Set by <see cref="MarathonBreakScreen"/> when the player should proceed
        /// to the next map (or leave after the last one). Backing out of results leaves this false.
        /// </summary>
        public bool ContinueRequested { get; set; }

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        private readonly List<APIMarathonMap> playableMaps = [];
        private readonly Dictionary<int, ScoreInfo> results = [];

        private OnlinePlayScreenWaveContainer waves = null!;
        private OsuSpriteText statusText = null!;
        private OsuSpriteText progressText = null!;
        private LoadingLayer loadingLayer = null!;

        private CancellationTokenSource? prepareCancellation;
        private bool reachedResults;
        private bool preferNoVideo;
        private bool summaryShown;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBlack(WaveContainer.APPEAR_DURATION);

        public MarathonPlayScreen(APIMarathon marathon)
        {
            Marathon = marathon;
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = waves = new OnlinePlayScreenWaveContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5,
                    },
                    new Header(MarathonStrings.HeaderTitle, null),
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Horizontal = WaveOverlayContainer.WIDTH_PADDING,
                            Top = Header.HEIGHT,
                            Bottom = ScreenFooter.HEIGHT + 20,
                        },
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 10,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background3,
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new osuTK.Vector2(8),
                                    Children = new Drawable[]
                                    {
                                        statusText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Text = MarathonStrings.DownloadingPool,
                                            Font = OsuFont.GetFont(size: 24, weight: FontWeight.SemiBold),
                                        },
                                        progressText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Font = OsuFont.GetFont(size: 16),
                                            Colour = colourProvider.Content2,
                                        },
                                    }
                                },
                                loadingLayer = new LoadingLayer(true),
                            }
                        }
                    }
                }
            };
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            this.FadeIn();
            waves.Show();
            beginPrepare();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            this.FadeIn(250);
            this.ScaleTo(1, 250, Easing.OutSine);

            Schedule(handleReturnFromGameplay);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);
            this.ScaleTo(1.1f, 250, Easing.InSine);
            this.FadeOut(250);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            prepareCancellation?.Cancel();
            waves.Hide();
            this.Delay(WaveContainer.DISAPPEAR_DURATION).FadeOut();
            return base.OnExiting(e);
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);
            logo.Delay(WaveContainer.DISAPPEAR_DURATION / 2).FadeOut();
        }

        /// <summary>
        /// Stores the result of a finished map so it can be shown in the run summary.
        /// Keyed by map index so retries replace the previous attempt instead of stacking up.
        /// </summary>
        public void RecordResult(ScoreInfo score) => results[CurrentIndex] = score;

        /// <summary>
        /// Creates the screen to show after a map finishes. The last map goes straight to the
        /// run summary instead of a break, so the overall stats appear as gameplay ends.
        /// </summary>
        public ResultsScreen CreateResultsFor(ScoreInfo score)
        {
            if (CurrentIndex < playableMaps.Count - 1)
                return new MarathonBreakScreen(score, this);

            summaryShown = true;
            return new MarathonSummaryScreen(Marathon, Results, TotalMaps);
        }

        private void handleReturnFromGameplay()
        {
            if (!this.IsCurrentScreen())
                return;

            if (summaryShown)
            {
                this.Exit();
                return;
            }

            if (!reachedResults || !ContinueRequested)
            {
                // failing ends the run, and that's still worth summarising. deliberately
                // backing out of a break (or quitting mid-map) just leaves.
                if (!reachedResults && results.TryGetValue(CurrentIndex, out var last) && !last.Passed)
                    finishRun();
                else
                    this.Exit();

                return;
            }

            CurrentIndex++;
            startCurrentMap();
        }

        private void finishRun()
        {
            if (results.Count == 0)
            {
                this.Exit();
                return;
            }

            summaryShown = true;
            this.Push(new MarathonSummaryScreen(Marathon, Results, TotalMaps));
        }

        private void beginPrepare()
        {
            loadingLayer.Show();
            prepareCancellation?.Cancel();
            prepareCancellation = new CancellationTokenSource();
            preferNoVideo = config.Get<bool>(OsuSetting.PreferNoVideo);

            Task.Run(() => preparePool(prepareCancellation.Token))
                .ContinueWith(task => Schedule(() =>
                {
                    loadingLayer.Hide();

                    if (task.IsCanceled || !this.IsCurrentScreen())
                        return;

                    if (task.IsFaulted)
                    {
                        Logger.Error(task.Exception, "Failed to prepare marathon map pool.");
                        notifications?.Post(new SimpleNotification { Text = MarathonStrings.DownloadNone });
                        this.Exit();
                        return;
                    }

                    if (playableMaps.Count == 0)
                    {
                        notifications?.Post(new SimpleNotification { Text = MarathonStrings.DownloadNone });
                        this.Exit();
                        return;
                    }

                    if (playableMaps.Count < Marathon.Maps.Length)
                        notifications?.Post(new SimpleNotification { Text = MarathonStrings.DownloadPartial });

                    CurrentIndex = 0;
                    startCurrentMap();
                }));
        }

        private async Task preparePool(CancellationToken cancellation)
        {
            var orderedMaps = Marathon.Maps.OrderBy(m => m.PlaylistOrder).ToArray();
            var ready = new HashSet<int>();

            void updateProgress()
            {
                int count = orderedMaps.Count(m => findLocal(m) != null);
                Schedule(() =>
                {
                    progressText.Text = MarathonStrings.DownloadProgress(count, orderedMaps.Length);
                });
            }

            updateProgress();

            foreach (var map in orderedMaps)
            {
                cancellation.ThrowIfCancellationRequested();

                if (findLocal(map) != null)
                    continue;

                if (map.BeatmapId <= 0)
                    continue;

                APIBeatmap? apiBeatmap;

                try
                {
                    apiBeatmap = await beatmapLookupCache.GetBeatmapAsync(map.BeatmapId, cancellation).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Log($"Marathon beatmap lookup failed ({map.BeatmapId}): {ex.Message}");
                    continue;
                }

                if (apiBeatmap == null)
                    continue;

                IBeatmapSetInfo? set = apiBeatmap.BeatmapSet;

                if (set == null && apiBeatmap.OnlineBeatmapSetID > 0)
                    set = new APIBeatmapSet { OnlineID = apiBeatmap.OnlineBeatmapSetID };

                if (set == null || set.OnlineID <= 0)
                    continue;

                if (!ready.Add(set.OnlineID))
                    continue;

                var setToDownload = set;
                Schedule(() => beatmapDownloader.Download(setToDownload, preferNoVideo));
            }

            var startedAt = DateTimeOffset.Now;

            while (!cancellation.IsCancellationRequested)
            {
                updateProgress();

                if (orderedMaps.All(m => findLocal(m) != null || m.BeatmapId <= 0))
                    break;

                if (DateTimeOffset.Now - startedAt > TimeSpan.FromMinutes(2))
                    break;

                await Task.Delay(250, cancellation).ConfigureAwait(false);
            }

            cancellation.ThrowIfCancellationRequested();

            foreach (var map in orderedMaps)
            {
                if (findLocal(map) != null)
                    playableMaps.Add(map);
            }
        }

        private void startCurrentMap()
        {
            if (!this.IsCurrentScreen())
                return;

            while (CurrentIndex < playableMaps.Count && !applyMap(playableMaps[CurrentIndex]))
                CurrentIndex++;

            if (CurrentIndex >= playableMaps.Count)
            {
                finishRun();
                return;
            }

            reachedResults = false;
            ContinueRequested = false;

            this.Push(new PlayerLoader(() =>
            {
                var player = new MarathonPlayer(this);
                player.OnShowingResults += () => reachedResults = true;
                return player;
            }));
        }

        private bool applyMap(APIMarathonMap map)
        {
            var local = findLocal(map);
            if (local == null)
                return false;

            var rulesetInfo = rulesets.GetRuleset(map.RulesetId) ?? local.Ruleset;
            if (rulesetInfo == null)
                return false;

            Ruleset rulesetInstance = rulesetInfo.CreateInstance();

            Mod[] mods;

            try
            {
                mods = map.RequiredMods.Select(m => m.ToMod(rulesetInstance)).ToArray();
            }
            catch (Exception ex)
            {
                Logger.Log($"Marathon mods could not be applied: {ex.Message}");
                mods = [];
            }

            Beatmap.Value = beatmaps.GetWorkingBeatmap(local);
            Ruleset.Value = rulesetInfo;
            Mods.Value = mods;
            return true;
        }

        private BeatmapInfo? findLocal(APIMarathonMap map)
        {
            if (!string.IsNullOrEmpty(map.Checksum))
            {
                var byChecksum = beatmaps.QueryBeatmap(b => b.MD5Hash == map.Checksum);
                if (byChecksum != null)
                    return byChecksum;
            }

            if (map.BeatmapId > 0)
                return beatmaps.QueryOnlineBeatmapId(map.BeatmapId);

            return null;
        }
    }
}
