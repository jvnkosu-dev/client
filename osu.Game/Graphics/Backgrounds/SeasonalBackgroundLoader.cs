// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Graphics.Backgrounds
{
    public partial class SeasonalBackgroundLoader : Component
    {
        public event Action<Exception> OnLoadFailure;
        public event Action BackgroundChanged;

        /// <summary>
        /// Fired when categories have been successfully refreshed from the server.
        /// </summary>
        public event Action OnCategoriesRefreshed;

        public readonly Bindable<IEnumerable<string>> AvailableCategories = new Bindable<IEnumerable<string>>(new List<string>());

        [Resolved]
        private IAPIProvider api { get; set; }

        private Bindable<bool> useSeasonalBackgrounds;
        private Bindable<string> selectedCategory;
        private Bindable<APISeasonalBackgrounds> currentBackgrounds;

        private int currentBackgroundIndex;

        private bool shouldShowCustomBackgrounds => useSeasonalBackgrounds.Value;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SessionStatics sessionStatics)
        {
            useSeasonalBackgrounds = config.GetBindable<bool>(OsuSetting.UseSeasonalBackgroundsV2);
            useSeasonalBackgrounds.BindValueChanged(_ => BackgroundChanged?.Invoke());

            selectedCategory = config.GetBindable<string>(OsuSetting.BackgroundCategory);
            selectedCategory.BindValueChanged(_ => fetchBackgroundsForSelectedCategory());

            currentBackgrounds = sessionStatics.GetBindable<APISeasonalBackgrounds>(Static.SeasonalBackgrounds);

            if (shouldShowCustomBackgrounds)
                fetchCategories(true);
        }

        /// <summary>
        /// Public method to trigger a refresh of categories from the UI.
        /// </summary>
        public void RefreshCategories(bool ignoreSuccess = false)
        {
            fetchCategories(ignoreSuccess);
        }

        private void fetchCategories(bool ignoreSuccess = false)
        {
            if (!shouldShowCustomBackgrounds) return;

            var request = new GetBackgroundCategoriesRequest();

            request.Success += response =>
            {
                var serverCategories = response.Categories ?? Enumerable.Empty<string>();

                AvailableCategories.Value = serverCategories.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                if (!AvailableCategories.Value.Any())
                {
                    selectedCategory.Value = "";
                    return; // we don't have any categories!!!
                }

                if (!AvailableCategories.Value.Contains(selectedCategory.Value))
                    selectedCategory.Value = AvailableCategories.Value.Contains("Default")
                    ? "Default"
                    : AvailableCategories.Value.ElementAt(0);

                fetchBackgroundsForSelectedCategory();

                if (!ignoreSuccess)
                    OnCategoriesRefreshed?.Invoke();
            };

            request.Failure += exception =>
            {
                AvailableCategories.Value = Array.Empty<string>();
                OnLoadFailure?.Invoke(exception);
            };

            api.PerformAsync(request);
        }

        private void fetchBackgroundsForSelectedCategory()
        {
            if (!shouldShowCustomBackgrounds) return;

            string categoryToFetch = selectedCategory.Value == "Default" ? null : selectedCategory.Value;
            var request = new GetSeasonalBackgroundsRequest(categoryToFetch);

            request.Success += response =>
            {
                currentBackgrounds.Value = response;
                currentBackgroundIndex = RNG.Next(0, response.Backgrounds?.Count ?? 0);
                BackgroundChanged?.Invoke();
            };

            request.Failure += exception =>
            {
                OnLoadFailure?.Invoke(exception);
            };

            api.PerformAsync(request);
        }

        public Background LoadNextBackground()
        {
            if (!shouldShowCustomBackgrounds || currentBackgrounds.Value?.Backgrounds?.Any() != true)
                return null;

            var backgrounds = currentBackgrounds.Value.Backgrounds;
            currentBackgroundIndex = (currentBackgroundIndex + 1) % backgrounds.Count;
            string url = backgrounds[currentBackgroundIndex].Url;

            return new SeasonalBackground(url);
        }
    }

    [LongRunningLoad]
    public partial class SeasonalBackground : Background
    {
        private readonly string url;
        private const string fallback_texture_name = @"Backgrounds/bg1";

        public SeasonalBackground(string url)
        {
            this.url = url;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            Sprite.Texture = textures.Get(url) ?? textures.Get(fallback_texture_name);
        }

        public override bool Equals(Background other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == GetType()
                   && ((SeasonalBackground)other).url == url;
        }
    }
}
