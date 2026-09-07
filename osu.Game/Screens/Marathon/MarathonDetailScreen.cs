// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay;
using osuTK;

namespace osu.Game.Screens.Marathon
{
    /// <summary>
    /// Read-only look at a marathon's map pool. Play lives on this screen as a
    /// regular sheared button next to the pool header, not in the footer.
    /// Clicking the listing card must not start the run.
    /// </summary>
    public partial class MarathonDetailScreen : OsuScreen
    {
        public override string Title => Marathon.Name;

        public override bool ShowFooter => true;

        public readonly APIMarathon Marathon;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        private OnlinePlayScreenWaveContainer waves = null!;
        private MarathonMapPoolPlaylist playlist = null!;
        private ShearedButton playButton = null!;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBlack(WaveContainer.APPEAR_DURATION);

        public MarathonDetailScreen(APIMarathon marathon)
        {
            Marathon = marathon;
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            var items = MarathonMaps.ToPlaylistItems(Marathon.Maps);

            playButton = new ShearedButton
            {
                Width = 110,
                Height = 40,
                Text = MarathonStrings.PlayButton,
                TextSize = 16,
                Enabled = { Value = Marathon.Maps.Length > 0 },
                Action = startPlay,
            };

            playButton.DarkerColour = colours.Lime1.Darken(0.6f);
            playButton.LighterColour = colours.Lime1;
            playButton.TextColour = Colour4.Black;

            InternalChild = waves = new OnlinePlayScreenWaveContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5,
                    },
                    new Header(Marathon.Name, null),
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Horizontal = WaveOverlayContainer.WIDTH_PADDING,
                            Top = Header.HEIGHT,
                            Bottom = ScreenFooter.HEIGHT + 20,
                        },
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 10,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background3,
                                },
                                new GridContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(20),
                                    RowDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(),
                                    },
                                    Content = new[]
                                    {
                                        new Drawable[]
                                        {
                                            new TruncatingSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Text = string.IsNullOrEmpty(Marathon.Description)
                                                    ? MarathonStrings.MapPoolHeader
                                                    : Marathon.Description,
                                                Font = OsuFont.GetFont(size: 18),
                                                Colour = colourProvider.Content1,
                                            },
                                        },
                                        new Drawable[]
                                        {
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Margin = new MarginPadding { Vertical = 10 },
                                                ColumnDimensions = new[]
                                                {
                                                    new Dimension(),
                                                    new Dimension(GridSizeMode.AutoSize),
                                                },
                                                RowDimensions = new[]
                                                {
                                                    new Dimension(GridSizeMode.AutoSize),
                                                },
                                                Content = new[]
                                                {
                                                    new Drawable[]
                                                    {
                                                        new FillFlowContainer
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Direction = FillDirection.Horizontal,
                                                            Spacing = new Vector2(10),
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Children = new Drawable[]
                                                            {
                                                                credit(),
                                                                starRange(),
                                                                rankRange(),
                                                            }
                                                        },
                                                        playButton.With(d =>
                                                        {
                                                            d.Anchor = Anchor.CentreRight;
                                                            d.Origin = Anchor.CentreRight;
                                                        }),
                                                    }
                                                }
                                            }
                                        },
                                        new Drawable[]
                                        {
                                            new SectionHeader(MarathonStrings.MapPoolHeader)
                                            {
                                                DetailsText = { Value = Marathon.Maps.Length > 0 ? Marathon.Maps.Length.ToString() : string.Empty },
                                            },
                                        },
                                        new Drawable[]
                                        {
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Children = new Drawable[]
                                                {
                                                    playlist = new MarathonMapPoolPlaylist
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        AllowDeletion = false,
                                                        AllowReordering = false,
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Text = MarathonStrings.Placeholder,
                                                        Font = OsuFont.GetFont(size: 20),
                                                        Colour = colourProvider.Content2,
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        Alpha = items.Length == 0 ? 1 : 0,
                                                    },
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        }
                    }
                }
            };

            playlist.Items.AddRange(items);
        }

        private void startPlay()
        {
            if (!this.IsCurrentScreen() || Marathon.Maps.Length == 0)
                return;

            this.Push(new MarathonPlayScreen(Marathon));
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            this.FadeIn();
            waves.Show();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            this.FadeIn(250);
            this.ScaleTo(1, 250, Easing.OutSine);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);
            this.ScaleTo(1.1f, 250, Easing.InSine);
            this.FadeOut(250);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            waves.Hide();
            this.Delay(WaveContainer.DISAPPEAR_DURATION).FadeOut();
            return base.OnExiting(e);
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);
            logo.Delay(WaveContainer.DISAPPEAR_DURATION / 2).FadeOut();
        }

        private Drawable credit()
        {
            string owner = string.IsNullOrEmpty(Marathon.OwnerUsername)
                ? $"{Marathon.Maps.Length} maps"
                : $"{Marathon.OwnerUsername} · {Marathon.Maps.Length} maps";

            if (Marathon.HasSourceCredit)
                owner = $"{owner} · originally {Marathon.SourceOwnerUsername}";

            return new OsuSpriteText
            {
                Text = owner,
                Font = OsuFont.GetFont(size: 14),
                Colour = colourProvider.Content2,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            };
        }

        private Drawable starRange()
        {
            var display = new MarathonStarRangeDisplay();

            if (MarathonStarRangeDisplay.TryGetRange(Marathon.Maps.Select(m => m.StarRating), out double min, out double max))
                display.SetRange(min, max);
            else
                display.ClearRange();

            return display;
        }

        private Drawable rankRange()
        {
            var display = new MarathonRankRangeDisplay();
            display.SetFrom(Marathon);
            return display;
        }
    }
}
