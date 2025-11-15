// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonLongestComboCounter : ComboCounter
    {
        protected ArgonCounterTextComponent Text = null!;

        protected override double RollingDuration => 250;

        protected virtual bool DisplayXSymbol => true;

        [SettingSource("Wireframe opacity", "Controls the opacity of the wireframes behind the digits.")]
        public BindableFloat WireframeOpacity { get; } = new BindableFloat(0.25f)
        {
            Precision = 0.01f,
            MinValue = 0,
            MaxValue = 1,
        };

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.ShowLabel))]
        public Bindable<bool> ShowLabel { get; } = new BindableBool(true);

        [SettingSource("Show animation on increase", "Shows a bouncing animation when the combo increases")]
        public Bindable<bool> ShowRolling { get; } = new BindableBool(true);

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            Current.BindTo(scoreProcessor.HighestCombo);
            Current.BindValueChanged(combo =>
            {
                bool wasIncrease = combo.NewValue > combo.OldValue;
                bool wasMiss = combo.OldValue > 1 && combo.NewValue == 0;

                float newScale = Math.Clamp(Text.NumberContainer.Scale.X * (wasIncrease ? 1.1f : 0.8f), 0.6f, 1.4f);

                float duration = ShowRolling.Value ? 500 : 0;

                Text.NumberContainer
                    .ScaleTo(new Vector2(newScale))
                    .ScaleTo(Vector2.One, duration, Easing.OutQuint);
            });
        }

        public override int DisplayedCount
        {
            get => base.DisplayedCount;
            set
            {
                base.DisplayedCount = value;
                updateWireframe();
            }
        }

        private void updateWireframe()
        {
            int digitsRequiredForDisplayCount = getDigitsRequiredForDisplayCount();

            if (digitsRequiredForDisplayCount != Text.WireframeTemplate.Length)
                Text.WireframeTemplate = new string('#', digitsRequiredForDisplayCount);
        }

        private int getDigitsRequiredForDisplayCount()
        {
            // one for the single presumed starting digit, one for the "x" at the end (unless disabled).
            int digitsRequired = DisplayXSymbol ? 2 : 1;
            long c = DisplayedCount;
            while ((c /= 10) > 0)
                digitsRequired++;
            return digitsRequired;
        }

        protected override LocalisableString FormatCount(int count) => DisplayXSymbol ? $@"{count}x" : count.ToString();

        protected override IHasText CreateText() => Text = new ArgonCounterTextComponent(Anchor.TopLeft, "MAX COMBO")
        {
            WireframeOpacity = { BindTarget = WireframeOpacity },
            ShowLabel = { BindTarget = ShowLabel },
        };
    }
}
