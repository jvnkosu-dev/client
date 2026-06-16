using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Submission
{
    public partial class SkinSubmissionPreviewImage : CompositeDrawable
    {
        public Bindable<FileInfo?> PreviewFile { get; } = new Bindable<FileInfo?>();

        private readonly Container content;
        private Sprite sprite = null!;

        public SkinSubmissionPreviewImage()
        {
            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 3.5f,
                Child = sprite = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, IRenderer renderer)
        {
            PreviewFile.BindValueChanged(_ => updatePreview(renderer, colours), true);
        }

        private void updatePreview(IRenderer renderer, OsuColour colours)
        {
            content.Clear();

            var file = PreviewFile.Value;

            if (file == null || !file.Exists)
            {
                content.Add(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.GreySeaFoamDarker,
                });
                content.Add(new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 16))
                {
                    Text = "Выберите изображение превью",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    TextAnchor = Anchor.Centre,
                });
                return;
            }

            try
            {
                using var stream = file.OpenRead();
                var texture = Framework.Graphics.Textures.Texture.FromStream(renderer, stream);

                content.Add(sprite = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                    Texture = texture,
                });

                sprite.FadeInFromZero(300);
            }
            catch
            {
                content.Add(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.GreySeaFoamDarker,
                });
                content.Add(new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 16))
                {
                    Text = "Не удалось загрузить превью",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    TextAnchor = Anchor.Centre,
                });
            }
        }
    }
}
