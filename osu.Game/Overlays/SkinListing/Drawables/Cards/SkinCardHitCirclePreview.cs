// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osu.Game.Skinning.Preview;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    /// <summary>
    /// Loads and displays a compact hit-circle skin preview on listing card hover,
    /// analogous to the beatmap card play button.
    /// </summary>
    public partial class SkinCardHitCirclePreview : Container
    {
        private const double preview_load_delay = 250;

        private readonly APIOnlineSkin onlineSkin;

        private readonly Container previewContent;
        private readonly LoadingSpinner loadingSpinner;

        private int? lastRequestId;
        private int? activePreviewSessionId;
        private ScheduledDelegate? deferredPreviewLoad;
        private bool dimmed;

        [Resolved]
        private OnlineSkinPreviewProvider previewProvider { get; set; } = null!;

        [Resolved]
        private SkinDownloader skinDownloader { get; set; } = null!;

        [Resolved]
        private SkinManager skinManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        public SkinCardHitCirclePreview(APIOnlineSkin onlineSkin)
        {
            this.onlineSkin = onlineSkin;

            RelativeSizeAxes = Axes.Both;
            Alpha = 0;

            Children = new Drawable[]
            {
                previewContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                loadingSpinner = new LoadingSpinner
                {
                    Size = new Vector2(18),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }

        public void SetDimmed(bool value)
        {
            if (dimmed == value)
                return;

            dimmed = value;

            if (dimmed)
                beginPreview();
            else
                hidePreview();
        }

        private void beginPreview()
        {
            this.FadeTo(1, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

            deferredPreviewLoad?.Cancel();
            lastRequestId = onlineSkin.OnlineID;

            if (previewContent.Count > 0)
            {
                loadingSpinner.Hide();
                return;
            }

            loadingSpinner.Show();

            int requestId = onlineSkin.OnlineID;

            deferredPreviewLoad = Scheduler.AddDelayed(() =>
            {
                if (lastRequestId != requestId || !dimmed)
                    return;

                tryShowPreview(requestId);
            }, preview_load_delay);
        }

        private void hidePreview()
        {
            deferredPreviewLoad?.Cancel();
            lastRequestId = null;
            endPreviewSession();

            previewContent.Clear();
            this.FadeTo(0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            loadingSpinner.Hide();
        }

        private void tryShowPreview(int requestId)
        {
            var installedSkin = skinDownloader.GetInstalledSkin(onlineSkin);

            if (installedSkin != null)
            {
                previewProvider.Invalidate(onlineSkin.OnlineID);
                Schedule(() =>
                {
                    if (lastRequestId != requestId || !dimmed)
                        return;

                    showSkin(installedSkin.PerformRead(skinManager.GetSkin));
                });
                return;
            }

            previewProvider.NotifyPreviewSessionStarted(onlineSkin.OnlineID);
            activePreviewSessionId = onlineSkin.OnlineID;

            previewProvider.GetPreview(onlineSkin, handle =>
            {
                Schedule(() =>
                {
                    if (lastRequestId != requestId || !dimmed)
                        return;

                    showSkin(handle.Skin);
                });
            }, () =>
            {
                Schedule(() =>
                {
                    if (lastRequestId != requestId)
                        return;

                    endPreviewSession();
                    loadingSpinner.Hide();
                    this.FadeTo(0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
                });
            });
        }

        private void showSkin(ISkin skin)
        {
            var osuRulesetInfo = rulesets.GetRuleset(0);
            if (osuRulesetInfo == null)
            {
                loadingSpinner.Hide();
                return;
            }

            var preview = osuRulesetInfo.CreateInstance().CreateSkinCardHoverPreview(skin);

            previewContent.Clear();

            if (preview != null)
            {
                preview.RelativeSizeAxes = Axes.Both;
                previewContent.Child = preview;
            }

            loadingSpinner.Hide();
        }

        private void endPreviewSession()
        {
            if (activePreviewSessionId == null)
                return;

            previewProvider.NotifyPreviewSessionEnded(activePreviewSessionId.Value);
            activePreviewSessionId = null;
        }

        protected override void Dispose(bool isDisposing)
        {
            deferredPreviewLoad?.Cancel();
            endPreviewSession();
            base.Dispose(isDisposing);
        }
    }
}
