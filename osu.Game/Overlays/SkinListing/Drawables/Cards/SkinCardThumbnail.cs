using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class SkinCardThumbnail : Container
    {
        public BindableBool Dimmed { get; } = new BindableBool();

        public new MarginPadding Padding
        {
            get => foreground.Padding;
            set => foreground.Padding = value;
        }

        private readonly Box background;
        private readonly Container foreground;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public SkinCardThumbnail(string? thumbnailUrl)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f),
                },
                new OnlineSkinSprite(thumbnailUrl)
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                foreground = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colourProvider.Background6;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Dimmed.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            bool shouldDim = Dimmed.Value;
            background.FadeColour(colourProvider.Background6.Opacity(shouldDim ? 0.6f : 0f), BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }
    }

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
