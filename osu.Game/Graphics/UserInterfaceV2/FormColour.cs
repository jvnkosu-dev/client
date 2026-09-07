// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormColour : CompositeDrawable, IHasCurrentValue<Colour4>, IFormControl, IHasPopover
    {
        public Bindable<Colour4> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableWithCurrent<Colour4> current = new BindableWithCurrent<Colour4>(Colour4.White);

        public LocalisableString Caption { get; init; }

        public LocalisableString HintText { get; init; }

        private FormControlBackground background = null!;
        private FormFieldCaption caption = null!;
        private Box fill = null!;
        private OsuSpriteText colourHexCode = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground(),
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(9),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Right = 85 },
                            Child = caption = new FormFieldCaption
                            {
                                Caption = Caption,
                                TooltipText = HintText,
                            },
                        },
                        new CircularContainer
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Size = new Vector2(80, 28),
                            Masking = true,
                            Children = new Drawable[]
                            {
                                fill = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                colourHexCode = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.Style.Caption1,
                                },
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            current.BindValueChanged(_ =>
            {
                updateColour();
                updateState();
                background.FlashOnCommit();
                ValueChanged?.Invoke();
            });
            current.BindDisabledChanged(_ => updateState(), true);
            updateColour();
        }

        private void updateColour()
        {
            fill.Colour = Current.Value;
            colourHexCode.Text = Current.Value.ToHex();
            colourHexCode.Colour = OsuColour.ForegroundTextColourFor(Current.Value);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateState();
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (IsDisabled)
                return true;

            this.ShowPopover();
            return true;
        }

        private void updateState()
        {
            caption.Colour = Current.Disabled ? colourProvider.Background1 : colourProvider.Content2;

            if (IsDisabled)
                background.VisualStyle = VisualStyle.Disabled;
            else if (IsHovered)
                background.VisualStyle = VisualStyle.Hovered;
            else
                background.VisualStyle = VisualStyle.Normal;
        }

        public Popover GetPopover() => new OsuPopover(false)
        {
            Child = new OsuColourPicker
            {
                Current = { BindTarget = Current }
            }
        };

        public IEnumerable<LocalisableString> FilterTerms => Caption.Yield();

        public event Action? ValueChanged;

        public bool IsDefault => Current.IsDefault;

        public void SetDefault() => Current.SetDefault();

        public bool IsDisabled => Current.Disabled;

        public float MainDrawHeight => DrawHeight;
    }
}
