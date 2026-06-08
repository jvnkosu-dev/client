using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    public partial class SkinUploader : Component
    {
        private readonly string uploadUrl = "https://osu.jvnko.boats/api/skins/upload";

        public async Task<bool> UploadSkinAsync(string oskFilePath, string description = "")
        {
            if (!File.Exists(oskFilePath))
            {
                System.Diagnostics.Debug.WriteLine("[SkinUploader] Файл скина не найден!");
                return false;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    using (var content = new MultipartFormDataContent())
                    {
                        using (var fileStream = new FileStream(oskFilePath, FileMode.Open, FileAccess.Read))
                        {
                            var fileContent = new StreamContent(fileStream);

                            content.Add(fileContent, "skin", Path.GetFileName(oskFilePath));
                            content.Add(new StringContent(description), "description");

                            // Добавили ConfigureAwait(false) для чистоты кода
                            var response = await client.PostAsync(uploadUrl, content).ConfigureAwait(false);

                            if (response.IsSuccessStatusCode)
                            {
                                System.Diagnostics.Debug.WriteLine("[SkinUploader] Скин успешно загружен!");
                                return true;
                            }
                            else
                            {
                                string errorMsg = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                System.Diagnostics.Debug.WriteLine($"[SkinUploader] Ошибка сервера: {response.StatusCode} - {errorMsg}");
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SkinUploader] Критическая ошибка: {ex.Message}");
                return false;
            }
        }
    }
}
