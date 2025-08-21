// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online
{
    public class DevelopmentEndpointConfiguration : EndpointConfiguration
    {
        public DevelopmentEndpointConfiguration()
        {
            WebsiteUrl = APIUrl = @"https://osu.jvnko.boats";
            APIClientSecret = @"ijBg9O6aULCYGnvEELYD3IdW7fqrYiFaoMdkzQNA";
            APIClientID = "1";
            SpectatorUrl = $@"https://osu-spec.jvnko.boats/spectator";
            MultiplayerUrl = $@"https://osu-spec.jvnko.boats/multiplayer";
            MetadataUrl = $@"https://osu-spec.jvnko.boats/metadata";
            BeatmapSubmissionServiceUrl = $@"https://osu-bss.jvnko.boats";
        }
    }
}
