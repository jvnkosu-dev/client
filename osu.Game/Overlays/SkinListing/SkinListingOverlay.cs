using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.SkinListing.Drawables.Cards;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinListing
{
    public partial class SkinListingOverlay : OnlineOverlay<SkinListingHeader>
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private IBindable<APIUser> apiUser = null!;

        private Container panelTarget = null!;
        private ReverseChildIDFillFlowContainer<SkinCard> foundContent = null!;

        private SkinListingFilterControl filterControl => Header.FilterControl;

        public SkinListingOverlay()
            : base(OverlayColourScheme.Blue)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourProvider.Background5,
                            },
                            panelTarget = new Container
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Masking = true,
                                Padding = new MarginPadding { Horizontal = 20 },
                            }
                        },
                    },
                }
            };

            filterControl.TypingStarted = onTypingStarted;
            filterControl.SearchStarted = onSearchStarted;
            filterControl.SearchFinished = onSearchFinished;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            filterControl.CardSize.BindValueChanged(_ => onCardSizeChanged());

            apiUser = api.LocalUser.GetBoundCopy();
            apiUser.BindValueChanged(_ => Schedule(onUserChanged));
        }

        private void onUserChanged()
        {
            cancellationToken?.Cancel();
            Loading.Hide();

            if (api.IsLoggedIn)
                replaceResultsAreaContent(Empty());
            else
                panelTarget.Clear();
        }

        public void ShowWithSearch(string query)
        {
            filterControl.Search(query);
            Show();
            ScrollFlow.ScrollToStart();
        }

        /// <summary>
        /// Re-fetch skins from the server and update the displayed results.
        /// </summary>
        public void RefreshListing()
        {
            filterControl.Refresh();
            ScrollFlow.ScrollToStart();
        }

        protected override SkinListingHeader CreateHeader() => new SkinListingHeader();

        protected override Color4 BackgroundColour => ColourProvider.Background6;

        private void onTypingStarted()
        {
            ScrollFlow.ScrollToStart();
        }

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);
            filterControl.TakeFocus();
        }

        private CancellationTokenSource? cancellationToken;
        private Task? panelLoadTask;

        private void onSearchStarted()
        {
            cancellationToken?.Cancel();

            if (panelTarget.Any())
                Loading.Show();
        }

        private void onSearchFinished(SkinListingFilterControl.SearchResult searchResult)
        {
            cancellationToken?.Cancel();

            if (searchResult.Type == SkinListingFilterControl.SearchResultType.Failed)
            {
                replaceResultsAreaContent(new ErrorDrawable());
                return;
            }

            var newCards = createCardsFor(searchResult.Results);

            if (!newCards.Any())
            {
                replaceResultsAreaContent(new NotFoundDrawable());
                return;
            }

            var content = createCardContainerFor(newCards);
            panelLoadTask = LoadComponentAsync(foundContent = content, replaceResultsAreaContent, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private IEnumerable<SkinCard> createCardsFor(IEnumerable<APIOnlineSkin> skins) => skins.Select(skin => SkinCard.Create(skin, filterControl.CardSize.Value).With(c =>
        {
            c.Anchor = Anchor.TopCentre;
            c.Origin = Anchor.TopCentre;
        })).ToArray();

        private static ReverseChildIDFillFlowContainer<SkinCard> createCardContainerFor(IEnumerable<SkinCard> newCards)
        {
            return new ReverseChildIDFillFlowContainer<SkinCard>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10),
                Alpha = 0,
                Margin = new MarginPadding
                {
                    Top = 15,
                    Bottom = ExpandedContentScrollContainer.HEIGHT + 20
                },
                ChildrenEnumerable = newCards
            };
        }

        private void replaceResultsAreaContent(Drawable content)
        {
            Loading.Hide();
            panelTarget.Child = content;
            content.FadeInFromZero();
        }

        private void onCardSizeChanged()
        {
            if (foundContent?.IsAlive != true || !foundContent.Any())
                return;

            Loading.Show();

            var newCards = createCardsFor(foundContent.Reverse().Select(card => card.Skin));

            cancellationToken?.Cancel();

            panelLoadTask = LoadComponentsAsync(newCards, cards =>
            {
                foundContent.Clear();
                foundContent.AddRange(cards);
                Loading.Hide();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }

        public partial class NotFoundDrawable : CompositeDrawable
        {
            public NotFoundDrawable()
            {
                RelativeSizeAxes = Axes.X;
                Height = 250;
                Alpha = 0;
                Margin = new MarginPadding { Top = 15 };
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures)
            {
                AddInternal(new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10, 0),
                    Children = new Drawable[]
                    {
                        new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Texture = textures.Get(@"Online/not-found")
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "Ничего не найдено :(",
                        }
                    }
                });
            }
        }

        public partial class ErrorDrawable : CompositeDrawable
        {
            public ErrorDrawable()
            {
                RelativeSizeAxes = Axes.X;
                Height = 250;
                Alpha = 0;
                Margin = new MarginPadding { Top = 15 };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AddInternal(new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "Не удалось загрузить список скинов",
                    Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                });
            }
        }
    }
}
