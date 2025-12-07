// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public partial class AudioSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GameplaySettingsStrings.AudioHeader;

        private SettingsCheckbox alwaysPlayFirst = null!;
        private SettingsCheckbox alwaysPlay = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, OsuConfigManager osuConfig)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<float>
                {
                    LabelText = AudioSettingsStrings.PositionalLevel,
                    Keywords = new[] { @"positional", @"balance" },
                    Current = osuConfig.GetBindable<float>(OsuSetting.PositionalHitsoundsLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                alwaysPlayFirst = new SettingsCheckbox
                {
                    ClassicDefault = false,
                    LabelText = GameplaySettingsStrings.AlwaysPlayFirstComboBreak,
                    Current = config.GetBindable<bool>(OsuSetting.AlwaysPlayFirstComboBreak)
                },
                alwaysPlay = new SettingsCheckbox
                {
                    ClassicDefault = false,
                    LabelText = "Always play combo break sound",
                    Current = config.GetBindable<bool>(OsuSetting.AlwaysPlayComboBreak)
                }
            };
            alwaysPlay.Current.BindValueChanged(d =>
            {
                alwaysPlayFirst.Current.Disabled = d.NewValue;
            }, true);
        }
    }
}
