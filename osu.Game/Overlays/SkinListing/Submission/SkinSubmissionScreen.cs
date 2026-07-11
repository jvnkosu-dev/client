using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Overlays.SkinListing;
using osu.Game.Overlays.SkinSet;
using osu.Game.Overlays;
using osu.Game.Screens;
using osu.Game.Screens.Edit.Submission;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Submission
{
    public partial class SkinSubmissionScreen : OsuScreen
    {
        private readonly Skin skin;

        private SkinSubmissionOverlay overlay = null!;

        [Cached]
        private readonly SkinSubmissionSettings settings = new SkinSubmissionSettings();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        [Resolved]
        private SkinEditorOverlay? skinEditorOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private SkinSetOverlay? skinSetOverlay { get; set; }

        [Resolved]
        private SkinUploader uploader { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private SkinListingOverlay skinListing { get; set; } = null!;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Resolved]
        private SkinLookupCache skinLookupCache { get; set; } = null!;

        [Resolved]
        private OsuConfigManager configManager { get; set; } = null!;

        private Container submissionProgress = null!;
        private SubmissionStageProgress prepareStep = null!;
        private SubmissionStageProgress uploadStep = null!;
        private SubmissionStageProgress finishStep = null!;

        private Sample completedSample = null!;

        private SkinSubmissionCompletionOverlay? completionOverlay;

        private bool uploadStarted;
        private bool openSkinPageOnExit;
        private bool returnToEditorOnExit = true;

        protected override bool InitialBackButtonVisibility => false;

        public SkinSubmissionScreen(Skin skin)
        {
            this.skin = skin;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            // ConfigureFromSkin / overlay creation happen in OnEntering (update thread).
            // BDL may run off-thread and cannot touch managed Realm Live<>.Value.

            AddInternal(submissionProgress = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                AutoSizeDuration = 400,
                AutoSizeEasing = Easing.OutQuint,
                Alpha = 0,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.6f,
                Masking = true,
                CornerRadius = 10,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5,
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(20),
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            prepareStep = new SubmissionStageProgress
                            {
                                StageDescription = SkinSubmissionStrings.Preparing,
                                StageIndex = 0,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                            uploadStep = new SubmissionStageProgress
                            {
                                StageDescription = SkinSubmissionStrings.Uploading,
                                StageIndex = 1,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                            finishStep = new SubmissionStageProgress
                            {
                                StageDescription = SkinSubmissionStrings.Finishing,
                                StageIndex = 2,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                        }
                    }
                }
            });

            completedSample = audio.Samples.Get(@"UI/bss-complete");
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            // Must run on update thread before creating the wizard (steps depend on IsUpdate).
            SkinMetadataHelper.ConfigureFromSkin(settings, skin);
            settings.SourceSkin = skin;

            AddInternal(overlay = new SkinSubmissionOverlay());

            overlay.State.BindValueChanged(_ =>
            {
                if (overlay.State.Value != Visibility.Hidden)
                    return;

                if (!overlay.Completed)
                {
                    allowExit();
                    skinEditorOverlay?.Show();
                    this.Exit();
                    return;
                }

                if (!validate())
                {
                    allowExit();
                    skinEditorOverlay?.Show();
                    this.Exit();
                    return;
                }

                // Wizard finished — show progress UI and start upload (beatmap submission pattern).
                submissionProgress.FadeIn(200, Easing.OutQuint);
                beginUpload();
            });

            overlay.RefreshHeaderCopy();
            overlay.Show();

            Task.Run(async () =>
            {
                try
                {
                    await overlay.ExportSkinToTempAsync(skin).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Schedule(() =>
                    {
                        notifications.Post(new SimpleNotification
                        {
                            Text = $"Failed to prepare skin: {ex.Message}",
                        });
                        overlay.Hide();
                    });
                }
            });
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (!BackButtonVisibility.Value && completionOverlay?.State.Value != Visibility.Visible)
                return true;

            // SkinSet is shown after a successful upload when requested — don't cover it with the editor.
            if (returnToEditorOnExit && !openSkinPageOnExit)
                skinEditorOverlay?.Show();

            return base.OnExiting(e);
        }

        private bool validate()
        {
            if (string.IsNullOrWhiteSpace(settings.Name.Value))
            {
                notifications.Post(new SimpleNotification { Text = "Skin name is empty. Set it in the skin editor Setup tab." });
                return false;
            }

            if (string.IsNullOrWhiteSpace(settings.Author.Value))
            {
                notifications.Post(new SimpleNotification { Text = "Skin author is empty. Set it in the skin editor Setup tab." });
                return false;
            }

            if (string.IsNullOrEmpty(settings.SkinFilePath) || !File.Exists(settings.SkinFilePath))
            {
                notifications.Post(new SimpleNotification { Text = "Failed to prepare the skin file for upload." });
                return false;
            }

            if (!settings.ModifiedModes.Any())
            {
                notifications.Post(new SimpleNotification { Text = "No modified rulesets selected. Set them in the skin editor Setup tab." });
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
            if (uploadStarted)
                return;

            uploadStarted = true;

            string version = string.IsNullOrWhiteSpace(settings.Version.Value)
                ? SkinIniVersionHelper.DEFAULT_VERSION
                : settings.Version.Value.Trim();

            settings.Version.Value = version;

            string name = settings.Name.Value.Trim();
            string author = settings.Author.Value.Trim();
            string uploadName = SkinIniVersionHelper.SanitizeUploadName(name);
            string engineType = settings.EngineType.Value.Trim();
            string modifiedModes = SkinModifiedModesHelper.FormatForUpload(settings.ModifiedModes);

            string skinFilePath = settings.SkinFilePath!;
            int? onlineSkinId = settings.OnlineSkinId.Value > 0 ? settings.OnlineSkinId.Value : null;

            string? previewPath = settings.PreviewFile.Value?.FullName;

            if (string.IsNullOrEmpty(previewPath))
                previewPath = SkinBackgroundHelper.ExportBackgroundToTempFile(skin, ((IStorageResourceProvider)skins).Files);

            var payload = new SkinUploadPayload
            {
                FilePath = skinFilePath,
                PreviewFilePath = previewPath,
                Name = uploadName,
                Version = version,
                Description = settings.Description.Value.Trim(),
                Author = author,
                Tags = settings.Tags.Value.Trim(),
                ModifiedModes = settings.ModifiedModes.ToList(),
                EngineType = engineType,
                OnlineSkinId = onlineSkinId,
            };

            prepareStep.SetInProgress();

            Task.Run(async () =>
            {
                try
                {
                    await Task.Run(() => SkinIniVersionHelper.EnsureSkinMetadataInOsk(
                        skinFilePath, uploadName, author, version, engineType, modifiedModes, onlineSkinId)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Schedule(() => failUpload(prepareStep, ex.Message));
                    return;
                }

                Schedule(() =>
                {
                    prepareStep.SetCompleted();
                    uploadStep.SetInProgress();
                });

                try
                {
                    var result = await uploader.SubmitSkinAsync(
                        payload,
                        api.AccessToken,
                        (current, total) =>
                        {
                            if (total > 0)
                                Schedule(() => uploadStep.SetInProgress((float)current / total));
                        }).ConfigureAwait(false);

                    Schedule(() => finishUpload(result));
                }
                catch (Exception ex)
                {
                    string message = string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message;
                    Schedule(() => failUpload(uploadStep, message));
                }
            });
        }

        private void finishUpload(SkinUploadResult result)
        {
            if (!result.Success)
            {
                failUpload(uploadStep, string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? (settings.IsUpdate.Value ? "Skin update failed." : "Skin upload failed.")
                    : result.ErrorMessage!);
                return;
            }

            uploadStep.SetCompleted();
            finishStep.SetInProgress();

            int? resolvedOnlineSkinId = result.AssignedOnlineSkinId ?? (settings.OnlineSkinId.Value > 0 ? settings.OnlineSkinId.Value : null);

            try
            {
                if (resolvedOnlineSkinId is > 0)
                    persistOnlineSkinId(resolvedOnlineSkinId.Value);

                skinListing.RefreshListing();

                finishStep.SetCompleted();
                completedSample.Play();

                bool openPage = configManager.Get<bool>(OsuSetting.SkinSubmissionOpenPageAfterSubmission)
                                && resolvedOnlineSkinId is > 0;

                if (openPage)
                {
                    allowExit();

                    int skinId = resolvedOnlineSkinId.Value;
                    openSkinPageOnExit = true;
                    returnToEditorOnExit = false;

                    Task.Run(async () =>
                    {
                        await Task.Delay(800).ConfigureAwait(false);
                        Schedule(() =>
                        {
                            skinSetOverlay?.FetchAndShowSkin(skinId);
                            this.Exit();
                        });
                    });
                    return;
                }

                // No auto-open: keep progress on screen and show Back + Done like the wizard footer.
                showCompletionActions();
            }
            catch (Exception ex)
            {
                failUpload(finishStep, ex.Message);
            }
        }

        private void showCompletionActions()
        {
            if (completionOverlay != null)
            {
                completionOverlay.Show();
                return;
            }

            completionOverlay = new SkinSubmissionCompletionOverlay
            {
                RequestDone = exitToMainMenu,
                RequestBackToEditor = returnToEditor,
            };

            AddInternal(completionOverlay);
            completionOverlay.Show();
        }

        private void returnToEditor()
        {
            returnToEditorOnExit = true;
            openSkinPageOnExit = false;
            allowExit();
            completionOverlay?.Hide();
            this.Exit();
        }

        private void exitToMainMenu()
        {
            returnToEditorOnExit = false;
            openSkinPageOnExit = false;
            allowExit();
            completionOverlay?.Hide();
            this.Exit();
        }

        private void failUpload(SubmissionStageProgress step, string message)
        {
            step.SetFailed(message);
            allowExit();
        }

        private void persistOnlineSkinId(int onlineSkinId)
        {
            settings.OnlineSkinId.Value = onlineSkinId;
            settings.IsUpdate.Value = true;

            string engineType = settings.EngineType.Value.Trim();
            if (!string.IsNullOrEmpty(engineType))
                skin.Configuration.SkinType = engineType;

            skins.PersistSetupMetadata(
                skin,
                settings.Name.Value.Trim(),
                settings.Author.Value.Trim(),
                string.IsNullOrWhiteSpace(settings.Version.Value) ? SkinIniVersionHelper.DEFAULT_VERSION : settings.Version.Value.Trim(),
                settings.Description.Value.Trim(),
                settings.Tags.Value.Trim(),
                SkinModifiedModesHelper.FormatForUpload(settings.ModifiedModes));

            skins.LastUploadedOnlineSkinId = onlineSkinId;

            var uploadedAt = DateTimeOffset.UtcNow;
            skins.PersistAfterSuccessfulUpload(skin.SkinInfo, onlineSkinId, uploadedAt);
            skin.Configuration.OnlineSkinId = onlineSkinId;
            skin.Configuration.ServerLastUpdated = SkinUpdateHelper.FormatServerLastUpdated(uploadedAt);

            skinLookupCache.Invalidate(onlineSkinId);

            if (skins.CurrentSkinInfo.Value.ID == skin.SkinInfo.ID)
                skins.CurrentSkin.Value = skin.SkinInfo.PerformRead(s => skins.GetSkin(s));

            Task.Run(async () =>
            {
                APIOnlineSkin? online = null;

                try
                {
                    await Task.Delay(750).ConfigureAwait(false);
                    skinLookupCache.Invalidate(onlineSkinId);
                    online = await skinLookupCache.GetSkinAsync(onlineSkinId).ConfigureAwait(false);
                }
                catch
                {
                    // Optimistic local baseline already applied above.
                }

                if (online == null)
                    return;

                Schedule(() => skins.SyncLocalSkinWithListing(skin.SkinInfo, online, reloadCurrent: true));
            });
        }

        private void allowExit() => BackButtonVisibility.Value = true;
    }
}
