using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Extensions;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.SkinListing;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    public static class SkinModifiedModesHelper
    {
        public static readonly string[] LegacyShortNames = { "osu", "taiko", "fruits", "mania" };

        public static IEnumerable<RulesetInfo> GetLegacyRulesetsInOrder(RulesetStore rulesets) =>
            rulesets.AvailableRulesets.Where(r => r.IsLegacyRuleset()).OrderBy(r => r.OnlineID);

        public static IEnumerable<RulesetInfo> GetMatchingRulesetsInOrder(IReadOnlyCollection<string> modes, RulesetStore rulesets) =>
            GetLegacyRulesetsInOrder(rulesets).Where(r => modes.Any(mode => modeMatches(mode, r)));

        public static bool HasFilter(RulesetInfo ruleset) => ruleset.OnlineID >= 0;

        public static bool IncludesMode(APIOnlineSkin skin, RulesetInfo ruleset) =>
            skin.ModifiedModes.Any(mode => modeMatches(mode, ruleset));

        public static bool IncludesMode(APIOnlineSkin skin, SkinListingModifiedMode mode) =>
            skin.ModifiedModes.Any(m => modeMatchesShortName(m, ToShortName(mode)));

        public static string ToShortName(SkinListingModifiedMode mode) => mode switch
        {
            SkinListingModifiedMode.Taiko => "taiko",
            SkinListingModifiedMode.Catch => "fruits",
            SkinListingModifiedMode.Mania => "mania",
            _ => "osu",
        };

        public static IEnumerable<APIOnlineSkin> Filter(IEnumerable<APIOnlineSkin> skins, IReadOnlyCollection<SkinListingModifiedMode> selectedModes)
        {
            if (selectedModes.Count == 0)
                return skins;

            var selectedSet = selectedModes.Select(ToShortName).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return skins.Where(s => GetNormalizedShortNames(s.ModifiedModes).SetEquals(selectedSet));
        }

        public static HashSet<string> GetNormalizedShortNames(IEnumerable<string> modes)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var mode in modes)
            {
                string? normalized = normalizeToShortName(mode);

                if (normalized != null)
                    result.Add(normalized);
            }

            return result;
        }

        public static IEnumerable<APIOnlineSkin> Filter(IEnumerable<APIOnlineSkin> skins, RulesetInfo ruleset)
        {
            if (!HasFilter(ruleset))
                return skins;

            return skins.Where(s => IncludesMode(s, ruleset));
        }

        public static string FormatForDisplay(IReadOnlyCollection<string> modes, RulesetStore rulesets)
        {
            var matched = GetLegacyRulesetsInOrder(rulesets)
                .Where(r => modes.Any(mode => modeMatches(mode, r)))
                .ToList();

            if (matched.Count == 0)
                return string.Empty;

            const int total = 4;
            string names = string.Join(", ", matched.Select(r => r.Name));

            if (matched.Count == total)
                return $"All rulesets ({total} of {total})";

            return $"{names} ({matched.Count} of {total})";
        }

        public static string FormatForUpload(IEnumerable<string> shortNames) =>
            string.Join(",", shortNames.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => Array.IndexOf(LegacyShortNames, s)));

        public static bool modeMatches(string mode, RulesetInfo ruleset)
        {
            if (string.Equals(mode, ruleset.ShortName, StringComparison.OrdinalIgnoreCase))
                return true;

            return int.TryParse(mode, out int id) && id == ruleset.OnlineID;
        }

        private static bool modeMatchesShortName(string mode, string shortName) =>
            string.Equals(mode, shortName, StringComparison.OrdinalIgnoreCase);

        private static string? normalizeToShortName(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
                return null;

            foreach (string shortName in LegacyShortNames)
            {
                if (string.Equals(mode, shortName, StringComparison.OrdinalIgnoreCase))
                    return shortName;
            }

            if (int.TryParse(mode, out int id) && id >= 0 && id < LegacyShortNames.Length)
                return LegacyShortNames[id];

            return null;
        }
    }
}
