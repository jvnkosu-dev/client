// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD.ClicksPerSecond
{
    public partial class ArgonClicksPerSecondCounter : RollingCounter<int>, ISerialisableDrawable
    {
        [Resolved]
        private ClicksPerSecondController controller { get; set; } = null!;

        protected override double RollingDuration => 175;

        [SettingSource("Wireframe opacity", "Controls the opacity of the wireframes behind the digits.")]
        public BindableFloat WireframeOpacity { get; } = new BindableFloat(0.25f)
        {
            Precision = 0.01f,
            MinValue = 0,
            MaxValue = 1,
        };

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.ShowLabel))]
        public Bindable<bool> ShowLabel { get; } = new BindableBool(true);

        public bool UsesFixedAnchor { get; set; }

        public ArgonClicksPerSecondCounter()
        {
            Current.Value = 0;
        }

        protected override void Update()
        {
            base.Update();

            Current.Value = controller.Value;
        }

        protected override IHasText CreateText() => new TextComponent()
        {
            WireframeOpacity = { BindTarget = WireframeOpacity },
            ShowLabel = { BindTarget = ShowLabel },
        };

        private partial class TextComponent : CompositeDrawable, IHasText
        {
            private readonly ArgonCounterTextComponent cpsValue;

            public IBindable<float> WireframeOpacity { get; } = new BindableFloat();

            public Bindable<bool> ShowLabel { get; } = new BindableBool();

            public LocalisableString Text
            {
                get => cpsValue.Text;
                set => cpsValue.Text = value;
            }

            public TextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Child = cpsValue = new ArgonCounterTextComponent(Anchor.TopLeft, "KEYS/SEC") // welp, not good
                            {
                                WireframeOpacity = { BindTarget = WireframeOpacity },
                                WireframeTemplate = @"##",
                                ShowLabel = { BindTarget = ShowLabel },
                            }
                        },
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
            }
        }
    }
}
