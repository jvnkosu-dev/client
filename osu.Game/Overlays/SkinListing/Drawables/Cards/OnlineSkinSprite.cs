// Copyright (c) jvnkosu! team, MIT license
// See the LICENCE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    /// <summary>
    /// Loads a skin thumbnail synchronously (via <see cref="LongRunningLoadAttribute"/>)
    /// so parent wrappers such as <see cref="UpdateableOnlineSkinCover"/> can fade it in
    /// only after the texture is ready — matching <see cref="Beatmaps.Drawables.OnlineBeatmapSetCover"/>.
    /// </summary>
    [LongRunningLoad]
    public partial class OnlineSkinSprite : Sprite
    {
        private readonly string? url;

        public OnlineSkinSprite(string? url)
        {
            this.url = url;
            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fill;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            if (string.IsNullOrEmpty(url))
                return;

            string? requestUrl = encodeUrl(url);

            if (requestUrl == null)
                return;

            Texture = textures.Get(requestUrl);
        }

        private static string? encodeUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return url;

            string encodedPath = string.Join("/", uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString));

            var builder = new UriBuilder
            {
                Scheme = uri.Scheme,
                Host = uri.Host,
                Port = uri.Port,
                Path = encodedPath,
            };

            return builder.Uri.AbsoluteUri;
        }
    }
}
