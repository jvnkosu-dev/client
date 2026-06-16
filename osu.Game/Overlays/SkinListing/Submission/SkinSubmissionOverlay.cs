using System;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinListing.Submission
{
    public partial class SkinSubmissionOverlay : WizardOverlay
    {
        [Resolved]
        private SkinSubmissionSettings settings { get; set; } = null!;

        public SkinSubmissionSettings Settings => settings;

        [Resolved]
        private Storage storage { get; set; } = null!;

        [Resolved]
        private SkinUploader uploader { get; set; } = null!;

        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        private bool uploadInProgress;

        public SkinSubmissionOverlay()
            : base(OverlayColourScheme.Aquamarine)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep<ScreenSkinMetadata>();

            Header.Title = "Выгрузка скина";
            Header.Description = "Заполните метаданные перед отправкой на сервер";
        }

        public void PopulateMetadataFromSkin(Skin skin) => SkinMetadataHelper.PopulateSettingsFromSkin(settings, skin);

        public async Task ExportSkinToTempAsync(Skin skin)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"skin-upload-{Guid.NewGuid()}.osk");

            using (var stream = File.Create(tempPath))
            {
                var exporter = new LegacySkinExporter(storage);
                await exporter.ExportToStreamAsync(skin.SkinInfo, stream).ConfigureAwait(false);
            }

            settings.SkinFilePath = tempPath;
        }

        protected override void ShowNextStep()
        {
            if (CurrentStepIndex == 0)
            {
                if (uploadInProgress || !validate())
                    return;

                beginUpload();
                return;
            }

            base.ShowNextStep();
        }

        private bool validate()
        {
            if (string.IsNullOrWhiteSpace(settings.Name.Value))
            {
                notifications.Post(new SimpleNotification { Text = "Укажите название скина." });
                return false;
            }

            if (string.IsNullOrWhiteSpace(settings.Author.Value))
            {
                notifications.Post(new SimpleNotification { Text = "Укажите автора скина." });
                return false;
            }

            if (string.IsNullOrEmpty(settings.SkinFilePath) || !File.Exists(settings.SkinFilePath))
            {
                notifications.Post(new SimpleNotification { Text = "Не удалось подготовить файл скина для выгрузки." });
                return false;
            }

            return true;
        }

        private void beginUpload()
        {
            string version = string.IsNullOrWhiteSpace(settings.Version.Value)
                ? SkinIniVersionHelper.DEFAULT_VERSION
                : settings.Version.Value.Trim();

            settings.Version.Value = version;

            string name = settings.Name.Value.Trim();
            string author = settings.Author.Value.Trim();
            string uploadName = SkinIniVersionHelper.FormatUploadName(name, version);

            if (string.IsNullOrEmpty(settings.SkinFilePath))
                return;

            string skinFilePath = settings.SkinFilePath;

            try
            {
                SkinIniVersionHelper.EnsureSkinMetadataInOsk(skinFilePath, uploadName, author, version);
            }
            catch (Exception ex)
            {
                notifications.Post(new SimpleNotification { Text = $"Не удалось обновить skin.ini: {ex.Message}" });
                return;
            }

            var payload = new SkinUploadPayload
            {
                FilePath = skinFilePath,
                PreviewFilePath = settings.PreviewFile.Value?.FullName,
                Name = uploadName,
                Version = version,
                Description = settings.Description.Value.Trim(),
                Author = author,
            };

            uploadInProgress = true;
            NextButton!.Enabled.Value = false;

            var notification = new ProgressNotification { Text = "Загрузка скина на сервер..." };
            notifications.Post(notification);

            Task.Run(async () =>
            {
                bool success = await uploader.UploadSkinAsync(payload).ConfigureAwait(false);

                Schedule(() =>
                {
                    uploadInProgress = false;
                    NextButton!.Enabled.Value = true;

                    if (success)
                    {
                        notification.State = ProgressNotificationState.Completed;
                        notification.Text = "Скин успешно загружен!";
                        base.ShowNextStep();
                    }
                    else
                    {
                        notification.State = ProgressNotificationState.Cancelled;
                        notification.Text = "Ошибка при загрузке скина.";
                    }
                });
            });
        }
    }
}
