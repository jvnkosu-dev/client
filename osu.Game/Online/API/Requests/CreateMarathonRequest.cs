// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class CreateMarathonRequest : MarathonServiceRequest<APIMarathon>
    {
        private readonly APIMarathon marathon;

        public CreateMarathonRequest(APIMarathon marathon)
        {
            this.marathon = marathon;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.ContentType = @"application/json";
            req.Method = HttpMethod.Post;
            req.AddRaw(JsonConvert.SerializeObject(marathon));
            return req;
        }

        protected override string Path => @"/api/marathons";
    }
}
