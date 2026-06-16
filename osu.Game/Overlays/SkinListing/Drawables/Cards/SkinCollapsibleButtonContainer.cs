using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Graphics;
using osu.Game.Online;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class SkinCollapsibleButtonContainer : Container
    {
        public Bindable<bool> ShowDetails = new Bindable<bool>();

        private readonly IBindable<DownloadState> downloadState;
        private readonly SkinDownloadButton downloadButton;
        private readonly GoToSkinButton goToSkinButton;

        public float ButtonsExpandedWidth
        {
            get => buttonsExpandedWidth;
            set
            {
                buttonsExpandedWidth = value;
                if (IsLoaded)
                    updateState();
            }
        }

        private float buttonsCollapsedWidth;

        public float ButtonsCollapsedWidth
        {
            get => buttonsCollapsedWidth;
            set
            {
                buttonsCollapsedWidth = value;
                if (IsLoaded)
                    updateState();
            }
        }

        public IEnumerable<BeatmapCardIconButton> Buttons => buttons;

        protected override Container<Drawable> Content => mainContent;

        private readonly Container background;
        private readonly Container buttonArea;
        private readonly Container<BeatmapCardIconButton> buttons;
        private readonly Container mainArea;
        private readonly Container mainContent;

        private readonly APIOnlineSkin skin;
        private float buttonsExpandedWidth;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public SkinCollapsibleButtonContainer(APIOnlineSkin skin, IBindable<DownloadState> downloadState)
        {
            this.skin = skin;
            this.downloadState = downloadState;

            RelativeSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = BeatmapCard.CORNER_RADIUS;

            InternalChildren = new Drawable[]
            {
                background = new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.White
                    },
                },
                buttonArea = new Container
                {
                    Name = @"Right (button) area",
                    RelativeSizeAxes = Axes.Y,
                    Origin = Anchor.TopRight,
                    Anchor = Anchor.TopRight,
                    Child = buttons = new Container<BeatmapCardIconButton>
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new BeatmapCardIconButton[]
                        {
                            downloadButton = new SkinDownloadButton(skin)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                            },
                            goToSkinButton = new GoToSkinButton(skin)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    }
                },
                mainArea = new Container
                {
                    Name = @"Main content",
                    RelativeSizeAxes = Axes.Y,
                    CornerRadius = BeatmapCard.CORNER_RADIUS,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new SkinCardContentBackground(skin.GetThumbnailRequestUrl())
                        {
                            RelativeSizeAxes = Axes.Both,
                            Dimmed = { BindTarget = ShowDetails },
                        },
                        mainContent = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Horizontal = 10,
                                Vertical = 4
                            },
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            downloadState.BindValueChanged(e =>
            {
                downloadButton.State.Value = e.NewValue;
                goToSkinButton.State.Value = e.NewValue;
                updateState();
            }, true);

            ShowDetails.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            buttonArea.Width = buttonsExpandedWidth;

            float buttonAreaWidth = ShowDetails.Value ? ButtonsExpandedWidth : ButtonsCollapsedWidth;
            float mainAreaWidth = DrawWidth - buttonAreaWidth;

            mainArea.ResizeWidthTo(mainAreaWidth, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

            background.ResizeWidthTo(buttonAreaWidth + BeatmapCard.CORNER_RADIUS, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            if (ButtonsCollapsedWidth == 0)
                background.FadeTo(ShowDetails.Value ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

            background.FadeColour(downloadState.Value == DownloadState.LocallyAvailable ? colours.Lime0 : colourProvider.Background3, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            buttons.FadeTo(ShowDetails.Value ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

            foreach (var button in buttons)
            {
                button.IdleColour = downloadState.Value != DownloadState.LocallyAvailable ? colourProvider.Light1 : colourProvider.Background3;
                button.HoverColour = downloadState.Value != DownloadState.LocallyAvailable ? colourProvider.Content1 : colourProvider.Foreground1;
            }
        }
    }
}
