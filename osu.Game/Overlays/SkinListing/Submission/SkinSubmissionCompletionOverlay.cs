// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Mods;

namespace osu.Game.Overlays.SkinListing.Submission
{
    /// <summary>
    /// Invisible overlay used only to host the same ScreenFooter chrome as the submission wizard
    /// (Back + primary sheared button) after upload completes.
    /// </summary>
    public partial class SkinSubmissionCompletionOverlay : ShearedOverlayContainer
    {
        public Action? RequestDone { get; set; }
        public Action? RequestBackToEditor { get; set; }

        public SkinSubmissionCompletionOverlay()
            : base(OverlayColourScheme.Aquamarine)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Header.Alpha = 0;
            Header.Close = null;

            // Keep the progress panel visible behind Back / Done.
            foreach (var child in TopLevelContent)
            {
                if (child is Box dim)
                    dim.Alpha = 0;
            }
        }

        public override VisibilityContainer CreateFooterContent() => new CompletionFooterContent
        {
            RequestDone = () => RequestDone?.Invoke(),
        };

        public override bool OnBackButton()
        {
            RequestBackToEditor?.Invoke();
            return true;
        }

        protected override bool OnClick(ClickEvent e) => true;

        public partial class CompletionFooterContent : VisibilityContainer
        {
            public Action? RequestDone { get; set; }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                RelativeSizeAxes = Axes.Both;

                Padding = new MarginPadding { Right = OsuGame.SCREEN_EDGE_MARGIN };

                InternalChild = new ShearedButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = SkinSubmissionStrings.Done,
                    DarkerColour = colourProvider.Colour3,
                    LighterColour = colourProvider.Colour2,
                    Action = () => RequestDone?.Invoke(),
                };
            }

            protected override void PopIn() => this.FadeIn();

            protected override void PopOut() => this.Delay(400).FadeOut();
        }
    }
}
