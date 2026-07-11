using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using osu.Framework.Extensions;
using osu.Game.Database;
using osu.Game.Models;
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

        /// <summary>
        /// Base filename (without extension) for the skin listing/background image stored inside a skin.
        /// </summary>
        public const string BackgroundName = "bg";

        private const string skin_ini_filename = "skin.ini";
        private const string name_key = "Name:";
        private const string author_key = "Author:";
        private const string skin_version_key = "SkinVersion:";
        private const string skin_type_key = "SkinType:";
        private const string modified_modes_key = "ModifiedModes:";
        private const string online_skin_id_key = "OnlineSkinID:";
        private const string description_key = "Description:";
        private const string tags_key = "Tags:";
        private const string server_last_updated_key = "ServerLastUpdated:";
        private const string server_content_length_key = "ServerContentLength:";

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

        /// <summary>
        /// Builds a background filename such as <c>bg.png</c> or <c>bg.jpg</c>, preserving the source extension.
        /// </summary>
        public static string GetBackgroundFilename(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".png";

            if (!extension.StartsWith('.'))
                extension = "." + extension;

            return BackgroundName + extension.ToLowerInvariant();
        }

        public static bool IsBackgroundFilename(string? filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            return string.Equals(Path.GetFileNameWithoutExtension(filename), BackgroundName, StringComparison.OrdinalIgnoreCase);
        }

        public static RealmNamedFileUsage? FindBackgroundFile(IHasRealmFiles model) =>
            model.Files.FirstOrDefault(f => IsBackgroundFilename(f.Filename));

        public static IEnumerable<RealmNamedFileUsage> FindBackgroundFiles(IHasRealmFiles model) =>
            model.Files.Where(f => IsBackgroundFilename(f.Filename));

        public static string GetSkinVersion(Skin skin, bool useDefaultIfMissing = false) => GetSkinVersion(skin.Configuration, useDefaultIfMissing);

        public static bool TryGetOnlineSkinId(SkinConfiguration configuration, out int onlineSkinId)
        {
            onlineSkinId = configuration.OnlineSkinId;
            return onlineSkinId > 0;
        }

        public static bool TryGetOnlineSkinId(Skin skin, out int onlineSkinId) => TryGetOnlineSkinId(skin.Configuration, out onlineSkinId);

        /// <summary>
        /// Reads <c>OnlineSkinID</c> from raw skin.ini text without constructing a full <see cref="Skin"/>.
        /// </summary>
        public static bool TryParseOnlineSkinIdFromIni(string skinIniContent, out int onlineSkinId)
        {
            onlineSkinId = 0;

            if (string.IsNullOrEmpty(skinIniContent))
                return false;

            using (var reader = new StringReader(skinIniContent))
            {
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal) || line.StartsWith(';'))
                        continue;

                    if (!line.StartsWith(online_skin_id_key, StringComparison.OrdinalIgnoreCase))
                        continue;

                    string value = line[(online_skin_id_key.Length)..].Trim();

                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out onlineSkinId) && onlineSkinId > 0)
                        return true;
                }
            }

            onlineSkinId = 0;
            return false;
        }

        public static string GetDisplayVersion(string? apiVersion, string skinName)
        {
            if (!string.IsNullOrWhiteSpace(apiVersion))
                return apiVersion.Trim();

            return ExtractVersionFromName(skinName) ?? string.Empty;
        }

        /// <summary>
        /// Returns a clean skin name for display in listings, with bracketed suffixes removed.
        /// </summary>
        public static string GetDisplayName(string skinName) => SanitizeUploadName(skinName);

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
        /// Prepares a skin name for upload by removing version suffixes and any bracketed segments.
        /// </summary>
        public static string SanitizeUploadName(string skinName)
        {
            skinName = ExtractBaseName(skinName);
            return removeAllBracketedSegments(skinName).Trim();
        }

        public static IEnumerable<string> ParseModifiedModes(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                yield break;

            foreach (string part in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (part.Length > 0)
                    yield return part;
            }
        }

        public static string UpdateSkinIniContent(string content, string version) =>
            UpdateSkinIniMetadata(content, name: null, author: null, version: version);

        public static string UpdateSkinIniMetadata(string content, string? name, string? author, string? version, string? skinType = null, string? modifiedModes = null, int? onlineSkinId = null, string? description = null, string? tags = null)
        {
            var lines = content.Replace("\r\n", "\n").Split('\n').ToList();

            if (!string.IsNullOrWhiteSpace(name))
                updateAllMatchingKeys(lines, name_key, name.Trim());

            if (!string.IsNullOrWhiteSpace(author))
                updateAllMatchingKeys(lines, author_key, author.Trim());

            if (!string.IsNullOrWhiteSpace(version))
                updateAllMatchingKeys(lines, skin_version_key, version.Trim());

            if (!string.IsNullOrWhiteSpace(skinType))
                updateAllMatchingKeys(lines, skin_type_key, skinType.Trim());

            if (!string.IsNullOrWhiteSpace(modifiedModes))
                updateAllMatchingKeys(lines, modified_modes_key, modifiedModes.Trim());

            if (description != null)
                updateAllMatchingKeys(lines, description_key, description.Trim());

            if (tags != null)
                updateAllMatchingKeys(lines, tags_key, tags.Trim());

            if (onlineSkinId is > 0)
                updateAllMatchingKeys(lines, online_skin_id_key, onlineSkinId.Value.ToString(CultureInfo.InvariantCulture));

            appendUploadMetadataBlock(lines, name, author, version, skinType, modifiedModes, onlineSkinId, description, tags);

            return string.Join('\n', lines);
        }

        /// <summary>
        /// Applies skin editor setup fields to <c>skin.ini</c>, updating existing keys or inserting them under <c>[General]</c>.
        /// </summary>
        public static string ApplyEditorSetupMetadata(string content, string name, string author, string version, string description, string tags, string skinType, string modifiedModes)
        {
            var lines = content.Replace("\r\n", "\n").Split('\n').ToList();

            setOrInsertKey(lines, name_key, name.Trim());
            setOrInsertKey(lines, author_key, author.Trim());
            setOrInsertKey(lines, skin_version_key, string.IsNullOrWhiteSpace(version) ? DEFAULT_VERSION : version.Trim());
            setOrInsertKey(lines, description_key, description.Trim());
            setOrInsertKey(lines, tags_key, tags.Trim());
            setOrInsertKey(lines, skin_type_key, skinType.Trim());
            setOrInsertKey(lines, modified_modes_key, modifiedModes.Trim());

            return string.Join('\n', lines);
        }

        /// <summary>
        /// Writes the server listing snapshot used for downloader update detection into <c>skin.ini</c>.
        /// </summary>
        public static string ApplyServerSnapshotMetadata(
            string content,
            string name,
            string author,
            string version,
            string description,
            string tags,
            string skinType,
            string modifiedModes,
            int onlineSkinId,
            string? serverLastUpdated,
            long? serverContentLength)
        {
            var lines = content.Replace("\r\n", "\n").Split('\n').ToList();

            setOrInsertKey(lines, name_key, name.Trim());
            setOrInsertKey(lines, author_key, author.Trim());
            setOrInsertKey(lines, skin_version_key, string.IsNullOrWhiteSpace(version) ? DEFAULT_VERSION : version.Trim());
            setOrInsertKey(lines, description_key, description.Trim());
            setOrInsertKey(lines, tags_key, tags.Trim());
            setOrInsertKey(lines, skin_type_key, skinType.Trim());
            setOrInsertKey(lines, modified_modes_key, modifiedModes.Trim());

            if (onlineSkinId > 0)
                setOrInsertKey(lines, online_skin_id_key, onlineSkinId.ToString(CultureInfo.InvariantCulture));

            if (!string.IsNullOrWhiteSpace(serverLastUpdated))
                setOrInsertKey(lines, server_last_updated_key, serverLastUpdated.Trim());

            if (serverContentLength is > 0)
                setOrInsertKey(lines, server_content_length_key, serverContentLength.Value.ToString(CultureInfo.InvariantCulture));

            return string.Join('\n', lines);
        }

        /// <summary>
        /// After a successful listing upload, sync <c>OnlineSkinID</c> + <c>ServerLastUpdated</c> without rewriting other metadata.
        /// </summary>
        public static string ApplyUploadSyncMetadata(string content, int onlineSkinId, string serverLastUpdated)
        {
            var lines = content.Replace("\r\n", "\n").Split('\n').ToList();

            if (onlineSkinId > 0)
                setOrInsertKey(lines, online_skin_id_key, onlineSkinId.ToString(CultureInfo.InvariantCulture));

            if (!string.IsNullOrWhiteSpace(serverLastUpdated))
                setOrInsertKey(lines, server_last_updated_key, serverLastUpdated.Trim());

            return string.Join('\n', lines);
        }

        public static void EnsureSkinMetadataInOsk(string oskPath, string name, string author, string version, string? skinType = null, string? modifiedModes = null, int? onlineSkinId = null) =>
            patchSkinIniInOsk(oskPath, content => UpdateSkinIniMetadata(content, name, author, version, skinType, modifiedModes, onlineSkinId));

        public static void EnsureOnlineSkinIdInOsk(string oskPath, int onlineSkinId) =>
            patchSkinIniInOsk(oskPath, content => UpdateSkinIniMetadata(content, name: null, author: null, version: null, onlineSkinId: onlineSkinId));

        public static void EnsureSkinVersionInOsk(string oskPath, string version) =>
            patchSkinIniInOsk(oskPath, content => UpdateSkinIniContent(content, version));

        private static string removeAllBracketedSegments(string text)
        {
            while (true)
            {
                int open = text.IndexOf('[');
                if (open < 0)
                    return text;

                int close = text.IndexOf(']', open);
                if (close < 0)
                    return text;

                text = (text[..open] + text[(close + 1)..]).Trim();
            }
        }

        private static void updateAllMatchingKeys(List<string> lines, string key, string value)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (!lines[i].TrimStart().StartsWith(key, StringComparison.OrdinalIgnoreCase))
                    continue;

                lines[i] = $"{key} {value}";
            }
        }

        private static void appendUploadMetadataBlock(List<string> lines, string? name, string? author, string? version, string? skinType, string? modifiedModes, int? onlineSkinId, string? description = null, string? tags = null)
        {
            if (string.IsNullOrWhiteSpace(name)
                && string.IsNullOrWhiteSpace(author)
                && string.IsNullOrWhiteSpace(version)
                && string.IsNullOrWhiteSpace(skinType)
                && string.IsNullOrWhiteSpace(modifiedModes)
                && string.IsNullOrWhiteSpace(description)
                && string.IsNullOrWhiteSpace(tags)
                && onlineSkinId is not > 0)
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

            if (!string.IsNullOrWhiteSpace(skinType))
                lines.Add($"{skin_type_key} {skinType.Trim()}");

            if (!string.IsNullOrWhiteSpace(modifiedModes))
                lines.Add($"{modified_modes_key} {modifiedModes.Trim()}");

            if (!string.IsNullOrWhiteSpace(description))
                lines.Add($"{description_key} {description.Trim()}");

            if (!string.IsNullOrWhiteSpace(tags))
                lines.Add($"{tags_key} {tags.Trim()}");

            if (onlineSkinId is > 0)
                lines.Add($"{online_skin_id_key} {onlineSkinId.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        private static void setOrInsertKey(List<string> lines, string key, string value)
        {
            bool updated = false;

            for (int i = 0; i < lines.Count; i++)
            {
                if (!lines[i].TrimStart().StartsWith(key, StringComparison.OrdinalIgnoreCase))
                    continue;

                lines[i] = $"{key} {value}";
                updated = true;
            }

            if (updated)
                return;

            int generalIndex = lines.FindIndex(l => l.Trim().Equals("[General]", StringComparison.OrdinalIgnoreCase));

            if (generalIndex < 0)
            {
                if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                    lines.Add(string.Empty);

                lines.Add("[General]");
                lines.Add($"{key} {value}");
                return;
            }

            int insertAt = generalIndex + 1;

            while (insertAt < lines.Count && string.IsNullOrWhiteSpace(lines[insertAt]))
                insertAt++;

            lines.Insert(insertAt, $"{key} {value}");
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
                using (var output = File.Create(tempPath))
                using (var writer = new ZipWriter(output, new ZipWriterOptions(CompressionType.Deflate) { ArchiveEncoding = skin_archive_encoding }))
                {
                    bool skinIniUpdated = false;

                    using (var input = File.OpenRead(oskPath))
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
