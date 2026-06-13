// Copyright (c) jvnkosu! team, MIT license
// See the LICENCE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinListing
{
    public partial class SkinListingOverlay : OnlineOverlay<SkinListingHeader>
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private FillFlowContainer<SkinListingCard> cardsContainer = null!;
        private OsuSpriteText statusText = null!;
        private GetSkinsRequest? activeRequest;
        private SkinUploadDialog? uploadDialog;

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
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourProvider.Background5,
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Masking = true,
                                Padding = new MarginPadding { Horizontal = 20, Vertical = 20 },
                                Child = cardsContainer = new FillFlowContainer<SkinListingCard>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Full,
                                    Spacing = new Vector2(20),
                                }
                            }
                        }
                    }
                }
            };

            filterControl.SearchStarted += performSearch;
            filterControl.TypingStarted += () => statusText.FadeOut(200);
            filterControl.UploadRequested += showUploadDialog;

            AddInternal(statusText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.GetFont(size: 24, weight: FontWeight.SemiBold),
                Alpha = 0,
            });
        }

        protected override SkinListingHeader CreateHeader() => new SkinListingHeader();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            performSearch(string.Empty);
        }

        public void ShowWithSearch(string query)
        {
            filterControl.Search(query);
            Show();
            ScrollFlow.ScrollToStart();
        }

        private void performSearch(string query)
        {
            activeRequest?.Cancel();
            cardsContainer.Clear();

            statusText.Text = "Fetching skins...";
            statusText.FadeIn(200);

            activeRequest = new GetSkinsRequest(query);
            activeRequest.Success += skins => Schedule(() => displaySkins(skins));
            activeRequest.Failure += ex => Schedule(() =>
            {
                statusText.Text = $"An exception has occurred during fetching: {ex.Message}";
                statusText.FadeIn(200);
            });

            api.PerformAsync(activeRequest);
        }

        private void displaySkins(List<APIOnlineSkin> skins)
        {
            statusText.FadeOut(200);
            cardsContainer.Clear();

            if (skins.Count == 0)
            {
                statusText.Text = "...nope, nothing found.";
                statusText.FadeIn(200);
                return;
            }

            foreach (var skin in skins)
                cardsContainer.Add(new SkinListingCard(skin));
        }

        private void showUploadDialog()
        {
            if (uploadDialog == null)
                AddInternal(uploadDialog = new SkinUploadDialog());

            uploadDialog.Show();
        }

        protected override Color4 BackgroundColour => ColourProvider.Background6;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;
    }

    public partial class SkinListingHeader : OverlayHeader
    {
        public SkinListingFilterControl FilterControl { get; private set; } = null!;

        protected override Drawable CreateContent() => FilterControl = new SkinListingFilterControl
        {
            Margin = new MarginPadding { Top = 10, Bottom = 10 }
        };

        protected override OverlayTitle CreateTitle() => new SkinListingTitle();

        private partial class SkinListingTitle : OverlayTitle
        {
            public SkinListingTitle()
            {
                Title = "skins";
                Description = "browse for new skins";
                Icon = OsuIcon.SkinB;
            }
        }
    }
}
