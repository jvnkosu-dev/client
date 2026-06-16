using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.IO.Network;
using osu.Game.Online.API;
using osu.Game.Overlays.SkinListing.Submission;

namespace osu.Game.Skinning
{
    public partial class SkinUploader : Component
    {
        private readonly string uploadUrl = "https://osu.jvnko.boats/api/skins/upload";

        public async Task<bool> UploadSkinAsync(SkinUploadPayload payload, Action<long, long>? onProgress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(payload.FilePath))
            {
                System.Diagnostics.Debug.WriteLine("[SkinUploader] Файл скина не найден!");
                return false;
            }

            try
            {
                long totalBytes = new FileInfo(payload.FilePath).Length;

                if (!string.IsNullOrEmpty(payload.PreviewFilePath) && File.Exists(payload.PreviewFilePath))
                    totalBytes += new FileInfo(payload.PreviewFilePath).Length;

                byte[] skinBytes = await readFileWithProgressAsync(payload.FilePath, totalBytes, onProgress, cancellationToken).ConfigureAwait(false);

                byte[]? previewBytes = null;
                string? previewExtension = null;

                if (!string.IsNullOrEmpty(payload.PreviewFilePath) && File.Exists(payload.PreviewFilePath))
                {
                    previewBytes = await readFileWithProgressAsync(payload.PreviewFilePath, totalBytes, onProgress, cancellationToken).ConfigureAwait(false);
                    previewExtension = Path.GetExtension(payload.PreviewFilePath);
                }

                var request = new OsuWebRequest(uploadUrl)
                {
                    Method = HttpMethod.Post,
                    Timeout = 600_000,
                };

                if (previewBytes != null)
                    request.AddFile("thumbnail", previewBytes, $"thumbnail{previewExtension ?? ".jpg"}");

                request.AddFile("skin", skinBytes, Path.GetFileName(payload.FilePath));
                request.AddParameter("name", payload.Name, RequestParameterType.Form);
                request.AddParameter("creator", payload.Author, RequestParameterType.Form);
                request.AddParameter("description", payload.Description, RequestParameterType.Form);

                if (onProgress != null)
                {
                    request.UploadProgress += (current, total) =>
                    {
                        // Reserve the first 15% of the bar for reading files from disk.
                        const float upload_portion = 0.85f;
                        const float upload_offset = 0.15f;

                        if (total > 0)
                            onProgress((long)((upload_offset + upload_portion * current / (float)total) * 1000), 1000);
                    };
                }

                await request.PerformAsync().ConfigureAwait(false);

                var status = request.ResponseStatusCode;
                bool success = status >= HttpStatusCode.OK && status < HttpStatusCode.MultipleChoices;

                if (success)
                    System.Diagnostics.Debug.WriteLine("[SkinUploader] Скин успешно загружен!");
                else
                    System.Diagnostics.Debug.WriteLine($"[SkinUploader] Ошибка сервера: {status}");

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SkinUploader] Критическая ошибка: {ex.Message}");
                return false;
            }
        }

        private static async Task<byte[]> readFileWithProgressAsync(string path, long totalBytes, Action<long, long>? onProgress, CancellationToken cancellationToken)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);

            try
            {
                long fileSize = stream.Length;

                if (fileSize == 0)
                    return [];

                if (fileSize <= int.MaxValue && totalBytes <= int.MaxValue)
                {
                    var data = new byte[fileSize];
                    int offset = 0;

                    while (offset < data.Length)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        int read = await stream.ReadAsync(data.AsMemory(offset, data.Length - offset), cancellationToken).ConfigureAwait(false);

                        if (read == 0)
                            break;

                        offset += read;
                        reportReadProgress(offset, fileSize, totalBytes, onProgress);
                    }

                    return data;
                }

                using var ms = new MemoryStream();
                var buffer = new byte[81920];
                long readBytes = 0;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    int read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

                    if (read == 0)
                        break;

                    await ms.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                    readBytes += read;
                    reportReadProgress(readBytes, fileSize, totalBytes, onProgress);
                }

                return ms.ToArray();
            }
            finally
            {
                await stream.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static void reportReadProgress(long bytesRead, long fileSize, long totalBytes, Action<long, long>? onProgress)
        {
            if (onProgress == null || totalBytes <= 0)
                return;

            // Reading from disk fills up to 15% of the progress bar.
            onProgress((long)(bytesRead / (float)totalBytes * 150), 1000);
        }
    }
}
