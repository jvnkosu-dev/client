using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using osu.Framework.Graphics;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Game.Online.API;
using osu.Game.Overlays.SkinListing.Submission;

namespace osu.Game.Skinning
{
    public partial class SkinUploader : Component
    {
        private const string skins_api_base = "https://osu.jvnko.boats/api/skins";
        private const string upload_url = skins_api_base + "/upload";

        public Task<SkinUploadResult> UploadSkinAsync(
            SkinUploadPayload payload,
            string? accessToken,
            Action<long, long>? onProgress = null,
            CancellationToken cancellationToken = default)
            => submitSkinAsync(payload, accessToken, HttpMethod.Post, upload_url, onProgress, cancellationToken);

        public Task<SkinUploadResult> UpdateSkinAsync(
            SkinUploadPayload payload,
            int skinId,
            string? accessToken,
            Action<long, long>? onProgress = null,
            CancellationToken cancellationToken = default)
            => submitSkinAsync(payload, accessToken, HttpMethod.Put, $"{skins_api_base}/{skinId}", onProgress, cancellationToken);

        public Task<SkinUploadResult> SubmitSkinAsync(
            SkinUploadPayload payload,
            string? accessToken,
            Action<long, long>? onProgress = null,
            CancellationToken cancellationToken = default)
        {
            if (payload.OnlineSkinId is int skinId && skinId > 0)
                return UpdateSkinAsync(payload, skinId, accessToken, onProgress, cancellationToken);

            return UploadSkinAsync(payload, accessToken, onProgress, cancellationToken);
        }

        private async Task<SkinUploadResult> submitSkinAsync(
            SkinUploadPayload payload,
            string? accessToken,
            HttpMethod method,
            string url,
            Action<long, long>? onProgress,
            CancellationToken cancellationToken)
        {
            if (!File.Exists(payload.FilePath))
                return SkinUploadResult.Failed("Skin file not found.");

            OsuWebRequest? request = null;

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

                request = new OsuWebRequest(url)
                {
                    Method = method,
                    Timeout = 600_000,
                };

                if (!string.IsNullOrEmpty(accessToken))
                    request.AddHeader(@"Authorization", $@"Bearer {accessToken}");

                if (previewBytes != null)
                    request.AddFile("thumbnail", previewBytes, $"thumbnail{previewExtension ?? ".jpg"}");

                request.AddFile("skin", skinBytes, Path.GetFileName(payload.FilePath));
                request.AddParameter("name", payload.Name, RequestParameterType.Form);
                request.AddParameter("creator", payload.Author, RequestParameterType.Form);
                request.AddParameter("description", payload.Description, RequestParameterType.Form);

                if (!string.IsNullOrWhiteSpace(payload.Tags))
                    request.AddParameter("tags", payload.Tags, RequestParameterType.Form);

                if (!string.IsNullOrWhiteSpace(payload.Version))
                    request.AddParameter("version", payload.Version, RequestParameterType.Form);

                if (payload.ModifiedModes.Count > 0)
                    request.AddParameter("modified_modes", SkinModifiedModesHelper.FormatForUpload(payload.ModifiedModes), RequestParameterType.Form);

                if (!string.IsNullOrWhiteSpace(payload.EngineType))
                    request.AddParameter("engine_type", payload.EngineType, RequestParameterType.Form);

                if (onProgress != null)
                {
                    request.UploadProgress += (current, total) =>
                    {
                        const float upload_portion = 0.85f;
                        const float upload_offset = 0.15f;

                        if (total > 0)
                            onProgress((long)((upload_offset + upload_portion * current / (float)total) * 1000), 1000);
                    };
                }

                await request.PerformAsync(cancellationToken).ConfigureAwait(false);

                var status = request.ResponseStatusCode ?? HttpStatusCode.InternalServerError;
                bool success = status >= HttpStatusCode.OK && status < HttpStatusCode.MultipleChoices;

                if (success)
                {
                    int? assignedId = method == HttpMethod.Put
                        ? payload.OnlineSkinId
                        : tryParseUploadedSkinId(request);

                    Logger.Log($"Skin {(method == HttpMethod.Put ? "update" : "upload")} completed successfully.", LoggingTarget.Network);
                    return SkinUploadResult.Completed(assignedId);
                }

                string error = formatServerError(request, status);
                Logger.Log($"Skin {(method == HttpMethod.Put ? "update" : "upload")} failed: {error}", LoggingTarget.Network, LogLevel.Important);
                return SkinUploadResult.Failed(error);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Skin {(method == HttpMethod.Put ? "update" : "upload")} failed with an exception.");

                if (request?.ResponseStatusCode is HttpStatusCode status)
                    return SkinUploadResult.Failed(formatServerError(request, status));

                string message = string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message;
                return SkinUploadResult.Failed(message);
            }
        }

        private static int? tryParseUploadedSkinId(OsuWebRequest request)
        {
            string? body = request.GetResponseString()?.Trim();

            if (string.IsNullOrWhiteSpace(body))
                return null;

            try
            {
                var token = JObject.Parse(body)["id"];

                if (token != null && int.TryParse(token.ToString(), out int id) && id > 0)
                    return id;
            }
            catch
            {
                // Response may be a legacy plain-text success message.
            }

            return null;
        }

        private static string formatServerError(OsuWebRequest request, HttpStatusCode status)
        {
            string body = request.GetResponseString()?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(body))
            {
                const int max_length = 300;
                if (body.Length > max_length)
                    body = body[..max_length] + "...";

                return $"HTTP {(int)status}: {body}";
            }

            return $"HTTP {(int)status} {status}";
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

            onProgress((long)(bytesRead / (float)totalBytes * 150), 1000);
        }
    }
}
