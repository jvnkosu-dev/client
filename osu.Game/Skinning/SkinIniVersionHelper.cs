using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using osu.Framework.Extensions;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;

namespace osu.Game.Skinning
{
    public static class SkinIniVersionHelper
    {
        public const string DEFAULT_VERSION = "1.0";

        private const string skin_ini_filename = "skin.ini";
        private const string name_key = "Name:";
        private const string author_key = "Author:";
        private const string skin_version_key = "SkinVersion:";

        private static readonly ArchiveEncoding skin_archive_encoding = new ArchiveEncoding
        {
            Default = Encoding.UTF8,
            Password = Encoding.UTF8,
        };

        public static string GetSkinVersion(SkinConfiguration configuration, bool useDefaultIfMissing = false)
        {
            if (!string.IsNullOrWhiteSpace(configuration.SkinVersion))
                return configuration.SkinVersion.Trim();

            return useDefaultIfMissing ? DEFAULT_VERSION : string.Empty;
        }

        public static string GetSkinVersion(Skin skin, bool useDefaultIfMissing = false) => GetSkinVersion(skin.Configuration, useDefaultIfMissing);

        public static string GetDisplayVersion(string? apiVersion, string skinName)
        {
            if (!string.IsNullOrWhiteSpace(apiVersion))
                return apiVersion.Trim();

            return ExtractVersionFromName(skinName) ?? string.Empty;
        }

        public static string? ExtractVersionFromName(string skinName)
        {
            int bracketIndex = skinName.LastIndexOf(" [", StringComparison.Ordinal);
            if (bracketIndex < 0 || !skinName.EndsWith(']'))
                return null;

            string suffix = skinName[(bracketIndex + 2)..^1].Trim();
            return suffix.Length > 0 ? suffix : null;
        }

        public static string ExtractBaseName(string skinName)
        {
            int bracketIndex = skinName.LastIndexOf(" [", StringComparison.Ordinal);
            if (bracketIndex >= 0 && skinName.EndsWith(']'))
                return skinName[..bracketIndex].Trim();

            return skinName.Trim();
        }

        /// <summary>
        /// Formats a skin name for server listing when the API does not expose a separate version field.
        /// </summary>
        public static string FormatUploadName(string baseName, string version)
        {
            baseName = ExtractBaseName(baseName);

            if (string.IsNullOrWhiteSpace(version))
                return baseName;

            if (ExtractVersionFromName(baseName) != null)
                return baseName;

            return $"{baseName} [{version.Trim()}]";
        }

        public static string UpdateSkinIniContent(string content, string version) =>
            UpdateSkinIniMetadata(content, name: null, author: null, version: version);

        public static string UpdateSkinIniMetadata(string content, string? name, string? author, string? version)
        {
            var lines = content.Replace("\r\n", "\n").Split('\n').ToList();

            if (!string.IsNullOrWhiteSpace(name))
                updateAllMatchingKeys(lines, name_key, name.Trim());

            if (!string.IsNullOrWhiteSpace(author))
                updateAllMatchingKeys(lines, author_key, author.Trim());

            if (!string.IsNullOrWhiteSpace(version))
                updateAllMatchingKeys(lines, skin_version_key, version.Trim());

            appendUploadMetadataBlock(lines, name, author, version);

            return string.Join('\n', lines);
        }

        public static void EnsureSkinMetadataInOsk(string oskPath, string name, string author, string version) =>
            patchSkinIniInOsk(oskPath, content => UpdateSkinIniMetadata(content, name, author, version));

        public static void EnsureSkinVersionInOsk(string oskPath, string version) =>
            patchSkinIniInOsk(oskPath, content => UpdateSkinIniContent(content, version));

        private static void updateAllMatchingKeys(List<string> lines, string key, string value)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (!lines[i].TrimStart().StartsWith(key, StringComparison.OrdinalIgnoreCase))
                    continue;

                lines[i] = $"{key} {value}";
            }
        }

        private static void appendUploadMetadataBlock(List<string> lines, string? name, string? author, string? version)
        {
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(author) && string.IsNullOrWhiteSpace(version))
                return;

            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                lines.Add(string.Empty);

            lines.Add("// The following content was automatically added during skin upload.");
            lines.Add("[General]");

            if (!string.IsNullOrWhiteSpace(name))
                lines.Add($"{name_key} {name.Trim()}");

            if (!string.IsNullOrWhiteSpace(author))
                lines.Add($"{author_key} {author.Trim()}");

            if (!string.IsNullOrWhiteSpace(version))
                lines.Add($"{skin_version_key} {version.Trim()}");
        }

        private static bool isSkinIniEntry(string? key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            string normalised = key.Replace('\\', '/');
            int slashIndex = normalised.LastIndexOf('/');

            if (slashIndex >= 0)
                normalised = normalised[(slashIndex + 1)..];

            return normalised.Equals(skin_ini_filename, StringComparison.OrdinalIgnoreCase);
        }

        private static void patchSkinIniInOsk(string oskPath, Func<string, string> updateContent)
        {
            string tempPath = oskPath + ".tmp";

            try
            {
                byte[] inputBytes = File.ReadAllBytes(oskPath);

                using (var output = File.Create(tempPath))
                using (var writer = new ZipWriter(output, new ZipWriterOptions(CompressionType.Deflate) { ArchiveEncoding = skin_archive_encoding }))
                {
                    bool skinIniUpdated = false;

                    using (var input = new MemoryStream(inputBytes))
                    using (var archive = ZipArchive.OpenArchive(input, new ReaderOptions { ArchiveEncoding = skin_archive_encoding }))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (entry.IsDirectory)
                                continue;

                            string entryKey = entry.Key ?? string.Empty;
                            if (string.IsNullOrEmpty(entryKey))
                                continue;

                            if (isSkinIniEntry(entryKey))
                            {
                                string iniContent = updateContent(readArchiveEntryText(entry));

                                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(iniContent)))
                                    writer.Write(entryKey, ms);

                                skinIniUpdated = true;
                            }
                            else
                            {
                                using (var stream = entry.OpenEntryStream())
                                using (var ms = new MemoryStream())
                                {
                                    stream.CopyTo(ms);
                                    ms.Position = 0;
                                    writer.Write(entryKey, ms);
                                }
                            }
                        }
                    }

                    if (!skinIniUpdated)
                    {
                        string iniContent = updateContent(string.Empty);

                        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(iniContent)))
                            writer.Write(skin_ini_filename, ms);
                    }
                }

                File.Delete(oskPath);
                File.Move(tempPath, oskPath);
            }
            catch
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);

                throw;
            }
        }

        private static string readArchiveEntryText(IArchiveEntry entry)
        {
            using (var stream = entry.OpenEntryStream())
            {
                if (entry.Size > 0)
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                        return reader.ReadToEnd();
                }

                return Encoding.UTF8.GetString(stream.ReadAllRemainingBytesToArray());
            }
        }
    }
}
