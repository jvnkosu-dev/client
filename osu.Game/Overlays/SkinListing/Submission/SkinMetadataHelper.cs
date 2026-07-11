using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.API.Requests;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinListing.Submission
{
    public static class SkinMetadataHelper
    {
        public const string UploadActionText = "Upload skin";
        public const string UpdateUploadActionText = "Update skin";

        public static string ExtractBaseName(string skinName) => SkinIniVersionHelper.ExtractBaseName(skinName);

        public static string GetUploadName(Skin skin) =>
            skin.SkinInfo.PerformRead(s => SkinIniVersionHelper.SanitizeUploadName(s.Name));

        public static string GetUploadAuthor(Skin skin)
        {
            return skin.SkinInfo.PerformRead(s =>
            {
                string creator = s.Creator;

                if (string.IsNullOrWhiteSpace(creator) || creator == @"Unknown")
                    return string.Empty;

                return creator.Trim();
            });
        }

        public static APIOnlineSkin? FindMatchingOnlineSkin(Skin skin, IEnumerable<APIOnlineSkin> listing, string? username)
        {
            string uploadName = GetUploadName(skin);
            string uploadAuthor = GetUploadAuthor(skin);

            return listing.FirstOrDefault(online => matchesUploadIdentity(uploadName, uploadAuthor, online) && IsCurrentUserUploader(online, username));
        }

        private static bool matchesUploadIdentity(string uploadName, string uploadAuthor, APIOnlineSkin online)
        {
            if (!string.Equals(uploadName, SkinIniVersionHelper.SanitizeUploadName(online.Name), StringComparison.OrdinalIgnoreCase))
                return false;

            if (string.IsNullOrWhiteSpace(uploadAuthor) || string.IsNullOrWhiteSpace(online.Creator))
                return string.IsNullOrWhiteSpace(uploadAuthor) && string.IsNullOrWhiteSpace(online.Creator);

            return string.Equals(uploadAuthor, online.Creator, StringComparison.OrdinalIgnoreCase);
        }

        public static void PopulateSettingsFromSkin(SkinSubmissionSettings settings, Skin skin)
        {
            var configuration = skin.Configuration;

            settings.Version.Value = SkinIniVersionHelper.GetSkinVersion(skin, useDefaultIfMissing: true);
            settings.Description.Value = configuration.Description?.Trim() ?? string.Empty;
            settings.Tags.Value = configuration.Tags?.Trim() ?? string.Empty;

            skin.SkinInfo.PerformRead(skinInfo =>
            {
                settings.Name.Value = SkinIniVersionHelper.SanitizeUploadName(skinInfo.Name);

                settings.Author.Value = !string.IsNullOrWhiteSpace(skinInfo.Creator) && skinInfo.Creator != @"Unknown"
                    ? skinInfo.Creator
                    : string.Empty;

                settings.EngineType.Value = !string.IsNullOrWhiteSpace(configuration.SkinType)
                    ? configuration.SkinType.Trim()
                    : SkinEngineTypeHelper.ToStorageString(SkinEngineTypeHelper.FromSkinInfo(skinInfo));
            });

            settings.ModifiedModes.Clear();

            foreach (string mode in SkinIniVersionHelper.ParseModifiedModes(configuration.ModifiedModes))
                settings.ModifiedModes.Add(mode);
        }

        public static void ConfigureFromSkin(SkinSubmissionSettings settings, Skin skin)
        {
            PopulateSettingsFromSkin(settings, skin);

            int onlineSkinId = skin.SkinInfo.PerformRead(s => s.OnlineSkinId);

            if (onlineSkinId <= 0)
                SkinIniVersionHelper.TryGetOnlineSkinId(skin, out onlineSkinId);

            if (onlineSkinId > 0)
            {
                settings.OnlineSkinId.Value = onlineSkinId;
                settings.IsUpdate.Value = true;
            }
            else
            {
                settings.OnlineSkinId.Value = 0;
                settings.IsUpdate.Value = false;
            }
        }

        public static void PopulateSettingsFromOnlineSkin(SkinSubmissionSettings settings, APIOnlineSkin skin)
        {
            settings.Name.Value = SkinIniVersionHelper.GetDisplayName(skin.Name);
            settings.Author.Value = skin.Creator;
            settings.Description.Value = skin.Description.Trim();
            settings.Tags.Value = skin.Tags.Trim();
            settings.Version.Value = SkinIniVersionHelper.GetDisplayVersion(skin.Version, skin.Name);
            settings.EngineType.Value = skin.EngineType.Trim();

            settings.ModifiedModes.Clear();

            foreach (string mode in skin.ModifiedModes ?? Enumerable.Empty<string>())
                settings.ModifiedModes.Add(mode);
        }

        public static void PopulateSettingsForUpdate(SkinSubmissionSettings settings, Skin skin, APIOnlineSkin onlineSkin)
        {
            PopulateSettingsFromSkin(settings, skin);
            PopulateSettingsFromOnlineSkin(settings, onlineSkin);

            settings.Version.Value = SkinIniVersionHelper.GetSkinVersion(skin, useDefaultIfMissing: true);

            if (!string.IsNullOrWhiteSpace(skin.Configuration.SkinType))
                settings.EngineType.Value = skin.Configuration.SkinType.Trim();

            if (settings.ModifiedModes.Count == 0)
            {
                foreach (string mode in onlineSkin.ModifiedModes ?? Enumerable.Empty<string>())
                    settings.ModifiedModes.Add(mode);
            }
        }

        public static bool IsCurrentUserUploader(APIOnlineSkin skin, string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            string? uploader = skin.GetUploaderDisplayName();
            return uploader != null && string.Equals(uploader, username.Trim(), System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
