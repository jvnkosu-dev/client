// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Mods.Input;

namespace osu.Game.Overlays.Settings.Sections.UserInterface
{
    public partial class SongSelectSettings : SettingsSubsection
    {
        protected override LocalisableString Header => UserInterfaceStrings.SongSelectHeader;

        private readonly Bindable<SettingsNote.Data?> selectV1Gone = new Bindable<SettingsNote.Data?>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            selectV1Gone.Value = new SettingsNote.Data("SelectV1 has been fully removed from upstream code base.\nAt this time, we cannot bring it back.", SettingsNote.Type.Warning);
            Bindable<bool> selectV1 = config.GetBindable<bool>(OsuSetting.ForceLegacySongSelect);
            selectV1.Disabled = true;

            Children = new Drawable[]
            {
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = UserInterfaceStrings.ForceLegacySongSelect,
                    Current = selectV1,
                })
                {
                    Note = { BindTarget = selectV1Gone }
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = UserInterfaceStrings.ShowConvertedBeatmaps,
                    Current = config.GetBindable<bool>(OsuSetting.ShowConvertedBeatmaps),
                })
                {
                    Keywords = new[] { "converts", "converted" }
                },
                new SettingsItemV2(new FormEnumDropdown<RandomSelectAlgorithm>
                {
                    Caption = UserInterfaceStrings.RandomSelectionAlgorithm,
                    Current = config.GetBindable<RandomSelectAlgorithm>(OsuSetting.RandomSelectAlgorithm),
                }),
                new SettingsItemV2(new FormEnumDropdown<ModSelectHotkeyStyle>
                {
                    Caption = UserInterfaceStrings.ModSelectHotkeyStyle,
                    Current = config.GetBindable<ModSelectHotkeyStyle>(OsuSetting.ModSelectHotkeyStyle),
                })
                {
                    ApplyClassicDefault = c => ((IHasCurrentValue<ModSelectHotkeyStyle>)c).Current.Value = ModSelectHotkeyStyle.Classic,
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = UserInterfaceStrings.ModSelectTextSearchStartsActive,
                    Current = config.GetBindable<bool>(OsuSetting.ModSelectTextSearchStartsActive),
                })
                {
                    ApplyClassicDefault = c => ((IHasCurrentValue<bool>)c).Current.Value = false,
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = GameplaySettingsStrings.BackgroundBlur,
                    Current = config.GetBindable<bool>(OsuSetting.SongSelectBackgroundBlur),
                }),
            };
        }
    }
}
