// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Settings.Sections.UserInterface
{
    public partial class MainMenuSettings : SettingsSubsection
    {
        protected override LocalisableString Header => UserInterfaceStrings.MainMenuHeader;

        // TODO: refactor seasonal bg code to the way it was before options were introduced
        private SeasonalBackgroundLoader? backgroundLoader = null!;

        private IBindable<APIUser> user = null!;

        // private SettingsEnumDropdown<BackgroundSource> backgroundSourceDropdown = null!;

        private Bindable<bool> useSeasonalBackgrounds = null!;

        private readonly Bindable<SettingsNote.Data?> backgroundSourceNote = new Bindable<SettingsNote.Data?>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api, SeasonalBackgroundLoader? backgroundLoader)
        {
            user = api.LocalUser.GetBoundCopy();

            this.backgroundLoader = backgroundLoader;

            useSeasonalBackgrounds = config.GetBindable<bool>(OsuSetting.UseSeasonalBackgroundsV2);

            var backgroundToggle = new FormCheckBox
            {
                Caption = UserInterfaceStrings.UseSeasonalBackgrounds,
                Current = config.GetBindable<bool>(OsuSetting.UseSeasonalBackgroundsV2),
            };

            var categoryDropdown = new FormDropdown<string>
            {
                Caption = UserInterfaceStrings.SeasonalBackgroundsCategories,
                Current = config.GetBindable<string>(OsuSetting.BackgroundCategory)
            };

            var refreshButton = new SettingsButtonV2
            {
                Text = UserInterfaceStrings.SeasonalBackgroundsRefresh,
                Action = () => backgroundLoader?.RefreshCategories()
            };

            // TODO: the category dropdown disappear if no backgrounds (e.g. when first enabling the setting)
            refreshButton.CanBeShown.BindTo(useSeasonalBackgrounds);

            useSeasonalBackgrounds.BindValueChanged(
                d =>
                {
                    backgroundLoader?.RefreshCategories(true);
                    categoryDropdown.Current.Disabled = !d.NewValue;
                },
                true
            );

            backgroundLoader?.AvailableCategories.BindValueChanged(categories => categoryDropdown.Items = categories.NewValue, true);

            Children = new Drawable[]
            {
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = UserInterfaceStrings.ShowMenuTips,
                    Current = config.GetBindable<bool>(OsuSetting.MenuTips)
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = UserInterfaceStrings.InterfaceVoices,
                    Current = config.GetBindable<bool>(OsuSetting.MenuVoice)
                })
                {
                    Keywords = new[] { "intro", "welcome" },
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = UserInterfaceStrings.OsuMusicTheme,
                    Current = config.GetBindable<bool>(OsuSetting.MenuMusic)
                })
                {
                    Keywords = new[] { "intro", "welcome" },
                },
                new SettingsItemV2(new FormEnumDropdown<IntroSequence>
                {
                    Caption = UserInterfaceStrings.IntroSequence,
                    Current = config.GetBindable<IntroSequence>(OsuSetting.IntroSequence),
                }),
                new SettingsItemV2(new FormEnumDropdown<BackgroundSource>
                {
                    Caption = UserInterfaceStrings.BackgroundSource,
                    Current = config.GetBindable<BackgroundSource>(OsuSetting.MenuBackgroundSource),
                }),
                new SettingsItemV2(backgroundToggle),
                new SettingsItemV2(categoryDropdown),
                refreshButton,
                new SettingsColour
                {
                    LabelText = UserInterfaceStrings.LogoColour,
                    Current = config.GetBindable<Colour4>(OsuSetting.MenuCookieColor),
                    ClassicDefault = Colour4.FromHex(@"ff66ba"),
                },
                new SettingsItemV2(new FormEnumDropdown<SeasonalBackgroundMode>
                {
                    Caption = UserInterfaceStrings.SeasonalBackgrounds,
                    Current = config.GetBindable<SeasonalBackgroundMode>(OsuSetting.SeasonalBackgroundMode),
                })
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            user.BindValueChanged(u =>
            {
                if (u.NewValue?.IsSupporter != true)
                    backgroundSourceNote.Value = new SettingsNote.Data(UserInterfaceStrings.NotSupporterNote, SettingsNote.Type.Informational);
                else
                    backgroundSourceNote.Value = null;
            }, true);
        }
    }
}
