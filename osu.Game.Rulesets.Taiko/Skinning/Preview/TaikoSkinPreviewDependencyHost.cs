// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Skinning.Preview;

namespace osu.Game.Rulesets.Taiko.Skinning.Preview
{
    /// <summary>
    /// Provides gameplay dependencies required by taiko skin preview drawables.
    /// </summary>
    public partial class TaikoSkinPreviewDependencyHost : Container
    {
        private readonly SkinPreviewBeatSyncProvider beatSyncProvider;
        private readonly DrawableHitObject hitObject;

        public TaikoSkinPreviewDependencyHost(Drawable child)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            var hit = new Hit
            {
                StartTime = 0,
                Type = HitType.Centre,
            };

            hit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            hitObject = new DrawableHit(hit) { Alpha = 0 };
            beatSyncProvider = new SkinPreviewBeatSyncProvider(new StopwatchClock(false));

            Child = child;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs<DrawableHitObject>(hitObject);
            dependencies.CacheAs(beatSyncProvider);
            dependencies.CacheAs<IBeatSyncProvider>(beatSyncProvider);
            return dependencies;
        }
    }
}
