// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Linq;
using osu.Game.Online.API.Requests;

namespace osu.Game.Skinning
{
    public static class SkinUpdateHelper
    {
        /// <summary>
        /// Whether a downloaded skin should offer an update from the server listing.
        /// Compares listing metadata, the persisted <c>ServerLastUpdated</c> snapshot,
        /// and local content hash vs <see cref="SkinInfo.OnlineHash"/> (same idea as beatmap <c>MatchesOnlineVersion</c>).
        /// </summary>
        public static bool IsUpdateAvailable(Skin local, APIOnlineSkin online)
        {
            if (online.OnlineID <= 0)
                return false;

            var configuration = local.Configuration;
            var skinInfo = local.SkinInfo.Value;

            // Local layout / file edits after last download or update.
            if (skinInfo.LocallyModified || !skinInfo.MatchesOnlineVersion)
                return true;

            string localName = SkinIniVersionHelper.SanitizeUploadName(skinInfo.Name);
            string onlineName = SkinIniVersionHelper.SanitizeUploadName(online.Name);

            if (!stringEquals(localName, onlineName))
                return true;

            string localAuthor = skinInfo.Creator == @"Unknown" ? string.Empty : skinInfo.Creator.Trim();
            if (!stringEquals(localAuthor, online.Creator))
                return true;

            string localVersion = SkinIniVersionHelper.GetSkinVersion(local, useDefaultIfMissing: false);
            string onlineVersion = SkinIniVersionHelper.GetDisplayVersion(online.Version, online.Name);
            if (!stringEquals(localVersion, onlineVersion))
                return true;

            if (!stringEquals(configuration.Description, online.Description))
                return true;

            if (!stringEquals(configuration.Tags, online.Tags))
                return true;

            string localType = !string.IsNullOrWhiteSpace(configuration.SkinType)
                ? configuration.SkinType.Trim()
                : SkinEngineTypeHelper.ToStorageString(SkinEngineTypeHelper.FromSkinInfo(skinInfo));

            if (!stringEquals(localType, online.EngineType))
                return true;

            var localModes = SkinModifiedModesHelper.GetNormalizedShortNames(SkinIniVersionHelper.ParseModifiedModes(configuration.ModifiedModes));
            var onlineModes = SkinModifiedModesHelper.GetNormalizedShortNames(online.ModifiedModes ?? Enumerable.Empty<string>());

            if (!localModes.SetEquals(onlineModes))
                return true;

            if (filesOutOfDate(configuration, online))
                return true;

            return false;
        }

        public static string FormatServerLastUpdated(DateTimeOffset? lastUpdated)
        {
            if (lastUpdated == null)
                return string.Empty;

            return lastUpdated.Value.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
        }

        public static bool TryParseServerLastUpdated(string? value, out DateTimeOffset parsed)
        {
            parsed = default;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed);
        }

        private static bool filesOutOfDate(SkinConfiguration configuration, APIOnlineSkin online)
        {
            // Without a local snapshot, rely on metadata comparison only (avoid forcing Update on all legacy installs).
            if (string.IsNullOrWhiteSpace(configuration.ServerLastUpdated))
                return false;

            if (online.LastUpdated == null)
                return false;

            if (!TryParseServerLastUpdated(configuration.ServerLastUpdated, out var localUpdated))
                return false;

            return online.LastUpdated.Value.UtcDateTime > localUpdated.UtcDateTime.AddSeconds(1);
        }

        private static bool stringEquals(string? left, string? right) =>
            string.Equals((left ?? string.Empty).Trim(), (right ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
