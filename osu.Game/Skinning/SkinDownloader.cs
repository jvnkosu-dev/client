using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.IO.Network;
using osu.Framework.Platform;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Skinning
{
    public partial class SkinDownloader : Component
    {
        [Resolved]
        private SkinManager skinManager { get; set; } = null!;

        [Resolved]
        private Storage storage { get; set; } = null!;

        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        public bool IsInstalled(APIOnlineSkin onlineSkin)
        {
            var installedSkins = skinManager.GetAllUsableSkins();
            return installedSkins.Any(s =>
                string.Equals(s.Value.Name, onlineSkin.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(s.Value.Creator, onlineSkin.Creator, StringComparison.OrdinalIgnoreCase));
        }

        public void DownloadAndImport(APIOnlineSkin onlineSkin)
        {
            if (IsInstalled(onlineSkin))
            {
                notifications.Post(new SimpleNotification
                {
                    Text = $"Скин '{onlineSkin.Name}' уже установлен!"
                });
                return;
            }

            string tempFileName = $"{onlineSkin.OnlineID}_{Guid.NewGuid()}.osk";
            string tempPath = Path.Combine(storage.GetFullPath("temp"), tempFileName);

            var notification = new ProgressNotification
            {
                Text = $"Скачивание скина {onlineSkin.Name}...",
            };

            var request = new FileWebRequest(tempPath, onlineSkin.DownloadUrl);

            request.DownloadProgress += (current, total) =>
            {
                notification.Progress = (float)current / total;
            };

            request.Finished += () =>
            {
                notification.Progress = 1;
                notification.Text = $"Импорт скина {onlineSkin.Name}...";

                Task.Run(async () =>
                {
                    try
                    {
                        await skinManager.Import(tempPath).ConfigureAwait(false);
                        notification.State = ProgressNotificationState.Completed;
                        notification.Text = $"Скин {onlineSkin.Name} успешно установлен!";
                        // В идеале тут нужно вызвать событие, чтобы карточки обновили свой статус "Установлено"
                    }
                    catch (Exception ex)
                    {
                        notification.State = ProgressNotificationState.Cancelled;
                        notification.Text = $"Ошибка импорта {onlineSkin.Name}: {ex.Message}";
                    }
                    finally
                    {
                        if (File.Exists(tempPath))
                            File.Delete(tempPath);
                    }
                });
            };

            request.Failed += (exception) =>
            {
                notification.State = ProgressNotificationState.Cancelled;
                notification.Text = $"Ошибка скачивания {onlineSkin.Name}: {exception.Message}";
            };

            notifications.Post(notification);
            request.PerformAsync();
        }
    }
}
