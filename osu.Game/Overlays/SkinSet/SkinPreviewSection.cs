// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osu.Game.Skinning.Preview;
using osuTK;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinPreviewSection : Container
    {
        private const double preview_load_delay = 250;

        public readonly Bindable<APIOnlineSkin?> Skin = new Bindable<APIOnlineSkin?>();

        private readonly FillFlowContainer sectionFlow;
        private readonly FillFlowContainer headerRow;
        private readonly SkinPreviewGalleryHost galleryHost;
        private readonly SkinPreviewRulesetDropdown rulesetDropdown;

        private int? lastRequestId;
        private int? activePreviewSessionId;
        private ScheduledDelegate? deferredPreviewLoad;

        public event System.Action? LayoutInvalidated;

        public SkinPreviewSection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Child = sectionFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    headerRow = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(6, 0),
                        Margin = new MarginPadding { Top = -4, Bottom = 5 },
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "Preview for",
                                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                            rulesetDropdown = new SkinPreviewRulesetDropdown
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                        },
                    },
                    galleryHost = new SkinPreviewGalleryHost(),
                },
            };
        }

        [Resolved]
        private OnlineSkinPreviewProvider previewProvider { get; set; } = null!;

        [Resolved]
        private SkinDownloader skinDownloader { get; set; } = null!;

        [Resolved]
        private SkinManager skinManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rulesetDropdown.Current.BindTarget = galleryHost.PreviewRuleset;
            rulesetDropdown.MenuStateChanged += state =>
            {
                sectionFlow.ChangeChildDepth(headerRow, state == MenuState.Open ? float.MinValue : 0);
            };
            galleryHost.LayoutInvalidated += () => LayoutInvalidated?.Invoke();
            Skin.BindValueChanged(_ => updatePreview(), true);
        }

        private void updatePreview()
        {
            deferredPreviewLoad?.Cancel();
            lastRequestId = null;
            endPreviewSession();

            var onlineSkin = Skin.Value;

            if (onlineSkin == null)
            {
                galleryHost.HidePreview();
                return;
            }

            int requestId = onlineSkin.OnlineID;
            lastRequestId = requestId;

            galleryHost.ShowLoading();
            updatePreviewRulesets(onlineSkin);

            deferredPreviewLoad = Scheduler.AddDelayed(() =>
            {
                if (lastRequestId != requestId)
                    return;

                tryShowInstalledSkin(onlineSkin, requestId);
            }, preview_load_delay);
        }

        private void updatePreviewRulesets(APIOnlineSkin onlineSkin)
        {
            var matchingRulesets = SkinModifiedModesHelper.GetMatchingRulesetsInOrder(onlineSkin.ModifiedModes, rulesets).ToArray();

            if (matchingRulesets.Length == 0 && rulesets.GetRuleset(0) is RulesetInfo osuRuleset)
                matchingRulesets = new[] { osuRuleset };

            rulesetDropdown.Items = matchingRulesets;

            var current = galleryHost.PreviewRuleset.Value;

            if (matchingRulesets.Length > 0 && (current == null || matchingRulesets.All(r => !r.Equals(current))))
                galleryHost.PreviewRuleset.Value = matchingRulesets[0];
        }

        private void tryShowInstalledSkin(APIOnlineSkin onlineSkin, int requestId)
        {
            var installedSkin = skinDownloader.GetInstalledSkin(onlineSkin);

            if (installedSkin != null)
            {
                previewProvider.Invalidate(onlineSkin.OnlineID);
                Schedule(() =>
                {
                    if (lastRequestId != requestId)
                        return;

                    galleryHost.ShowSkin(installedSkin.PerformRead(skinManager.GetSkin));
                });
                return;
            }

            previewProvider.NotifyPreviewSessionStarted(onlineSkin.OnlineID);
            activePreviewSessionId = onlineSkin.OnlineID;

            previewProvider.GetPreview(onlineSkin, handle =>
            {
                Schedule(() =>
                {
                    if (lastRequestId != requestId)
                        return;

                    galleryHost.ShowSkin(handle);
                });
            }, () =>
            {
                Schedule(() =>
                {
                    if (lastRequestId != requestId)
                        return;

                    endPreviewSession();
                    galleryHost.ShowFailure("Failed to load skin preview");
                });
            });
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
            endPreviewSession();
            deferredPreviewLoad?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
