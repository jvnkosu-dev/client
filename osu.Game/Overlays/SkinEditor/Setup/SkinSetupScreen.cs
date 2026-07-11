// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.SkinEditor.Setup
{
    public partial class SkinSetupScreen : SkinEditorScreen
    {
        public const float COLUMN_WIDTH = 450;
        public const float SPACING = 28;
        public const float MAX_WIDTH = 2 * COLUMN_WIDTH + SPACING;

        private OsuScrollContainer scroll = null!;
        private FillFlowContainer flow = null!;

        public SkinSetupScreen()
            : base(SkinEditorScreenMode.Setup)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3,
                },
                scroll = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(15),
                    Child = flow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Spacing = new Vector2(25),
                        Children = new Drawable[]
                        {
                            createSection(new SkinMetadataSection()),
                            createSection(new SkinModeSection()),
                            createSection(new SkinResourcesSection()),
                        }
                    }
                }
            };
        }

        private static Drawable createSection(Drawable section) => section.With(s =>
        {
            s.Width = COLUMN_WIDTH;
            s.Anchor = Anchor.TopCentre;
            s.Origin = Anchor.TopCentre;
        });

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (scroll.DrawWidth > MAX_WIDTH)
            {
                flow.RelativeSizeAxes = Axes.None;
                flow.Width = MAX_WIDTH;
            }
            else
            {
                flow.RelativeSizeAxes = Axes.X;
                flow.Width = 1;
            }
        }

        protected override void PopOut()
        {
            // Commit any focused textboxes before leaving setup.
            GetContainingFocusManager()?.TriggerFocusContention(this);
            base.PopOut();
        }
    }
}
