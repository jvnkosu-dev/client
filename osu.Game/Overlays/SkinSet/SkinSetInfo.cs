using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinSetInfo : Container
    {
        private const float metadata_width = 185;
        private const float spacing = 20;
        private const float base_height = 300;

        private readonly Box background;
        private readonly SkinMetadataSectionDescription description;
        private readonly SkinMetadataSectionVersion version;
        private readonly SkinMetadataSectionSkinType skinType;
        private readonly SkinMetadataSectionTags tags;

        public readonly Bindable<APIOnlineSkin?> Skin = new Bindable<APIOnlineSkin?>();

        public SkinSetInfo()
        {
            RelativeSizeAxes = Axes.X;
            Height = base_height;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 15, Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Right = metadata_width + SkinSetOverlay.RIGHT_WIDTH + spacing * 2 },
                            Child = description = new SkinMetadataSectionDescription(),
                        },
                        new OsuScrollContainer
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
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Skin.BindValueChanged(s => updateSkin(s.NewValue), true);
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
    }
}
