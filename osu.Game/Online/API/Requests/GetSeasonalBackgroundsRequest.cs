// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.
#nullable enable

using System.Net;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetSeasonalBackgroundsRequest : APIRequest<APISeasonalBackgrounds>
    {
        private readonly string? category;

        public GetSeasonalBackgroundsRequest()
        {
        }

        public GetSeasonalBackgroundsRequest(string? category)
        {
            this.category = category;
        }

        protected override string Target => getPath();

        private string getPath()
        {
            if (string.IsNullOrEmpty(category))
                return @"seasonal-backgrounds";

            return $@"seasonal-backgrounds?category={WebUtility.UrlEncode(category)}";
        }
    }
}
