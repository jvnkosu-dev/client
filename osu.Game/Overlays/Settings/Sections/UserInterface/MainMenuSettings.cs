// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Settings;

namespace osu.Game.Overlays.Settings.Sections.UserInterface
{
    public partial class MainMenuSettings : SettingsSubsection
    {
        protected override LocalisableString Header => UserInterfaceStrings.MainMenuHeader;

        [Resolved]
        private SeasonalBackgroundLoader backgroundLoader { get; set; }

        private IBindable<APIUser> user;

        private SettingsEnumDropdown<BackgroundSource> backgroundSourceDropdown;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api)
        {
            user = api.LocalUser.GetBoundCopy();

            var backgroundModeBindable = config.GetBindable<SeasonalBackgroundMode>(OsuSetting.SeasonalBackgroundMode);
            var enabledProxyBindable = new Bindable<bool>();

            backgroundModeBindable.BindValueChanged(mode => enabledProxyBindable.Value = mode.NewValue == SeasonalBackgroundMode.Always, true);
            enabledProxyBindable.BindValueChanged(enabled => backgroundModeBindable.Value = enabled.NewValue ? SeasonalBackgroundMode.Always : SeasonalBackgroundMode.Never);

            var backgroundToggle = new SettingsCheckbox
            {
                LabelText = UserInterfaceStrings.UseSeasonalBackgrounds,
                Current = enabledProxyBindable
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

            backgroundLoader.AvailableCategories.BindValueChanged(categories => categoryDropdown.Items = categories.NewValue, true);

            backgroundModeBindable.BindValueChanged(mode =>
            {
                if (mode.NewValue == SeasonalBackgroundMode.Always)
                {
                    categoryDropdown.Show();
                    refreshButton.Show();
                }
                else
                {
                    categoryDropdown.Hide();
                    refreshButton.Hide();
                }
            }, true);

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = UserInterfaceStrings.ShowMenuTips,
                    Current = config.GetBindable<bool>(OsuSetting.MenuTips)
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
                    LabelText = @"osu! logo colour",
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
