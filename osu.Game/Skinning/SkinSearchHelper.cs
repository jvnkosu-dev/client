using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.API.Requests;

namespace osu.Game.Skinning
{
    public static class SkinSearchHelper
    {
        public static IEnumerable<APIOnlineSkin> Filter(IEnumerable<APIOnlineSkin> skins, string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return skins;

            string q = query.Trim();

            return skins.Where(s =>
                contains(s.Name, q)
                || contains(s.Creator, q)
                || contains(s.Description, q)
                || matchesTags(s.Tags, q));
        }

        private static bool matchesTags(string? tags, string query)
        {
            if (string.IsNullOrWhiteSpace(tags))
                return false;

            return tags.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                       .Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        private static bool contains(string? value, string query) =>
            !string.IsNullOrEmpty(value) && value.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
