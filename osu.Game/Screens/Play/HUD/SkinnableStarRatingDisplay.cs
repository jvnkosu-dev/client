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
    public partial class SkinnableStarRatingDisplay : CompositeDrawable, ISerialisableDrawable
    {
        private StarRatingDisplay starRatingDisplay { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private CancellationTokenSource? difficultyCancellationSource;
        private IBindable<StarDifficulty>? difficultyBindable;

        public SkinnableStarRatingDisplay() // WARNING: this is awful
        {
            AutoSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                starRatingDisplay = new StarRatingDisplay(new StarDifficulty(0.00, 0), animated: true)
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            beatmap.BindValueChanged(b =>
            {
                difficultyCancellationSource?.Cancel();
                difficultyCancellationSource = new CancellationTokenSource();

                difficultyBindable?.UnbindAll();
                difficultyBindable = difficultyCache.GetBindableDifficulty(b.NewValue.BeatmapInfo, difficultyCancellationSource.Token);
            }, true);
            starRatingDisplay.Current.BindTo((Bindable<StarDifficulty>)difficultyBindable!);
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
