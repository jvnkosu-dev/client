// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinEditor.Setup
{
    public partial class SkinMetadataSection : SkinSetupSection
    {
        private FormTextBox nameTextBox = null!;
        private FormTextBox authorTextBox = null!;
        private FormTextBox versionTextBox = null!;
        private FormTextBox descriptionTextBox = null!;
        private FormTextBox tagsTextBox = null!;
        private FormTextBox skinTypeTextBox = null!;

        private bool reloading;
        private bool dirty;

        public override LocalisableString Title => EditorSetupStrings.MetadataHeader;

        private Bindable<Skin> currentSkin = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                nameTextBox = createTextBox("Skin name", "Enter skin name..."),
                authorTextBox = createTextBox("Author", "Enter skin author..."),
                versionTextBox = createTextBox("Version", "1.0", "Populated from skin.ini (SkinVersion); can be changed"),
                descriptionTextBox = createTextBox("Description", "Enter skin description (optional)..."),
                tagsTextBox = createTextBox("Tags", "Enter tags separated by spaces...", "For example: argon minimalist hitcircle"),
                skinTypeTextBox = createTextBox("Skin type", hint: "Detected automatically from the selected skin"),
            };

            skinTypeTextBox.ReadOnly = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentSkin = Skins.CurrentSkin.GetBoundCopy();
            currentSkin.BindValueChanged(_ => reloadFromSkin(), true);

            foreach (var item in new[] { nameTextBox, authorTextBox, versionTextBox, descriptionTextBox, tagsTextBox })
            {
                item.Current.BindValueChanged(_ =>
                {
                    if (reloading)
                        return;

                    dirty = true;
                    Scheduler.AddOnce(persist);
                });
            }
        }

        private static FormTextBox createTextBox(LocalisableString caption, LocalisableString? placeholder = null, LocalisableString? hint = null)
            => new FormTextBox
            {
                RelativeSizeAxes = Axes.X,
                Caption = caption,
                PlaceholderText = placeholder ?? default,
                HintText = hint ?? default,
            };

        private void reloadFromSkin()
        {
            reloading = true;

            var skin = currentSkin.Value;
            var skinInfo = skin.SkinInfo.Value;
            var configuration = skin.Configuration;

            nameTextBox.Current.Value = SkinIniVersionHelper.SanitizeUploadName(skinInfo.Name);
            authorTextBox.Current.Value = !string.IsNullOrWhiteSpace(skinInfo.Creator) && skinInfo.Creator != @"Unknown"
                ? skinInfo.Creator
                : string.Empty;
            versionTextBox.Current.Value = SkinIniVersionHelper.GetSkinVersion(skin, useDefaultIfMissing: true);
            descriptionTextBox.Current.Value = configuration.Description?.Trim() ?? string.Empty;
            tagsTextBox.Current.Value = configuration.Tags?.Trim() ?? string.Empty;
            skinTypeTextBox.Current.Value = !string.IsNullOrWhiteSpace(configuration.SkinType)
                ? configuration.SkinType.Trim()
                : SkinEngineTypeHelper.ToStorageString(SkinEngineTypeHelper.FromSkinInfo(skinInfo));

            dirty = false;
            reloading = false;
        }

        private void persist()
        {
            if (reloading || !dirty)
                return;

            if (Skins.EnsureMutableSkin())
                return;

            var skin = currentSkin.Value;

            Skins.PersistSetupMetadata(
                skin,
                nameTextBox.Current.Value,
                authorTextBox.Current.Value,
                versionTextBox.Current.Value,
                descriptionTextBox.Current.Value,
                tagsTextBox.Current.Value,
                skin.Configuration.ModifiedModes);

            dirty = false;
        }
    }
}
