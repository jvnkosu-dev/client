using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.IO.Network;
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

            if (activeRequests.ContainsKey(onlineSkin.OnlineID))
                return;

            string tempFileName = $"{onlineSkin.OnlineID}_{Guid.NewGuid()}.osk";
            string tempPath = Path.Combine(storage.GetFullPath("temp"), tempFileName);

            var notification = new ProgressNotification
            {
                Text = $"Downloading skin {onlineSkin.Name}...",
            };

            var request = new FileWebRequest(tempPath, onlineSkin.DownloadUrl);
            activeRequests[onlineSkin.OnlineID] = request;

            request.DownloadProgress += (current, total) =>
            {
                notification.Progress = (float)current / total;
            };

            request.Finished += () =>
            {
                notification.Progress = 1;
                notification.Text = $"Importing skin {onlineSkin.Name}...";

                Task.Run(async () =>
                {
                    try
                    {
                        await skinManager.Import(new ImportTask(tempPath), new ImportParameters
                        {
                            OnlineSkinListingName = onlineSkin.Name,
                            OnlineSkinListingCreator = onlineSkin.Creator,
                        }).ConfigureAwait(false);

                        notification.CompletionClickAction = () =>
                        {
                            var installed = GetInstalledSkin(onlineSkin);

                            if (installed != null)
                                game.PresentSkin(installed.Value);

                            return true;
                        };
                        notification.CompletionText = $"Skin {onlineSkin.Name} installed successfully!";
                        notification.State = ProgressNotificationState.Completed;
                        Schedule(() =>
                        {
                            activeRequests.Remove(onlineSkin.OnlineID);

                            var imported = GetInstalledSkin(onlineSkin);

                            if (imported != null && onlineSkin.OnlineID > 0)
                            {
                                installedOnlineSkins[onlineSkin.OnlineID] = imported;
                                skinManager.PersistOnlineSkinId(imported, onlineSkin.OnlineID);
                            }

                            DownloadCompleted?.Invoke(onlineSkin);
                        });
                    }
                    catch (Exception ex)
                    {
                        notification.State = ProgressNotificationState.Cancelled;
                        notification.Text = $"Failed to import {onlineSkin.Name}: {ex.Message}";
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
                    }
                });
            };

            request.Failed += _ =>
            {
                notification.State = ProgressNotificationState.Cancelled;
                notification.Text = $"Failed to download {onlineSkin.Name}";
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
