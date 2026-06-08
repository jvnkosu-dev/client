using System.Collections.Generic;
using osu.Game.Online.API;

namespace osu.Game.Online.API.Requests
{
    public class GetSkinsRequest : APIRequest<List<APIOnlineSkin>>
    {
        private readonly string search;

        public GetSkinsRequest(string search = "")
        {
            this.search = search;
        }

        protected override string Uri => $"https://osu.jvnko.boats/api/skins?q={System.Uri.EscapeDataString(search)}";

        protected override string Target => string.Empty;
    }
}
