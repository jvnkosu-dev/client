// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Skinning;
using osu.Game.Utils;

namespace osu.Game.Overlays.SkinEditor.Setup
{
    public partial class SkinResourcesSection : SkinSetupSection
    {
        public override LocalisableString Title => EditorSetupStrings.ResourcesHeader;

        private FormFileSelector backgroundChooser = null!;
        private SkinSetupPreviewImage backgroundPreview = null!;

        private Bindable<Skin> currentSkin = null!;
        private bool reloading;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IRenderer renderer { get; set; } = null!;

        [Resolved]
        private SkinEditor skinEditor { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            backgroundPreview = new SkinSetupPreviewImage
            {
                RelativeSizeAxes = Axes.X,
                Height = 110,
            };

            Children = new Drawable[]
            {
                backgroundChooser = new FormFileSelector(SupportedExtensions.IMAGE_EXTENSIONS)
                {
                    RelativeSizeAxes = Axes.X,
                    Caption = GameplaySettingsStrings.BackgroundHeader,
                    PlaceholderText = EditorSetupStrings.ClickToSelectBackground,
                    HintText = "Saved inside the skin as bg.* (original format kept)",
                    AllowClear = true,
                    Current = { BindTarget = backgroundPreview.PreviewFile },
                },
            };

            backgroundChooser.PreviewContainer.Add(backgroundPreview);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentSkin = Skins.CurrentSkin.GetBoundCopy();
            currentSkin.BindValueChanged(_ => reloadFromSkin(), true);

            backgroundChooser.Current.BindValueChanged(backgroundChanged);
            skinEditor.SkinBackgroundChanged += reloadFromSkin;
        }

        protected override void Dispose(bool isDisposing)
        {
            skinEditor.SkinBackgroundChanged -= reloadFromSkin;
            base.Dispose(isDisposing);
        }

        private void reloadFromSkin()
        {
            reloading = true;

            var skin = currentSkin.Value;
            bool hasBackground = skin.SkinInfo.PerformRead(s => SkinIniVersionHelper.FindBackgroundFile(s) != null);

            backgroundChooser.Current.Value = null;

            if (hasBackground)
                backgroundPreview.ShowSkinBackground(skin, renderer, Colours, Skins);

            reloading = false;
        }

        private void backgroundChanged(ValueChangedEvent<FileInfo?> file)
        {
            if (reloading)
                return;

            if (Skins.EnsureMutableSkin())
                return;

            var skin = currentSkin.Value;

            if (file.NewValue == null)
            {
                skin.SkinInfo.PerformWrite(skinInfo => Skins.ClearSkinBackground(skinInfo));
                realm.Run(r => r.Refresh());
                reloadFromSkin();
                return;
            }

            if (!file.NewValue.Exists)
                return;

            skin.SkinInfo.PerformWrite(skinInfo =>
            {
                using (var contents = file.NewValue.OpenRead())
                    Skins.SetSkinBackground(skinInfo, contents, file.NewValue.Extension);
            });

            realm.Run(r => r.Refresh());

            // Force preview from skin storage (not only the picked FileInfo), so it stays after chooser clears.
            reloading = true;
            backgroundChooser.Current.Value = null;
            reloading = false;
            backgroundPreview.ShowSkinBackground(skin, renderer, Colours, Skins);
        }
    }
}
