using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Submission
{
    public partial class ScreenSkinMetadata : WizardScreen
    {
        public override LocalisableString? NextStepText => "Загрузить на сервер";

        [Resolved]
        private SkinSubmissionSettings settings { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            var previewImage = new SkinSubmissionPreviewImage
            {
                RelativeSizeAxes = Axes.X,
                Height = 110,
            };

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
                        Caption = "Название скина",
                        PlaceholderText = "Введите название скина...",
                        Current = { BindTarget = settings.Name },
                    },
                    new FormTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Автор",
                        PlaceholderText = "Введите имя автора скина...",
                        Current = { BindTarget = settings.Author },
                    },
                    new FormTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Версия",
                        PlaceholderText = "1.0",
                        HintText = "Подставляется из skin.ini (SkinVersion), можно изменить",
                        Current = { BindTarget = settings.Version },
                    },
                    new FormTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Описание",
                        PlaceholderText = "Введите описание скина (необязательно)...",
                        Current = { BindTarget = settings.Description },
                    },
                    previewSelector = new FormFileSelector(SupportedExtensions.IMAGE_EXTENSIONS)
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = "Превью",
                        PlaceholderText = "Выберите изображение превью...",
                        HintText = "Отображается на карточке скина в каталоге (необязательно)",
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
