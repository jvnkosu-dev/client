// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Net;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetWelcomeMusicRequest : APIRequest<List<APIWelcomeMusic>>
    {
        private readonly string category;

        public GetWelcomeMusicRequest(string category)
        {
            this.category = category;
        }

        protected override string Target => $"https://osu.jvnko.boats/welcome-music/list?category={WebUtility.UrlEncode(category)}";
    }
}
