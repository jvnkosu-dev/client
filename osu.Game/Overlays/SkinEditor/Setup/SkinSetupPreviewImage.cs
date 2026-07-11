// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.IO;
using osu.Game.Skinning;
using FileInfo = System.IO.FileInfo;

namespace osu.Game.Overlays.SkinEditor.Setup
{
    public partial class SkinSetupPreviewImage : CompositeDrawable
    {
        public Bindable<FileInfo?> PreviewFile { get; } = new Bindable<FileInfo?>();

        private readonly Container content;
        private Sprite sprite = null!;

        public SkinSetupPreviewImage()
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

        public void ShowSkinBackground(Skin skin, IRenderer renderer, OsuColour colours, IStorageResourceProvider resources)
        {
            content.Clear();

            var texture = SkinBackgroundHelper.GetTexture(skin, renderer, resources);

            if (texture == null)
            {
                showPlaceholder(colours, "No background image");
                return;
            }

            content.Add(sprite = new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fill,
                Texture = texture,
            });

            sprite.FadeInFromZero(300);
        }

        private void updatePreview(IRenderer renderer, OsuColour colours)
        {
            content.Clear();

            var file = PreviewFile.Value;

            if (file == null || !file.Exists)
            {
                showPlaceholder(colours, "Select a background image");
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
                showPlaceholder(colours, "Failed to load preview");
            }
        }

        private void showPlaceholder(OsuColour colours, string text)
        {
            content.Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colours.GreySeaFoamDarker,
            });
            content.Add(new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 16))
            {
                Text = text,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                TextAnchor = Anchor.Centre,
            });
        }
    }
}
