// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Overlays.Comments;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinSet
{
    /// <summary>
    /// Placeholder comments section for skins. Mirrors the beatmap set layout but remains
    /// inactive until skin comments are implemented server-side.
    /// </summary>
    public partial class SkinCommentsSection : FillFlowContainer
    {
        public readonly Bindable<APIOnlineSkin?> Skin = new Bindable<APIOnlineSkin?>();

        public SkinCommentsSection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(0, 10);

            Children = new Drawable[]
            {
                // Sits on the page background, not inside the comments card.
                new SkinCommentsUnavailableBanner(),
                new BeatmapSetLayoutSection
                {
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new CommentsContainer
                            {
                                Alpha = 0.45f,
                            },
                            // Absorb clicks so the grayed-out editor/header cannot be interacted with.
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black,
                                Alpha = 0.001f,
                            },
                        },
                    },
                },
            };

            Skin.BindValueChanged(skin =>
            {
                if (skin.NewValue?.OnlineID > 0)
                    Show();
                else
                    Hide();
            }, true);
        }

        /// <summary>
        /// Matches <see cref="Settings.SettingsNote"/> warning styling (yellow plaque + dark text),
        /// with an exclamation triangle icon.
        /// </summary>
        private partial class SkinCommentsUnavailableBanner : CompositeDrawable
        {
            public SkinCommentsUnavailableBanner()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding
                {
                    Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING,
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, OverlayColourProvider colourProvider)
            {
                InternalChild = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    CornerRadius = 5,
                    CornerExponent = 2.5f,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Orange1,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(8, 0),
                            Padding = new MarginPadding(8),
                            Children = new Drawable[]
                            {
                                new SpriteIcon
                                {
                                    Icon = FontAwesome.Solid.ExclamationTriangle,
                                    Size = new Vector2(14),
                                    Colour = colourProvider.Background5,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                                new OsuSpriteText
                                {
                                    Text = "Comments for skins are not implemented yet.",
                                    Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                                    Colour = colourProvider.Background5,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                            },
                        },
                    },
                };
            }
        }
    }
}
