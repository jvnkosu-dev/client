// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning.Preview;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Preview
{
    /// <summary>
    /// Provides gameplay dependencies required by skin preview drawables.
    /// </summary>
    public partial class SkinPreviewDependencyHost : Container
    {
        private readonly SkinPreviewBeatSyncProvider beatSyncProvider;
        private readonly DrawableHitObject hitObject;

        public SkinPreviewDependencyHost(Drawable child)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            var circle = new HitCircle
            {
                StartTime = 0,
                Position = Vector2.Zero,
                IndexInCurrentCombo = 2,
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 5 });

            hitObject = new DrawableHitCircle(circle) { Alpha = 0 };
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
