// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public partial class Disclaimer : StartupScreen
    {
        private SpriteIcon icon;
        private Color4 iconColour;
        private LinkFlowContainer textFlow;
        private const float icon_y = -85;
        private const float icon_size = 30;

        private readonly OsuScreen nextScreen;

        private readonly Bindable<APIUser> currentUser = new Bindable<APIUser>();
        private FillFlowContainer fill;

        private readonly List<ITextPart> expendableText = new List<ITextPart>();

        public Disclaimer(OsuScreen nextScreen = null)
        {
            this.nextScreen = nextScreen;
            ValidForResume = false;
        }

        [Resolved]
        private IAPIProvider api { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.ExclamationTriangle,
                    Size = new Vector2(icon_size),
                    Y = icon_y,
                },
                fill = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Y = icon_y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        textFlow = new LinkFlowContainer
                        {
                            Width = 680,
                            AutoSizeAxes = Axes.Y,
                            TextAnchor = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Spacing = new Vector2(0, 2),
                        },
                    }
                },
            };

            textFlow.AddText("Disclaimer", t => t.Font = t.Font.With(Typeface.Torus, 30, FontWeight.Regular));


            static void formatRegular(SpriteText t) => t.Font = OsuFont.GetFont(size: 20, weight: FontWeight.Regular);
            static void formatSemiBold(SpriteText t) => t.Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold);

            textFlow.NewParagraph();
            textFlow.AddText("This is ", formatRegular);
            textFlow.AddText("jvnkosu!", formatSemiBold);
            textFlow.AddText(", an unofficial osu!(lazer) server based on official source code.", formatRegular);
            textFlow.NewParagraph();
            textFlow.AddText("We are not in any way affiliated with, or endorsed by, the osu! team.", formatSemiBold);
            textFlow.NewParagraph();
            textFlow.NewParagraph();
            textFlow.AddText("Thank you, and have fun!", formatRegular);

            iconColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            if (nextScreen != null)
                LoadComponentAsync(nextScreen);

            ((IBindable<APIUser>)currentUser).BindTo(api.LocalUser);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);

            // Once this screen has finished being displayed, we don't want to unnecessarily handle user change events.
            currentUser.UnbindAll();
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            // icon.RotateTo(10);
            icon.FadeOut();
            icon.ScaleTo(0.5f);

            icon.Delay(500).FadeIn(50).ScaleTo(1, 500, Easing.OutQuint);
            fill.MoveToOffset(new Vector2(0, 15), 0, Easing.OutQuart);

            using (BeginDelayedSequence(3000))
            {
                icon.MoveToY(icon_y, 0, Easing.InQuart)
                    .FadeColour(Color4.White, 160)
                    .Then()
                    .FadeColour(iconColour, 200, Easing.OutQuint);

                Schedule(() => expendableText.SelectMany(t => t.Drawables).ForEach(t =>
                {
                    t.FadeOut(100);
                    t.ScaleTo(new Vector2(0, 1), 100, Easing.OutQuart);
                }));
            }

            double delay = 500;
            foreach (var c in textFlow.Children)
                c.FadeTo(0.001f).Delay(delay += 20).FadeIn(500);

            this
                .FadeInFromZero(500)
                .Then(5500)
                .FadeOut(250)
                .ScaleTo(0.9f, 250, Easing.InQuint)
                .Finally(_ =>
                {
                    if (nextScreen != null)
                        this.Push(nextScreen);
                });
        }
    }
}
