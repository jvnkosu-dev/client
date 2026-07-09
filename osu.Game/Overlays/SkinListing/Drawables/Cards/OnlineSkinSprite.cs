using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class OnlineSkinSprite : Sprite
    {
        private readonly string? url;

        [Resolved]
        private IRenderer renderer { get; set; } = null!;

        private CancellationTokenSource? loadCancellation;

        public OnlineSkinSprite(string? url)
        {
            this.url = url;
            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fill;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Gray5;

            if (string.IsNullOrEmpty(url))
                return;

            string? requestUrl = encodeUrl(url);

            if (requestUrl == null)
                return;

            loadCancellation = new CancellationTokenSource();
            var token = loadCancellation.Token;

            Task.Run(async () =>
            {
                try
                {
                    using var client = new HttpClient();
                    var bytes = await client.GetByteArrayAsync(new Uri(requestUrl), token).ConfigureAwait(false);

                    if (token.IsCancellationRequested)
                        return;

                    Schedule(() =>
                    {
                        if (token.IsCancellationRequested)
                            return;

                        using var stream = new MemoryStream(bytes);
                        Texture = Framework.Graphics.Textures.Texture.FromStream(renderer, stream);
                        Colour = Color4.White;
                    });
                }
                catch (OperationCanceledException)
                {
                }
                catch
                {
                }
            }, token);
        }

        protected override void Dispose(bool isDisposing)
        {
            loadCancellation?.Cancel();
            loadCancellation?.Dispose();
            base.Dispose(isDisposing);
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
