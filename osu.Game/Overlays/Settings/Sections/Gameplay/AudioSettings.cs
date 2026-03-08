// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public partial class AudioSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GameplaySettingsStrings.AudioHeader;

        private FormCheckBox alwaysPlayFirst = null!;
        private FormCheckBox alwaysPlay = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, OsuConfigManager osuConfig)
        {
            alwaysPlayFirst = new FormCheckBox
            {
                Caption = GameplaySettingsStrings.AlwaysPlayFirstComboBreak,
                Current = config.GetBindable<bool>(OsuSetting.AlwaysPlayFirstComboBreak)
            };

            alwaysPlay = new FormCheckBox
            {
                Caption = "Always play combo break sound",
                Current = config.GetBindable<bool>(OsuSetting.AlwaysPlayComboBreak)
            };

            Children = new Drawable[]
            {
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = AudioSettingsStrings.PositionalLevel,
                    Current = osuConfig.GetBindable<float>(OsuSetting.PositionalHitsoundsLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                })
                {
                    Keywords = new[] { @"positional", @"balance" },
                },

                new SettingsItemV2(alwaysPlayFirst),
                new SettingsItemV2(alwaysPlay)
            };
            alwaysPlay.Current.BindValueChanged(
                d =>
                {
                    alwaysPlayFirst.Current.Disabled = d.NewValue;
                },
                true
            );
        }
    }
}
