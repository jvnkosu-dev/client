// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning.Preview
{
    /// <summary>
    /// Displays a ruleset skin preview gallery for a loaded skin instance.
    /// </summary>
    public partial class SkinPreviewGalleryHost : Container
    {
        private const float gallery_spacing = 10;
        private const int default_gallery_rows = 3;

        /// <summary>
        /// Height of the default osu! 2×3 preview grid, reserved while loading so the page doesn't jump.
        /// </summary>
        private static readonly float default_gallery_height =
            default_gallery_rows * SkinPreviewCard.CARD_SIZE + (default_gallery_rows - 1) * gallery_spacing;

        public Bindable<RulesetInfo> PreviewRuleset { get; } = new Bindable<RulesetInfo>();

        private readonly Container galleryHost;
        private readonly Box galleryHeightSpacer;
        private readonly LoadingSpinner loadingSpinner;
        private readonly OsuSpriteText statusText;

        private SkinPreviewHandle? previewHandle;
        private bool ownsPreviewHandle;
        private Skin? currentSkin;
        private int loadGeneration;

        public event Action? LayoutInvalidated;

        public SkinPreviewGalleryHost()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        statusText = new OsuSpriteText
                        {
                            Font = OsuFont.Default.With(size: 13),
                            Colour = Color4.White,
                            Alpha = 0.6f,
                        },
                        galleryHost = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                // Reserve default 2×3 gallery height while the spinner is showing.
                                // AlwaysPresent is required: Alpha=0 drawables are otherwise skipped by AutoSize.
                                galleryHeightSpacer = new Box
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = default_gallery_height,
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                },
                                loadingSpinner = new LoadingSpinner
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                            },
                        },
                    },
                },
            };
        }

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        public void ShowLoading(string message = "Loading preview...")
        {
            clearGallery();
            statusText.Hide();
            loadingSpinner.Show();
            this.Show();
            invalidateLayout();
        }

        public void ShowFailure(string message)
        {
            clearGallery();
            loadingSpinner.Hide();
            statusText.Text = message;
            statusText.Show();
            this.Show();
            invalidateLayout();
        }

        public void ShowSkin(Skin skin)
        {
            clearGallery();
            currentSkin = skin;
            showGalleryForSkin(skin);
        }

        public void ShowSkin(SkinPreviewHandle handle, bool takeOwnership = false)
        {
            clearGallery();
            previewHandle = handle;
            ownsPreviewHandle = takeOwnership;
            currentSkin = handle.Skin;
            showGalleryForSkin(handle.Skin);
        }

        public void HidePreview()
        {
            clearGallery();
            currentSkin = null;
            loadingSpinner.Hide();
            this.Hide();
            invalidateLayout();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            PreviewRuleset.BindValueChanged(_ => refreshGalleryIfLoaded());
        }

        private void refreshGalleryIfLoaded()
        {
            if (currentSkin == null)
                return;

            loadGeneration++;
            clearGalleryContent();
            showGalleryForSkin(currentSkin);
        }

        private void showGalleryForSkin(Skin skin)
        {
            var previewRuleset = PreviewRuleset.Value?.CreateInstance() ?? rulesets.GetRuleset(0)?.CreateInstance();

            if (previewRuleset == null)
            {
                ShowFailure("Preview unavailable");
                return;
            }

            var gallery = previewRuleset.CreateSkinPreviewGallery(skin, previewRuleset);

            if (gallery == null)
            {
                ShowFailure("Preview unavailable for this skin");
                return;
            }

            statusText.Hide();
            loadingSpinner.Show();

            int generation = loadGeneration;

            LoadComponentAsync(gallery, loaded =>
            {
                if (generation != loadGeneration)
                {
                    loaded.Dispose();
                    return;
                }

                loadingSpinner.Hide();
                galleryHost.Add(loaded);
                this.Show();
                invalidateLayout();
            });
        }

        private void clearGallery()
        {
            loadGeneration++;
            clearGalleryContent();

            if (ownsPreviewHandle)
                previewHandle?.Dispose();

            previewHandle = null;
            ownsPreviewHandle = false;
            currentSkin = null;
            invalidateLayout();
        }

        private void clearGalleryContent()
        {
            foreach (var child in galleryHost.ToArray())
            {
                if (child == loadingSpinner || child == galleryHeightSpacer)
                    continue;

                child.Expire();
            }
        }

        private void invalidateLayout()
        {
            Schedule(() =>
            {
                Invalidate(Invalidation.DrawSize);

                for (Drawable? drawable = Parent; drawable != null; drawable = drawable.Parent)
                    drawable.Invalidate(Invalidation.RequiredParentSizeToFit | Invalidation.DrawSize);

                LayoutInvalidated?.Invoke();
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            if (ownsPreviewHandle)
                previewHandle?.Dispose();

            base.Dispose(isDisposing);
        }
    }
}
