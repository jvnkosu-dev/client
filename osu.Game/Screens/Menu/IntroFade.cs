// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Audio;

namespace osu.Game.Screens.Menu
{
    public partial class IntroFade : OsuScreen
    {
        [Resolved]
        private WelcomeMusicManager? musicManager { get; set; }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            this.FadeInFromZero(1000, Easing.OutQuint);

#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
            var track = musicManager.GetPreloadedTrack();
#pragma warning restore CS8602 // Разыменование вероятной пустой ссылки.
            track?.Start();

            Scheduler.AddDelayed(() => this.Push(new MainMenu()), 2000);
        }
    }
}
