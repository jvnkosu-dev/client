// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Audio
{
    [Cached]
    public partial class WelcomeMusicManager : Drawable
    {
        public event Action<Exception> OnLoadFailure;
        public event Action OnCategoriesRefreshed;

        public readonly Bindable<IEnumerable<string>> AvailableCategories = new Bindable<IEnumerable<string>>();

        private ITrack preloadedTrack;
        private List<APIWelcomeMusic> currentTracks;

        [Resolved]
        private IAPIProvider api { get; set; }
        [Resolved]
        private AudioManager audioManager { get; set; }
        [Resolved]
        private OsuConfigManager config { get; set; }
        [Resolved]
        private GameHost host { get; set; }

        private Bindable<WelcomeMusicMode> musicMode;
        private Bindable<string> selectedCategory;

        [BackgroundDependencyLoader]
        private void load()
        {
            musicMode = config.GetBindable<WelcomeMusicMode>(OsuSetting.WelcomeMusicMode);
            selectedCategory = config.GetBindable<string>(OsuSetting.WelcomeMusicCategory);
            fetchCategories();
        }

        public void RefreshCategories() => fetchCategories();

        private void fetchCategories()
        {
            var request = new GetMusicCategoriesRequest();
            request.Success += response =>
            {
                var serverCategories = response.Categories ?? Enumerable.Empty<string>();
                AvailableCategories.Value = serverCategories.ToList();
                OnCategoriesRefreshed?.Invoke();
            };
            request.Failure += exception =>
            {
                Logger.Error(exception, "ОШИБКА: Не удалось загрузить категории музыки!");
                AvailableCategories.Value = new[] { "Не удалось загрузить..." };
                OnLoadFailure?.Invoke(exception);
            };
            api.PerformAsync(request);
        }

        public async Task PreloadCurrentTrack()
        {
            if (musicMode.Value == WelcomeMusicMode.Default)
            {
                preloadedTrack = audioManager.Tracks.Get("Samples/welcome.ogg");
                return;
            }

            if (string.IsNullOrEmpty(selectedCategory.Value) || selectedCategory.Value.Contains("Не удалось"))
                return;

            var request = new GetWelcomeMusicRequest(selectedCategory.Value);
            var tcs = new TaskCompletionSource<bool>();

            request.Success += response =>
            {
                currentTracks = response;
                tcs.SetResult(true);
            };
            request.Failure += exception =>
            {
                Logger.Error(exception, "ОШИБКА: Не удалось загрузить список треков!");
                tcs.SetResult(false);
            };

            api.PerformAsync(request);
            await   tcs.Task;

            if (currentTracks?.Any() != true)
                return;

            var randomTrackInfo = currentTracks[RNG.Next(0, currentTracks.Count)];

            try
            {
                preloadedTrack = audioManager.Tracks.Get(randomTrackInfo.Url);
                if (preloadedTrack != null)
                    preloadedTrack.Looping = false;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"ОШИБКА: Не удалось загрузить трек по URL: {randomTrackInfo.Url}");
            }
        }

        public ITrack GetPreloadedTrack() => preloadedTrack;

        public void RequestRestart()
        {
            host.Exit();
        }
    }
}
