using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.SkinListing.Submission;

namespace osu.Game.Skinning
{
    public partial class SkinUploader : Component
    {
        private readonly string uploadUrl = "https://osu.jvnko.boats/api/skins/upload";

        public async Task<bool> UploadSkinAsync(SkinUploadPayload payload)
        {
            if (!File.Exists(payload.FilePath))
            {
                System.Diagnostics.Debug.WriteLine("[SkinUploader] Файл скина не найден!");
                return false;
            }

            try
            {
                byte[] skinBytes = await File.ReadAllBytesAsync(payload.FilePath).ConfigureAwait(false);
                byte[]? previewBytes = null;
                string? previewExtension = null;

                if (!string.IsNullOrEmpty(payload.PreviewFilePath) && File.Exists(payload.PreviewFilePath))
                {
                    previewBytes = await File.ReadAllBytesAsync(payload.PreviewFilePath).ConfigureAwait(false);
                    previewExtension = Path.GetExtension(payload.PreviewFilePath);
                }

                using var client = new HttpClient();
                using var content = new MultipartFormDataContent();

                if (previewBytes != null)
                {
                    var previewContent = new ByteArrayContent(previewBytes);
                    previewContent.Headers.ContentType = new MediaTypeHeaderValue(getImageMimeType(previewExtension));
                    content.Add(previewContent, "thumbnail", $"thumbnail{previewExtension ?? ".jpg"}");
                }

                var fileContent = new ByteArrayContent(skinBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Add(fileContent, "skin", Path.GetFileName(payload.FilePath));

                content.Add(new StringContent(payload.Name), "name");
                content.Add(new StringContent(payload.Author), "creator");
                content.Add(new StringContent(payload.Description), "description");

                var response = await client.PostAsync(uploadUrl, content).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("[SkinUploader] Скин успешно загружен!");
                    return true;
                }

                string errorMsg = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                System.Diagnostics.Debug.WriteLine($"[SkinUploader] Ошибка сервера: {response.StatusCode} - {errorMsg}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SkinUploader] Критическая ошибка: {ex.Message}");
                return false;
            }
        }

        private static string getImageMimeType(string? extension)
        {
            return extension?.ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "image/jpeg",
            };
        }
    }
}
