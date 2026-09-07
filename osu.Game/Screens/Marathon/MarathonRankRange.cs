// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Screens.Marathon
{
    /// <summary>
    /// Recommended global-rank band for a marathon. Older playlists stored this
    /// as "Digit N" tags; new ones persist <see cref="APIMarathon.RankMin"/> /
    /// <see cref="APIMarathon.RankMax"/> directly.
    /// </summary>
    public static class MarathonRankRange
    {
        public static bool TryGet(APIMarathon marathon, out int min, out int max)
        {
            if (TryNormalise(marathon.RankMin, marathon.RankMax, out min, out max))
                return true;

            return TryFromTags(marathon.Tags, out min, out max);
        }

        public static bool TryNormalise(int min, int max, out int from, out int to)
        {
            from = min;
            to = max;

            if (from <= 0 && to <= 0)
                return false;

            if (from <= 0)
                from = to;
            else if (to <= 0)
                to = from;

            if (from > to)
                (from, to) = (to, from);

            from = Math.Max(1, from);
            to = Math.Max(from, to);
            return true;
        }

        public static bool TryFromTags(IEnumerable<string>? tags, out int min, out int max)
        {
            min = max = 0;

            if (tags == null)
                return false;

            int lowestDigit = int.MaxValue;
            int highestDigit = int.MinValue;

            foreach (string tag in tags)
            {
                if (!tryParseDigit(tag, out int digit))
                    continue;

                lowestDigit = Math.Min(lowestDigit, digit);
                highestDigit = Math.Max(highestDigit, digit);
            }

            if (highestDigit < lowestDigit)
                return false;

            min = digitFloor(lowestDigit);
            max = digitCeil(highestDigit);
            return true;
        }

        /// <summary>
        /// Distance from <paramref name="viewerRank"/> to the band. Zero if the rank is inside it.
        /// </summary>
        public static int Gap(int min, int max, int viewerRank)
        {
            if (!TryNormalise(min, max, out min, out max))
                return int.MaxValue;

            if (viewerRank >= min && viewerRank <= max)
                return 0;

            return viewerRank < min ? min - viewerRank : viewerRank - max;
        }

        /// <summary>
        /// Traffic-light fit: inside or within 100k of the band is close,
        /// 100–200k is medium, anything wider is far.
        /// </summary>
        public static RankFit Fit(int min, int max, int? viewerRank)
        {
            if (viewerRank is not > 0 || !TryNormalise(min, max, out min, out max))
                return RankFit.Unknown;

            int gap = Gap(min, max, viewerRank.Value);

            if (gap < 100_000)
                return RankFit.Close;

            if (gap <= 200_000)
                return RankFit.Medium;

            return RankFit.Far;
        }

        private static bool tryParseDigit(string? tag, out int digit)
        {
            digit = 0;

            if (string.IsNullOrWhiteSpace(tag))
                return false;

            tag = tag.Trim();

            if (tag.StartsWith("Digit ", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(tag.AsSpan("Digit ".Length), out digit))
            {
                return digit is >= 1 and <= 6;
            }

            return int.TryParse(tag, out digit) && digit is >= 1 and <= 6;
        }

        private static int digitFloor(int digit) => digit <= 1 ? 1 : (int)Math.Pow(10, digit - 1);

        private static int digitCeil(int digit) => digit >= 6 ? 9_999_999 : (int)Math.Pow(10, digit) - 1;
    }

    public enum RankFit
    {
        Unknown,
        Close,
        Medium,
        Far,
    }
}
