using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinSetInfo : Container
    {
        private const float metadata_width = 185;
        private const float spacing = 20;
        private const float content_top_padding = 15;
        private const float preview_padding = 15;

        private readonly Box background;
        private readonly Box previewBackground;
        private readonly SkinMetadataSectionDescription description;
        private readonly SkinMetadataSectionVersion version;
        private readonly SkinMetadataSectionSkinType skinType;
        private readonly SkinMetadataSectionTags tags;
        private readonly SkinPreviewSection previewSection;
        private readonly Container content;

        public readonly Bindable<APIOnlineSkin?> Skin = new Bindable<APIOnlineSkin?>();

        public SkinSetInfo()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Top = content_top_padding, Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            Padding = new MarginPadding { Right = metadata_width + SkinSetOverlay.RIGHT_WIDTH + spacing * 2 },
                            Child = new OverflowOnlyScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ScrollbarOverlapsContent = false,
                                Child = description = new SkinMetadataSectionDescription(),
                            },
                        },
                        new OverflowOnlyScrollContainer
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = metadata_width,
                            Padding = new MarginPadding { Left = 10 },
                            Margin = new MarginPadding { Right = SkinSetOverlay.RIGHT_WIDTH + spacing },
                            Masking = true,
                            ScrollbarOverlapsContent = false,
                            Child = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Full,
                                Padding = new MarginPadding { Right = 5 },
                                Children = new Drawable[]
                                {
                                    version = new SkinMetadataSectionVersion(),
                                    skinType = new SkinMetadataSectionSkinType(),
                                    tags = new SkinMetadataSectionTags(),
                                },
                            },
                        },
                        // Preview column drives section height (including reserved gallery height while loading).
                        new Container
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Width = SkinSetOverlay.RIGHT_WIDTH,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                previewBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding(preview_padding),
                                    Child = previewSection = new SkinPreviewSection(),
                                },
                            },
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background5;
            previewBackground.Colour = colourProvider.Background4;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Skin.BindValueChanged(s => updateSkin(s.NewValue), true);
            previewSection.Skin.BindTo(Skin);
            previewSection.LayoutInvalidated += invalidateLayout;
        }

        protected override void Dispose(bool isDisposing)
        {
            previewSection.LayoutInvalidated -= invalidateLayout;
            base.Dispose(isDisposing);
        }

        private void invalidateLayout()
        {
            Schedule(() =>
            {
                previewSection.Invalidate(Invalidation.RequiredParentSizeToFit | Invalidation.DrawSize);
                content.Invalidate(Invalidation.RequiredParentSizeToFit | Invalidation.DrawSize);
                Invalidate(Invalidation.RequiredParentSizeToFit | Invalidation.DrawSize);
            });
        }

        private void updateSkin(APIOnlineSkin? skin)
        {
            if (skin == null)
            {
                description.Metadata = string.Empty;
                version.Metadata = string.Empty;
                skinType.Metadata = string.Empty;
                tags.Metadata = string.Empty;
                return;
            }

            description.Metadata = string.IsNullOrWhiteSpace(skin.Description) ? string.Empty : skin.Description.Trim();
            version.Metadata = SkinIniVersionHelper.GetDisplayVersion(skin.Version, skin.Name);
            tags.Metadata = skin.Tags.Trim();
            skinType.Metadata = string.IsNullOrWhiteSpace(skin.EngineType)
                ? string.Empty
                : SkinEngineTypeHelper.GetDisplayName(skin);
        }

        /// <summary>
        /// Scrolls only when content overflows; otherwise lets the parent page scroll handle the input.
        /// </summary>
        private partial class OverflowOnlyScrollContainer : OsuScrollContainer
        {
            protected override bool OnScroll(ScrollEvent e)
            {
                if (ScrollableExtent <= 0)
                    return false;

                if (e.ScrollDelta.Y > 0 && IsScrolledToStart())
                    return false;

                if (e.ScrollDelta.Y < 0 && IsScrolledToEnd())
                    return false;

                return base.OnScroll(e);
            }

            protected override bool OnDragStart(DragStartEvent e)
            {
                if (ScrollableExtent <= 0)
                    return false;

                if (e.Delta.Y > 0 && IsScrolledToStart())
                    return false;

                if (e.Delta.Y < 0 && IsScrolledToEnd())
                    return false;

                return base.OnDragStart(e);
            }
        }
    }
}
