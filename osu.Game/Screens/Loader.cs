// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Screens.Menu;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Seasonal;
using IntroSequence = osu.Game.Configuration.IntroSequence;
using osu.Game.Audio;

namespace osu.Game.Screens
{
    public partial class Loader : StartupScreen
    {
        [Resolved]
        private OsuConfigManager config { get; set; }
        [Resolved]
        private WelcomeMusicManager musicManager { get; set; }

        private WelcomeMusicMode musicMode;

        public Loader()
        {
            ValidForResume = false;
        }

        private OsuScreen loadableScreen;
        private ShaderPrecompiler precompiler;
        private LoadingSpinner spinner;
        private ScheduledDelegate spinnerShow;

        protected virtual OsuScreen CreateLoadableScreen()
        {
            var introSequence = config.Get<IntroSequence>(OsuSetting.IntroSequence);

            if (musicMode == WelcomeMusicMode.Custom)
                return new IntroFade();

            if (SeasonalUIConfig.ENABLED && !DebugUtils.IsNUnitRunning)
                return new IntroChristmas(createMainMenu);

            if (introSequence == IntroSequence.Random)
                introSequence = (IntroSequence)RNG.Next(0, (int)IntroSequence.Random);

            switch (introSequence)
            {
                case IntroSequence.Circles:
                    return new IntroCircles(createMainMenu);
                case IntroSequence.Welcome:
                    return new IntroWelcome(createMainMenu);
                default:
                    return new IntroTriangles(createMainMenu);
            }
        }

        private static MainMenu createMainMenu() => new MainMenu();

        protected virtual ShaderPrecompiler CreateShaderPrecompiler() => new ShaderPrecompiler();

        public override async void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            musicMode = config.Get<WelcomeMusicMode>(OsuSetting.WelcomeMusicMode);

            await musicManager.PreloadCurrentTrack().ConfigureAwait(true);

            LoadComponentAsync(loadableScreen = CreateLoadableScreen());

            LoadComponentAsync(precompiler = CreateShaderPrecompiler(), AddInternal);
            LoadComponentAsync(spinner = new LoadingSpinner(true, true)
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Margin = new MarginPadding(40),
            }, _ =>
            {
                AddInternal(spinner);
                spinnerShow = Scheduler.AddDelayed(spinner.Show, 200);
            });

            checkIfLoaded();
        }

        private void checkIfLoaded()
        {
            if (loadableScreen?.LoadState != LoadState.Ready || !precompiler.IsLoaded)
            {
                Schedule(checkIfLoaded);
                return;
            }

            spinnerShow?.Cancel();

            if (spinner.State.Value == Visibility.Visible)
            {
                spinner.Hide();
                Scheduler.AddDelayed(() => this.Push(loadableScreen), LoadingSpinner.TRANSITION_DURATION);
            }
            else
                this.Push(loadableScreen);
        }

        public partial class ShaderPrecompiler : Drawable
        {
            // ... код ShaderPrecompiler остается без изменений ... (Блять, а где он?)
        }
    }
}
