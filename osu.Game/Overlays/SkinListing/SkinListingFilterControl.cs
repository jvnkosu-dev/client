using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinListing
{
    public partial class SkinListingFilterControl : CompositeDrawable
    {
        public Action<SearchResult>? SearchFinished;
        public Action? SearchStarted;
        public Action? TypingStarted;

        public IBindable<BeatmapCardSize> CardSize => cardSize;

        private readonly Bindable<BeatmapCardSize> cardSize = new Bindable<BeatmapCardSize>();

        private readonly SkinListingSearchControl searchControl;
        private readonly SkinListingSortTabControl sortControl;
        private readonly Box sortControlBackground;

        private ScheduledDelegate? queryChangedDebounce;
        private GetSkinsRequest? getSkinsRequest;
        private List<APIOnlineSkin> lastResults = new List<APIOnlineSkin>();

        private IBindable<APIUser> apiUser = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        public SkinListingFilterControl()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Masking = true,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Colour = Color4.Black.Opacity(0.25f),
                            Type = EdgeEffectType.Shadow,
                            Radius = 3,
                            Offset = new Vector2(0f, 1f),
                        },
                        Child = searchControl = new SkinListingSearchControl
                        {
                            TypingStarted = () => TypingStarted?.Invoke()
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Children = new Drawable[]
                        {
                            sortControlBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            sortControl = new SkinListingSortTabControl
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Left = 20 }
                            },
                            new SkinListingCardSizeTabControl
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Margin = new MarginPadding { Right = 20 },
                                Current = { BindTarget = cardSize }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            sortControlBackground.Colour = colourProvider.Background4;
        }

        public void Search(string query)
            => Schedule(() => searchControl.Query.Value = query);

        public void TakeFocus() => searchControl.TakeFocus();

        /// <summary>
        /// Re-fetch skins from the server using the current search criteria.
        /// </summary>
        public void Refresh() => Schedule(() => queueUpdateSearch());

        protected override void LoadComplete()
        {
            base.LoadComplete();

            config.BindWith(OsuSetting.SkinListingCardSize, cardSize);

            cardSize.BindValueChanged(v =>
            {
                if (v.NewValue != BeatmapCardSize.Normal && v.NewValue != BeatmapCardSize.Extra)
                    cardSize.Value = BeatmapCardSize.Normal;
            }, true);

            searchControl.Query.BindValueChanged(_ =>
            {
                resetSortControl();
                queueUpdateSearch(true);
            });

            sortControl.Current.BindValueChanged(_ => resortAndPublish());
            sortControl.SortDirection.BindValueChanged(_ => resortAndPublish());

            searchControl.ModifiedModes.BindCollectionChanged((_, _) => resortAndPublish());
            searchControl.EngineType.BindValueChanged(_ => resortAndPublish());

            apiUser = api.LocalUser.GetBoundCopy();
            apiUser.BindValueChanged(_ =>
            {
                if (!api.IsLoggedIn)
                {
                    resetSearch();
                    lastResults.Clear();
                    searchControl.Skin = null;
                    return;
                }

                queueUpdateSearch();
            });

            queueUpdateSearch();
        }

        private void resetSortControl() => sortControl.Reset(!string.IsNullOrEmpty(searchControl.Query.Value));

        private void queueUpdateSearch(bool queryTextChanged = false)
        {
            resetSearch();

            if (!api.IsLoggedIn)
                return;

            SearchStarted?.Invoke();

            queryChangedDebounce = Scheduler.AddDelayed(performRequest, queryTextChanged ? 500 : 100);
        }

        private void performRequest()
        {
            getSkinsRequest?.Cancel();
            getSkinsRequest = new GetSkinsRequest(searchControl.Query.Value);

            getSkinsRequest.Success += skins =>
            {
                lastResults = sortSkins(skins);
                getSkinsRequest = null;
                searchControl.Skin = getFeaturedSkin(skins);
                publishResults();
            };

            getSkinsRequest.Failure += _ =>
            {
                getSkinsRequest = null;
                SearchFinished?.Invoke(SearchResult.Failed());
            };

            api.PerformAsync(getSkinsRequest);
        }

        private void resortAndPublish()
        {
            if (!lastResults.Any())
                return;

            SearchStarted?.Invoke();
            lastResults = sortSkins(lastResults);
            publishResults();
        }

        private void publishResults()
        {
            var filtered = SkinModifiedModesHelper.Filter(lastResults, searchControl.ModifiedModes);
            filtered = SkinEngineTypeHelper.Filter(filtered, searchControl.EngineType.Value);
            SearchFinished?.Invoke(SearchResult.ResultsReturned(filtered.ToList()));
        }

        private static APIOnlineSkin? getFeaturedSkin(IEnumerable<APIOnlineSkin> skins) =>
            skins.OrderByDescending(s => s.LastUpdated ?? s.CreatedAt ?? DateTimeOffset.MinValue).FirstOrDefault();

        private List<APIOnlineSkin> sortSkins(List<APIOnlineSkin> skins)
        {
            IEnumerable<APIOnlineSkin> ordered = sortControl.Current.Value switch
            {
                SortCriteria.Name => skins.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase),
                SortCriteria.Creator => skins.OrderBy(s => s.Creator, StringComparer.OrdinalIgnoreCase),
                SortCriteria.Updated => skins.OrderBy(s => s.LastUpdated ?? DateTimeOffset.MinValue),
                SortCriteria.Favourites => skins.OrderBy(s => s.FavouriteCount),
                SortCriteria.Relevance => skins,
                _ => skins
            };

            if (sortControl.SortDirection.Value == SortDirection.Ascending)
                ordered = ordered.Reverse();

            return ordered.ToList();
        }

        private void resetSearch()
        {
            getSkinsRequest?.Cancel();
            getSkinsRequest = null;
            queryChangedDebounce?.Cancel();
        }

        protected override void Dispose(bool isDisposing)
        {
            resetSearch();
            base.Dispose(isDisposing);
        }

        public enum SearchResultType
        {
            ResultsReturned,
            Failed,
        }

        public struct SearchResult
        {
            public SearchResultType Type { get; private set; }
            public List<APIOnlineSkin> Results { get; private set; }

            public static SearchResult ResultsReturned(List<APIOnlineSkin> results) => new SearchResult
            {
                Type = SearchResultType.ResultsReturned,
                Results = results,
            };

            public static SearchResult Failed() => new SearchResult
            {
                Type = SearchResultType.Failed,
                Results = new List<APIOnlineSkin>(),
            };
        }
    }
}
