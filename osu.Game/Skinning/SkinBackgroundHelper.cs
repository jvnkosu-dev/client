// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO;

namespace osu.Game.Skinning
{
    public static class SkinBackgroundHelper
    {
        /// <summary>
        /// Loads the skin's <c>bg.*</c> image as a fresh texture from user file storage.
        /// Always reads bytes directly (avoids TextureStore / realm lookup caches that only clear on restart).
        /// </summary>
        public static Texture? GetTexture(Skin skin, IRenderer renderer, IStorageResourceProvider resources)
        {
            string? storagePath = skin.SkinInfo.PerformRead(s =>
            {
                var file = SkinIniVersionHelper.FindBackgroundFile(s);
                return file?.File.GetStoragePath();
            });

            if (storagePath == null)
                return null;

            return loadFromStoragePath(renderer, resources.Files, storagePath);
        }

        /// <summary>
        /// Copies the skin background to a temp file for upload, or returns <c>null</c> if none exists.
        /// </summary>
        public static string? ExportBackgroundToTempFile(Skin skin, IResourceStore<byte[]> files)
        {
            var background = skin.SkinInfo.PerformRead(s =>
            {
                var file = SkinIniVersionHelper.FindBackgroundFile(s);
                if (file == null)
                    return null;

                return new BackgroundFile(file.Filename, file.File.GetStoragePath());
            });

            if (background == null)
                return null;

            using var stream = files.GetStream(background.StoragePath);
            if (stream == null)
                return null;

            string tempPath = Path.Combine(Path.GetTempPath(), $"skin-bg-{Guid.NewGuid()}{Path.GetExtension(background.Filename)}");

            using (var output = File.Create(tempPath))
                stream.CopyTo(output);

            return tempPath;
        }

        private static Texture? loadFromStoragePath(IRenderer renderer, IResourceStore<byte[]> files, string storagePath)
        {
            try
            {
                using var stream = files.GetStream(storagePath);
                if (stream == null)
                    return null;

                // Texture.FromStream requires a seekable stream; storage streams often are not.
                using var memory = new MemoryStream();
                stream.CopyTo(memory);
                memory.Position = 0;

                if (memory.Length == 0)
                    return null;

                return Texture.FromStream(renderer, memory);
            }
            catch
            {
                return null;
            }
        }

        private record BackgroundFile(string Filename, string StoragePath);
    }
}
