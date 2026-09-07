// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
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

        private SeasonalBackgroundLoader? backgroundLoader;

        private IBindable<APIUser> user = null!;
        private Bindable<BackgroundSource> backgroundSource = null!;

        private readonly Bindable<SettingsNote.Data?> backgroundSourceNote = new Bindable<SettingsNote.Data?>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api, SeasonalBackgroundLoader? backgroundLoader)
        {
            user = api.LocalUser.GetBoundCopy();

            this.backgroundLoader = backgroundLoader;

            var backgroundSource = this.backgroundSource = config.GetBindable<BackgroundSource>(OsuSetting.MenuBackgroundSource);
            var showCustomControls = new BindableBool(backgroundSource.Value == BackgroundSource.Custom);

            var categoryDropdown = new FormDropdown<string>
            {
                Caption = UserInterfaceStrings.SeasonalBackgroundsCategories,
                Current = config.GetBindable<string>(OsuSetting.BackgroundCategory)
            };

            var categoryItem = new SettingsItemV2(categoryDropdown);
            categoryItem.CanBeShown.BindTo(showCustomControls);

            var refreshButton = new SettingsButtonV2
            {
                Text = UserInterfaceStrings.SeasonalBackgroundsRefresh,
                Action = () => backgroundLoader?.RefreshCategories()
            };
            refreshButton.CanBeShown.BindTo(showCustomControls);

            backgroundSource.BindValueChanged(s =>
            {
                showCustomControls.Value = s.NewValue == BackgroundSource.Custom;

                if (s.NewValue == BackgroundSource.Custom)
                    backgroundLoader?.RefreshCategories(true);
            }, true);

            backgroundLoader?.AvailableCategories.BindValueChanged(categories => Schedule(() => categoryDropdown.Items = categories.NewValue), true);

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
                    Current = backgroundSource,
                })
                {
                    Note = { BindTarget = backgroundSourceNote }
                },
                categoryItem,
                refreshButton,
                new SettingsItemV2(new FormColour
                {
                    Caption = UserInterfaceStrings.LogoColour,
                    Current = config.GetBindable<Colour4>(OsuSetting.MenuCookieColor),
                })
                {
                    ApplyClassicDefault = c => ((IHasCurrentValue<Colour4>)c).Current.Value = Colour4.FromHex(@"ff66ba"),
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

            user.BindValueChanged(_ => updateBackgroundSourceNote(), true);
            backgroundSource.BindValueChanged(_ => updateBackgroundSourceNote(), true);
        }

        private void updateBackgroundSourceNote()
        {
            if (backgroundSource.Value == BackgroundSource.Custom || user.Value?.IsSupporter == true)
                backgroundSourceNote.Value = null;
            else
                backgroundSourceNote.Value = new SettingsNote.Data(UserInterfaceStrings.NotSupporterNote, SettingsNote.Type.Informational);
        }
    }
}
