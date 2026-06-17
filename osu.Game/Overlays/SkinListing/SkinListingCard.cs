// Copyright (c) jvnkosu! team, MIT license
// See the LICENCE file in the repository root for full license text

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Input.Events;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Skinning;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinListing
{
    public partial class SkinListingCard : CompositeDrawable
    {
        private readonly APIOnlineSkin skin;
        private Container borderContainer = null!;
        private Box background = null!;
        private RoundedButton downloadButton = null!;

        [Resolved]
        private SkinDownloader skinDownloader { get; set; } = null!;

        public SkinListingCard(APIOnlineSkin skin)
        {
            this.skin = skin;
            Size = new Vector2(260, 300);
            Margin = new MarginPadding(10);
            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                borderContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 14,
                    BorderColour = colours.Blue,
                    BorderThickness = 0,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Gray2,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 150,
                                    Children = new Drawable[]
                                    {
                                        new DelayedLoadWrapper(new OnlineImage(skin.ThumbnailUrl))
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = ColourInfo.GradientVertical(
                                                colours.Gray2.Opacity(0f),
                                                colours.Gray2.Opacity(1f)
                                            )
                                        }
                                    }
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5),
                                    Padding = new MarginPadding { Left = 15, Right = 15, Bottom = 15 },
                                    Children = new Drawable[]
                                    {
                                        new TruncatingSpriteText
                                        {
                                            Text = skin.Name,
                                            Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold),
                                            RelativeSizeAxes = Axes.X,
                                        },
                                        new TruncatingSpriteText
                                        {
                                            Text = $"created by {skin.Creator}",
                                            Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                                            Colour = colours.BlueLight,
                                            RelativeSizeAxes = Axes.X,
                                        },
                                        new TruncatingSpriteText
                                        {
                                            Text = string.IsNullOrEmpty(skin.Description) ? "No description given!" : skin.Description,
                                            Font = OsuFont.GetFont(size: 12),
                                            Colour = colours.Gray8,
                                            RelativeSizeAxes = Axes.X,
                                            Margin = new MarginPadding { Bottom = 10 }
                                        },
                                        downloadButton = new RoundedButton
                                        {
                                            Text = "Download",
                                            RelativeSizeAxes = Axes.X,
                                            Height = 35,
                                            Action = () => skinDownloader.DownloadAndImport(skin)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            updateStatus(colours);
        }

        private void updateStatus(OsuColour colours)
        {
            var installedSkins = skinDownloader.IsInstalled(skin);
            if (installedSkins)
            {
                // For now just disable the button; a hash check could enable an "Update" action later.
                downloadButton.Enabled.Value = false;
                downloadButton.Text = "Installed";
                downloadButton.BackgroundColour = colours.Gray4;
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            borderContainer.TransformTo(nameof(borderContainer.BorderThickness), 4f, 200, Easing.OutQuint);
            background.FadeColour(OsuColour.Gray(0.2f), 200, Easing.OutQuint);
            this.ScaleTo(1.02f, 200, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            borderContainer.TransformTo(nameof(borderContainer.BorderThickness), 0f, 200, Easing.OutQuint);
            background.FadeColour(OsuColour.Gray(0.125f), 200, Easing.OutQuint);
            this.ScaleTo(1f, 200, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }

    public partial class OnlineImage : Sprite
    {
        private readonly string url;

        [Resolved]
        private IRenderer renderer { get; set; } = null!;

        public OnlineImage(string url)
        {
            this.url = url;
            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fill;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Gray5;

            if (string.IsNullOrEmpty(url)) return;

            Task.Run(async () =>
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(url).ConfigureAwait(false);
                        Schedule(() =>
                        {
                            using (var stream = new MemoryStream(bytes))
                            {
                                Texture = Texture.FromStream(renderer, stream);
                                Colour = Color4.White;
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load skin cover: {ex.Message}");
                }
            });
        }
    }
}
