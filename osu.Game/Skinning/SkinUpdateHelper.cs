// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Logging;
using osu.Game.Online.API.Requests;

namespace osu.Game.Skinning
{
    public static class SkinUpdateHelper
    {
        /// <summary>
        /// Whether a downloaded skin should offer an update from the server listing.
        /// <list type="bullet">
        /// <item><see cref="SkinInfo.LocallyModified"/> — local edits since last listing sync</item>
        /// <item>SkinVersion vs listing version — remote package changed</item>
        /// </list>
        /// Does not use <c>LastUpdated</c> or content-hash drift: after the author uploads, the server
        /// timestamp / hash rewrite race falsely offered Update while the origin pill stayed Server.
        /// </summary>
        public static bool IsUpdateAvailable(Skin local, APIOnlineSkin online)
        {
            if (online.OnlineID <= 0)
                return false;

            var skinInfo = local.SkinInfo.Value;

            if (skinInfo.LocallyModified)
            {
                log(online.OnlineID, "LocallyModified");
                return true;
            }

            string localVersion = SkinIniVersionHelper.GetSkinVersion(local, useDefaultIfMissing: false);
            string onlineVersion = SkinIniVersionHelper.GetDisplayVersion(online.Version, online.Name);

            // Only when both sides expose a version — avoids empty-vs-"1.0" false positives after upload.
            if (!string.IsNullOrWhiteSpace(localVersion)
                && !string.IsNullOrWhiteSpace(onlineVersion)
                && !stringEquals(localVersion, onlineVersion))
            {
                log(online.OnlineID, $"VersionMismatch local='{localVersion}' online='{onlineVersion}'");
                return true;
            }

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

        private static bool stringEquals(string? left, string? right) =>
            string.Equals((left ?? string.Empty).Trim(), (right ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);

        private static void log(int onlineId, string reason) =>
            Logger.Log($"Skin update offered for #{onlineId}: {reason}", LoggingTarget.Information);
    }
}
