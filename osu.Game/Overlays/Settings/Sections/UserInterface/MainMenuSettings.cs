// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Settings.Sections.UserInterface
{
    public partial class MainMenuSettings : SettingsSubsection
    {
        protected override LocalisableString Header => UserInterfaceStrings.MainMenuHeader;

        [Resolved]
        private SeasonalBackgroundLoader backgroundLoader { get; set; }

        private IBindable<APIUser> user;

        private SettingsEnumDropdown<BackgroundSource> backgroundSourceDropdown;

        private Bindable<bool> useSeasonalBackgrounds;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api)
        {
            user = api.LocalUser.GetBoundCopy();
            useSeasonalBackgrounds = config.GetBindable<bool>(OsuSetting.UseSeasonalBackgroundsV2);

            var backgroundToggle = new SettingsCheckbox
            {
                LabelText = UserInterfaceStrings.UseSeasonalBackgrounds,
                Current = config.GetBindable<bool>(OsuSetting.UseSeasonalBackgroundsV2),
                ClassicDefault = true
            };

            var categoryDropdown = new SettingsDropdown<string>
            {
                LabelText = UserInterfaceStrings.SeasonalBackgroundsCategories,
                Current = config.GetBindable<string>(OsuSetting.BackgroundCategory)
            };

            var refreshButton = new SettingsButton
            {
                Text = UserInterfaceStrings.SeasonalBackgroundsRefresh,
                Action = () => backgroundLoader.RefreshCategories()
            };

            // TODO: the category dropdown disappear if no backgrounds (e.g. when first enabling the setting)
            refreshButton.CanBeShown.BindTo(useSeasonalBackgrounds);
            categoryDropdown.CanBeShown.BindTo(useSeasonalBackgrounds);
            useSeasonalBackgrounds.BindValueChanged(
                _ => backgroundLoader.RefreshCategories(true)
            );

            backgroundLoader.AvailableCategories.BindValueChanged(categories => categoryDropdown.Items = categories.NewValue, true);

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = UserInterfaceStrings.ShowMenuTips,
                    Current = config.GetBindable<bool>(OsuSetting.MenuTips),
                },
                new SettingsCheckbox
                {
                    Keywords = new[] { "intro", "welcome" },
                    LabelText = UserInterfaceStrings.InterfaceVoices,
                    Current = config.GetBindable<bool>(OsuSetting.MenuVoice)
                },
                new SettingsCheckbox
                {
                    Keywords = new[] { "intro", "welcome" },
                    LabelText = UserInterfaceStrings.OsuMusicTheme,
                    Current = config.GetBindable<bool>(OsuSetting.MenuMusic)
                },
                new SettingsEnumDropdown<IntroSequence>
                {
                    LabelText = UserInterfaceStrings.IntroSequence,
                    Current = config.GetBindable<IntroSequence>(OsuSetting.IntroSequence),
                },
                backgroundSourceDropdown = new SettingsEnumDropdown<BackgroundSource>
                {
                    LabelText = UserInterfaceStrings.BackgroundSource,
                    Current = config.GetBindable<BackgroundSource>(OsuSetting.MenuBackgroundSource),
                },
                backgroundToggle,
                categoryDropdown,
                refreshButton,
                new SettingsColour
                {
                    LabelText = UserInterfaceStrings.LogoColour,
                    Current = config.GetBindable<Colour4>(OsuSetting.MenuCookieColor),
                    ClassicDefault = Colour4.FromHex(@"ff66ba"),
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            user.BindValueChanged(u =>
            {
                if (u.NewValue?.IsSupporter != true)
                    backgroundSourceDropdown.SetNoticeText(UserInterfaceStrings.NotSupporterNote, true);
                else
                    backgroundSourceDropdown.ClearNoticeText();
            }, true);
        }
    }
}
