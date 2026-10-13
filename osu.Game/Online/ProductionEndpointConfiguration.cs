// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online
{
    public class ProductionEndpointConfiguration : EndpointConfiguration
    {
        public ProductionEndpointConfiguration()
        {
            WebsiteUrl = APIUrl = @"https://osu.jvnko.boats";
            // OAuth client credentials are supplied at release/build time — do not commit secrets.
            APIClientSecret = string.Empty;
            APIClientID = string.Empty;
            SpectatorUrl = $@"https://osu-spec.jvnko.boats/spectator";
            MultiplayerUrl = $@"https://osu-spec.jvnko.boats/multiplayer";
            MetadataUrl = $@"https://osu-spec.jvnko.boats/metadata";
            BeatmapSubmissionServiceUrl = $@"https://osu-bss.jvnko.boats";
            MarathonUrl = @"https://osu-marathon.jvnko.boats";
        }
    }
}
