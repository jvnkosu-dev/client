// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
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
                beatmapSetOnlineStatusPill.Status = beatmap.Value.BeatmapSetInfo.Status;
            }, true);
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
