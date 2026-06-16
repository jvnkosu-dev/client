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
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinListing
{
    public partial class SkinListingSearchControl : CompositeDrawable
    {
        public Action? TypingStarted;

        public Bindable<string> Query => textBox.Current;

        private readonly SkinSearchTextBox textBox;

        private readonly Box background;

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

        private partial class SkinSearchTextBox : BasicSearchTextBox
        {
            public Action? TextChanged;

            protected override Color4 SelectionColour => Color4.Gray;

            public SkinSearchTextBox()
            {
                PlaceholderText = "Введите название скина или автора...";
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
    }
}
