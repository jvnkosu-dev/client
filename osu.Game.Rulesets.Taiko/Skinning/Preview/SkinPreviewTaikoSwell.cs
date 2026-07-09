// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Preview
{
    public partial class SkinPreviewTaikoSwell : Container
    {
        public SkinPreviewTaikoSwell()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var swell = new Swell
            {
                StartTime = 0,
                EndTime = 3000,
            };

            swell.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Child = new TaikoSwellPreviewHost(new DrawableSwell(swell), new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.Swell), _ => new DefaultSwell())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Scale = new Vector2(0.55f),
            });
        }

        private partial class TaikoSwellPreviewHost : Container
        {
            public TaikoSwellPreviewHost(DrawableHitObject hitObject, Drawable child)
            {
                RelativeSizeAxes = Axes.Both;
                Child = child;
                this.hitObject = hitObject;
            }

            private readonly DrawableHitObject hitObject;

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
                dependencies.CacheAs(hitObject);
                return dependencies;
            }
        }
    }
}
