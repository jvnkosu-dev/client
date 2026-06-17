using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Submission
{
    public partial class ScreenSkinMetadata : WizardScreen
    {
        [Resolved]
        private SkinSubmissionSettings settings { get; set; } = null!;

        public override LocalisableString? NextStepText
        {
            get
            {
                // Settings may not be resolved yet when the footer first queries this during screen push.
                if (settings == null)
                    return null;

                return settings.IsUpdate.Value ? SkinMetadataHelper.UpdateUploadActionText : SkinMetadataHelper.UploadActionText;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var previewImage = new SkinSubmissionPreviewImage
            {
                RelativeSizeAxes = Axes.X,
                Height = 110,
            };

            var modifiedModesControl = new SkinSubmissionModifiedModesControl();
            modifiedModesControl.ModifiedModes.BindTo(settings.ModifiedModes);

            FormFileSelector previewSelector;

            Content.Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    new FormTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Skin name",
                        PlaceholderText = "Enter skin name...",
                        Current = { BindTarget = settings.Name },
                    },
                    new FormTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Author",
                        PlaceholderText = "Enter skin author...",
                        Current = { BindTarget = settings.Author },
                    },
                    new FormTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Version",
                        PlaceholderText = "1.0",
                        HintText = "Populated from skin.ini (SkinVersion); can be changed",
                        Current = { BindTarget = settings.Version },
                    },
                    new FormTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Description",
                        PlaceholderText = "Enter skin description (optional)...",
                        Current = { BindTarget = settings.Description },
                    },
                    new FormTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Tags",
                        PlaceholderText = "Enter tags separated by spaces...",
                        HintText = "For example: argon minimalist hitcircle",
                        Current = { BindTarget = settings.Tags },
                    },
                    new FormTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Skin type",
                        HintText = "Detected automatically from the selected skin",
                        ReadOnly = true,
                        Current = { BindTarget = settings.EngineType },
                    },
                    modifiedModesControl,
                    previewSelector = new FormFileSelector(SupportedExtensions.IMAGE_EXTENSIONS)
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Preview",
                        PlaceholderText = "Select a preview image...",
                        HintText = "Shown on the skin card in the listing (optional)",
                        AllowClear = true,
                        Current = { BindTarget = settings.PreviewFile },
                    },
                }
            });

            previewSelector.PreviewContainer.Add(previewImage);
            previewImage.PreviewFile.BindTarget = settings.PreviewFile;
        }
    }
}
