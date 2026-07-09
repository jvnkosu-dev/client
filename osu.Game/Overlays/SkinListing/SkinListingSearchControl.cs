using System;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.SkinListing.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinListing
{
    public partial class SkinListingSearchControl : CompositeDrawable
    {
        public Action? TypingStarted;

        public Bindable<string> Query => textBox.Current;

        public BindableList<SkinListingModifiedMode> ModifiedModes => modifiedModesFilter.Current;

        public Bindable<SkinEngineType?> EngineType => engineTypeFilter.Current;

        public APIOnlineSkin? Skin
        {
            set => setSkinCover(value);
        }

        private readonly SkinSearchTextBox textBox;
        private readonly SkinSearchModifiedModesFilterRow modifiedModesFilter;
        private readonly SkinSearchEngineTypeFilterRow engineTypeFilter;

        private readonly Box background;
        private readonly UpdateableOnlineSkinCover skinCover;

        public SkinListingSearchControl()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            AddRangeInternal(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = skinCover = new TopSearchSkinCover
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                    },
                },
                new Container
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding
                    {
                        Vertical = 20,
                        Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING,
                    },
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 20),
                        Children = new Drawable[]
                        {
                            textBox = new SkinSearchTextBox
                            {
                                RelativeSizeAxes = Axes.X,
                                TextChanged = () => TypingStarted?.Invoke(),
                            },
                            new ReverseChildIDFillFlowContainer<Drawable>
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Direction = FillDirection.Vertical,
                                Padding = new MarginPadding { Horizontal = 10 },
                                Spacing = new Vector2(5),
                                Children = new Drawable[]
                                {
                                    modifiedModesFilter = new SkinSearchModifiedModesFilterRow(),
                                    engineTypeFilter = new SkinSearchEngineTypeFilterRow(),
                                }
                            },
                        }
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Dark6;
        }

        public void TakeFocus() => textBox.TakeFocus();

        private void setSkinCover(APIOnlineSkin? skin)
        {
            if (skin == null || string.IsNullOrEmpty(skin.GetThumbnailRequestUrl()))
            {
                skinCover.FadeOut(600, Easing.OutQuint);
                return;
            }

            skinCover.Skin = skin;
            skinCover.FadeTo(0.1f, 200, Easing.OutQuint);
        }

        private partial class SkinSearchTextBox : BasicSearchTextBox
        {
            public Action? TextChanged;

            protected override Color4 SelectionColour => Color4.Gray;

            public SkinSearchTextBox()
            {
                PlaceholderText = "Search for a skin or author...";
            }

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                if (!base.OnKeyDown(e))
                    return false;

                TextChanged?.Invoke();
                return true;
            }

            public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                if (!base.OnPressed(e))
                    return false;

                TextChanged?.Invoke();
                return true;
            }
        }

        private partial class TopSearchSkinCover : UpdateableOnlineSkinCover
        {
            protected override bool TransformImmediately => true;
        }
    }
}
