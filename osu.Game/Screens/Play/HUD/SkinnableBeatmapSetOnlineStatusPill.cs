// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public partial class SkinnableBeatmapSetOnlineStatusPill : CompositeDrawable, ISerialisableDrawable
    {
        private BeatmapSetOnlineStatusPill beatmapSetOnlineStatusPill { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        public SkinnableBeatmapSetOnlineStatusPill() // WARNING: this is awful
        {
            AutoSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                beatmapSetOnlineStatusPill = new BeatmapSetOnlineStatusPill()
                {
                    ShowUnknownStatus = true
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            beatmap.BindValueChanged(b =>
            {
                beatmapSetOnlineStatusPill.Status = beatmap.Value.BeatmapInfo.Status;
            }, true);
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
