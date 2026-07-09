// Copyright (c) jvnkosu! team, MIT license
// See the LICENCE file in the repository root for full license text.

namespace osu.Game.Online.API.Requests
{
    public class GetSkinRequest : APIRequest<APIOnlineSkin>
    {
        public int ID { get; }

        public GetSkinRequest(int id)
        {
            ID = id;
        }

        protected override string Uri => $"https://osu.jvnko.boats/api/skins/{ID}";

        protected override string Target => string.Empty;
    }
}
