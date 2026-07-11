// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Skinning
{
    public partial class SkinDownloader : Component
    {
        public event Action<APIOnlineSkin, FileWebRequest>? DownloadBegan;
        public event Action<APIOnlineSkin>? DownloadCompleted;
        public event Action<APIOnlineSkin>? DownloadFailed;

        private readonly Dictionary<int, FileWebRequest> activeRequests = new Dictionary<int, FileWebRequest>();
        private readonly Dictionary<int, Live<SkinInfo>> installedOnlineSkins = new Dictionary<int, Live<SkinInfo>>();

        [Resolved]
        private SkinManager skinManager { get; set; } = null!;

        [Resolved]
        private Storage storage { get; set; } = null!;

        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        [Resolved]
        private OsuGame game { get; set; } = null!;

        public bool IsInstalled(APIOnlineSkin onlineSkin) => GetInstalledSkin(onlineSkin) != null;

        public Live<SkinInfo>? GetInstalledSkin(APIOnlineSkin onlineSkin)
        {
            if (onlineSkin.OnlineID > 0 && installedOnlineSkins.TryGetValue(onlineSkin.OnlineID, out var cached))
            {
                if (skinManager.GetAllUsableSkins().Any(s => s.ID == cached.ID))
                    return cached;

                installedOnlineSkins.Remove(onlineSkin.OnlineID);
            }

            return skinManager.GetAllUsableSkins().FirstOrDefault(s => s.PerformRead(info => matchesOnlineSkin(info, onlineSkin)));
        }

        public FileWebRequest? GetActiveRequest(APIOnlineSkin onlineSkin) =>
            activeRequests.TryGetValue(onlineSkin.OnlineID, out var request) ? request : null;

        public void DownloadAndImport(APIOnlineSkin onlineSkin)
        {
            if (IsInstalled(onlineSkin))
            {
                notifications.Post(new SimpleNotification
                {
                    Text = $"Skin '{onlineSkin.Name}' is already installed!"
                });
                return;
            }

            beginDownload(onlineSkin, existing: null);
        }

        public void DownloadAndUpdate(APIOnlineSkin onlineSkin, Live<SkinInfo> existing)
        {
            if (activeRequests.ContainsKey(onlineSkin.OnlineID))
                return;

            beginDownload(onlineSkin, existing);
        }

        private void beginDownload(APIOnlineSkin onlineSkin, Live<SkinInfo>? existing)
        {
            if (activeRequests.ContainsKey(onlineSkin.OnlineID))
                return;

            bool isUpdate = existing != null;
            string tempFileName = $"{onlineSkin.OnlineID}_{Guid.NewGuid()}.osk";
            string tempPath = Path.Combine(storage.GetFullPath("temp"), tempFileName);

            var notification = new ProgressNotification
            {
                Text = isUpdate
                    ? $"Downloading update for {onlineSkin.Name}..."
                    : $"Downloading skin {onlineSkin.Name}...",
            };

            var request = new FileWebRequest(tempPath, onlineSkin.DownloadUrl);
            activeRequests[onlineSkin.OnlineID] = request;

            request.DownloadProgress += (current, total) =>
            {
                notification.Progress = total > 0 ? (float)current / total : 0;
            };

            request.Finished += () =>
            {
                notification.Progress = 1;
                notification.Text = isUpdate
                    ? $"Updating skin {onlineSkin.Name}..."
                    : $"Importing skin {onlineSkin.Name}...";

                // Capture ID on this callback thread; Live.Value must not be touched from Task.Run.
                Guid? existingId = existing?.ID;
                long contentLength = new FileInfo(tempPath).Exists ? new FileInfo(tempPath).Length : 0;

                Task.Run(async () =>
                {
                    string? extractDir = null;

                    try
                    {
                        if (isUpdate)
                        {
                            extractDir = Path.Combine(storage.GetFullPath("temp"), $"skin-update-{onlineSkin.OnlineID}-{Guid.NewGuid()}");
                            Directory.CreateDirectory(extractDir);
                            ZipFile.ExtractToDirectory(tempPath, extractDir);

                            var original = new SkinInfo { ID = existingId!.Value };
                            await skinManager.ImportAsUpdate(notification, new ImportTask(extractDir), original).ConfigureAwait(false);
                        }
                        else
                        {
                            await skinManager.Import(new ImportTask(tempPath), new ImportParameters
                            {
                                OnlineSkinListingName = onlineSkin.Name,
                                OnlineSkinListingCreator = onlineSkin.Creator,
                            }).ConfigureAwait(false);
                        }

                        notification.CompletionClickAction = () =>
                        {
                            var installed = GetInstalledSkin(onlineSkin);

                            if (installed != null)
                                game.PresentSkin(installed.Value);

                            return true;
                        };
                        notification.CompletionText = isUpdate
                            ? $"Skin {onlineSkin.Name} updated successfully!"
                            : $"Skin {onlineSkin.Name} installed successfully!";
                        notification.State = ProgressNotificationState.Completed;

                        Schedule(() =>
                        {
                            activeRequests.Remove(onlineSkin.OnlineID);

                            var imported = existing ?? GetInstalledSkin(onlineSkin);

                            if (imported != null && onlineSkin.OnlineID > 0)
                            {
                                installedOnlineSkins[onlineSkin.OnlineID] = imported;
                                skinManager.PersistServerSnapshot(imported, onlineSkin, contentLength > 0 ? contentLength : null);

                                // Reload current skin instance so Configuration / files refresh.
                                if (skinManager.CurrentSkinInfo.Value.ID == imported.ID)
                                {
                                    var refreshed = imported.PerformRead(s => s.CreateInstance(skinManager));
                                    skinManager.CurrentSkin.Value = refreshed;
                                }
                            }

                            DownloadCompleted?.Invoke(onlineSkin);
                        });
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, isUpdate
                            ? $"Failed to update skin '{onlineSkin.Name}'"
                            : $"Failed to import skin '{onlineSkin.Name}'");

                        notification.State = ProgressNotificationState.Cancelled;
                        notification.Text = isUpdate
                            ? $"Failed to update {onlineSkin.Name}: {ex.Message}"
                            : $"Failed to import {onlineSkin.Name}: {ex.Message}";
                        Schedule(() =>
                        {
                            activeRequests.Remove(onlineSkin.OnlineID);
                            DownloadFailed?.Invoke(onlineSkin);
                        });
                    }
                    finally
                    {
                        if (File.Exists(tempPath))
                            File.Delete(tempPath);

                        if (extractDir != null && Directory.Exists(extractDir))
                        {
                            try
                            {
                                Directory.Delete(extractDir, true);
                            }
                            catch
                            {
                                // Best-effort cleanup.
                            }
                        }
                    }
                });
            };

            request.Failed += _ =>
            {
                notification.State = ProgressNotificationState.Cancelled;
                notification.Text = isUpdate
                    ? $"Failed to download update for {onlineSkin.Name}"
                    : $"Failed to download {onlineSkin.Name}";
                activeRequests.Remove(onlineSkin.OnlineID);
                DownloadFailed?.Invoke(onlineSkin);
            };

            notifications.Post(notification);
            DownloadBegan?.Invoke(onlineSkin, request);
            request.PerformAsync();
        }

        internal static bool matchesOnlineSkin(SkinInfo skinInfo, APIOnlineSkin onlineSkin)
        {
            if (!namesMatch(skinInfo.Name, onlineSkin.Name))
                return false;

            if (string.IsNullOrEmpty(onlineSkin.Creator))
                return true;

            return string.Equals(skinInfo.Creator, onlineSkin.Creator, StringComparison.OrdinalIgnoreCase);
        }

        private static bool namesMatch(string installedName, string onlineName)
        {
            string sanitizedOnlineName = SkinIniVersionHelper.SanitizeUploadName(onlineName);

            if (string.Equals(installedName, sanitizedOnlineName, StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(installedName, onlineName, StringComparison.OrdinalIgnoreCase))
                return true;

            // SkinImporter appends " [archiveName]" when the archive filename differs from the skin name (drag-and-drop imports only).
            return installedName.StartsWith(sanitizedOnlineName + " [", StringComparison.OrdinalIgnoreCase)
                   || installedName.StartsWith(onlineName + " [", StringComparison.OrdinalIgnoreCase);
        }
    }
}
