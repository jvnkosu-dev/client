using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.SkinListing;
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

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private SkinListingOverlay skinListing { get; set; } = null!;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        public bool IsUpdate => settings.IsUpdate.Value;

        private Skin? submissionSkin;
        private bool uploadInProgress;

        public SkinSubmissionOverlay()
            : base(OverlayColourScheme.Aquamarine)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep<ScreenSkinMetadata>();
            RefreshHeaderCopy();
        }

        public void SetSubmissionSkin(Skin skin) => submissionSkin = skin;

        public void RefreshHeaderCopy()
        {
            if (IsUpdate)
            {
                Header.Title = SkinMetadataHelper.UpdateUploadActionText;
                Header.Description = "Update metadata and files for your existing skin on the server";
            }
            else
            {
                Header.Title = "Skin upload";
                Header.Description = "Fill in metadata before submitting to the server";
            }
        }

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
                notifications.Post(new SimpleNotification { Text = "Please enter a skin name." });
                return false;
            }

            if (string.IsNullOrWhiteSpace(settings.Author.Value))
            {
                notifications.Post(new SimpleNotification { Text = "Please enter the skin author." });
                return false;
            }

            if (string.IsNullOrEmpty(settings.SkinFilePath) || !File.Exists(settings.SkinFilePath))
            {
                notifications.Post(new SimpleNotification { Text = "Failed to prepare the skin file for upload." });
                return false;
            }

            if (!settings.ModifiedModes.Any())
            {
                notifications.Post(new SimpleNotification { Text = "Please select at least one modified ruleset." });
                return false;
            }

            if (api.LocalUser.Value is GuestUser)
            {
                notifications.Post(new SimpleNotification { Text = "Please log in to upload a skin." });
                return false;
            }

            if (string.IsNullOrEmpty(api.AccessToken))
            {
                notifications.Post(new SimpleNotification { Text = "Failed to obtain an auth token. Please log in again." });
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
            string uploadName = SkinIniVersionHelper.SanitizeUploadName(name);
            string engineType = settings.EngineType.Value.Trim();
            string modifiedModes = SkinModifiedModesHelper.FormatForUpload(settings.ModifiedModes);

            if (string.IsNullOrEmpty(settings.SkinFilePath))
                return;

            string skinFilePath = settings.SkinFilePath;
            int? onlineSkinId = settings.OnlineSkinId.Value > 0 ? settings.OnlineSkinId.Value : null;

            var payload = new SkinUploadPayload
            {
                FilePath = skinFilePath,
                PreviewFilePath = settings.PreviewFile.Value?.FullName,
                Name = uploadName,
                Version = version,
                Description = settings.Description.Value.Trim(),
                Author = author,
                Tags = settings.Tags.Value.Trim(),
                ModifiedModes = settings.ModifiedModes.ToList(),
                EngineType = settings.EngineType.Value.Trim(),
                OnlineSkinId = onlineSkinId,
            };

            uploadInProgress = true;
            NextButton!.Enabled.Value = false;

            var notification = new ProgressNotification
            {
                Text = IsUpdate ? "Preparing skin for update..." : "Preparing skin for upload...",
                State = ProgressNotificationState.Active,
                IsImportant = true,
            };
            notifications.Post(notification);

            Task.Run(async () =>
            {
                try
                {
                    try
                    {
                        await Task.Run(() => SkinIniVersionHelper.EnsureSkinMetadataInOsk(skinFilePath, uploadName, author, version, engineType, modifiedModes, onlineSkinId), notification.CancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Schedule(() => finishUpload(notification, SkinUploadResult.Failed($"Failed to update skin.ini: {ex.Message}")));
                        return;
                    }

                    Schedule(() => notification.Text = IsUpdate ? "Updating skin on server..." : "Uploading skin to server...");

                    var result = await uploader.SubmitSkinAsync(
                        payload,
                        api.AccessToken,
                        (current, total) =>
                        {
                            if (total > 0)
                                notification.Progress = (float)current / total;
                        },
                        notification.CancellationToken).ConfigureAwait(false);

                    Schedule(() => finishUpload(notification, result));
                }
                catch (OperationCanceledException)
                {
                    Schedule(() =>
                    {
                        if (notification.State == ProgressNotificationState.Active)
                        {
                            notification.State = ProgressNotificationState.Cancelled;
                            notification.Text = "Skin upload cancelled.";
                        }

                        resetUploadState();
                    });
                }
                catch (Exception ex)
                {
                    string message = string.IsNullOrWhiteSpace(ex.Message)
                        ? ex.GetType().Name
                        : ex.Message;

                    Schedule(() => finishUpload(notification, SkinUploadResult.Failed(message)));
                }
            });
        }

        private void finishUpload(ProgressNotification notification, SkinUploadResult result)
        {
            resetUploadState();

            if (result.Success)
            {
                int? resolvedOnlineSkinId = result.AssignedOnlineSkinId ?? (settings.OnlineSkinId.Value > 0 ? settings.OnlineSkinId.Value : null);

                if (resolvedOnlineSkinId is > 0)
                    persistOnlineSkinId(resolvedOnlineSkinId.Value);

                notification.Progress = 1;
                notification.CompletionText = IsUpdate ? "Skin updated successfully!" : "Skin uploaded successfully!";
                notification.State = ProgressNotificationState.Completed;
                skinListing.RefreshListing();

                base.ShowNextStep();
                return;
            }

            notification.Text = string.IsNullOrWhiteSpace(result.ErrorMessage)
                ? IsUpdate ? "Skin update failed." : "Skin upload failed."
                : IsUpdate ? $"Skin update failed: {result.ErrorMessage}" : $"Skin upload failed: {result.ErrorMessage}";
            notification.State = ProgressNotificationState.Cancelled;
        }

        private void persistOnlineSkinId(int onlineSkinId)
        {
            settings.OnlineSkinId.Value = onlineSkinId;
            settings.IsUpdate.Value = true;

            if (submissionSkin == null)
                return;

            submissionSkin.Configuration.OnlineSkinId = onlineSkinId;
            skins.PersistOnlineSkinId(submissionSkin.SkinInfo, onlineSkinId);
        }

        private void resetUploadState()
        {
            uploadInProgress = false;
            NextButton!.Enabled.Value = true;
        }
    }
}
