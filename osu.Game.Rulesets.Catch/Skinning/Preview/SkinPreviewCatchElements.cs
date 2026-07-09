// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Preview
{
    internal partial class SkinPreviewCatchDependencyHost : Container
    {
        private readonly PalpableCatchHitObject hitObject;
        private readonly bool hyperDash;
        private readonly int indexInBeatmap;

        public SkinPreviewCatchDependencyHost(PalpableCatchHitObject hitObject, Drawable child, bool hyperDash = false, int indexInBeatmap = 0)
        {
            this.hitObject = hitObject;
            this.hyperDash = hyperDash;
            this.indexInBeatmap = indexInBeatmap;
            RelativeSizeAxes = Axes.Both;
            Child = child;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs<IHasCatchObjectState>(new PreviewCatchObjectState(hitObject, hyperDash, indexInBeatmap));
            return dependencies;
        }

        private class PreviewCatchObjectState : IHasCatchObjectState
        {
            public PreviewCatchObjectState(PalpableCatchHitObject hitObject, bool hyperDash, int indexInBeatmap)
            {
                HitObject = hitObject;
                HyperDash.Value = hyperDash;
                IndexInBeatmap.Value = indexInBeatmap;
            }

            public PalpableCatchHitObject HitObject { get; }

            public Bindable<Color4> AccentColour { get; } = new Bindable<Color4>(Color4.White);

            public Bindable<bool> HyperDash { get; } = new Bindable<bool>();

            public Bindable<int> IndexInBeatmap { get; } = new Bindable<int>();

            public double DisplayStartTime => 0;

            public Vector2 DisplayPosition => Vector2.Zero;

            public Vector2 DisplaySize => new Vector2(48);

            public float DisplayRotation => 0;

            public void RestoreState(CatchObjectState state)
            {
            }
        }
    }

    public partial class SkinPreviewCatchCatcher : Container
    {
        private SkinnableCatcher catcher = null!;

        public SkinPreviewCatchCatcher()
        {
            RelativeSizeAxes = Axes.Both;

            Child = catcher = new SkinnableCatcher
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                Height = 60,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            catcher.AnimationState.Value = CatcherAnimationState.Idle;
        }
    }

    public partial class SkinPreviewCatchFruit : Container
    {
        public SkinPreviewCatchFruit()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var fruit = new Fruit { StartTime = 0 };
            fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Child = new SkinPreviewCatchDependencyHost(fruit, new SkinnableDrawable(new CatchSkinComponentLookup(CatchSkinComponents.Fruit), _ => new FruitPiece())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Scale = new Vector2(0.85f),
            }, indexInBeatmap: 2);
        }
    }

    public partial class SkinPreviewCatchDroplet : Container
    {
        public SkinPreviewCatchDroplet()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var droplet = new Droplet { StartTime = 0 };
            droplet.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Child = new SkinPreviewCatchDependencyHost(droplet, new SkinnableDrawable(new CatchSkinComponentLookup(CatchSkinComponents.Droplet), _ => new DropletPiece())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Scale = new Vector2(0.85f),
            }, hyperDash: true);
        }
    }

    public partial class SkinPreviewCatchBanana : Container
    {
        public SkinPreviewCatchBanana()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var banana = new Banana { StartTime = 0 };
            banana.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Child = new SkinPreviewCatchDependencyHost(banana, new SkinnableDrawable(new CatchSkinComponentLookup(CatchSkinComponents.Banana), _ => new BananaPiece())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Scale = new Vector2(0.85f),
            });
        }
    }

    public partial class SkinPreviewCatchFont : Container
    {
        private readonly ISkin skin;
        private SkinnableDrawable comboCounter = null!;

        public SkinPreviewCatchFont(ISkin skin)
        {
            this.skin = skin;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = comboCounter = new SkinnableDrawable(new CatchSkinComponentLookup(CatchSkinComponents.CatchComboCounter), _ => Empty())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
            };

            comboCounter.OnSkinChanged += updateCombo;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateCombo();
        }

        private void updateCombo()
        {
            if (comboCounter.Drawable is ICatchComboCounter counter)
                counter.UpdateCombo(42, Color4.Red);
        }
    }
}
