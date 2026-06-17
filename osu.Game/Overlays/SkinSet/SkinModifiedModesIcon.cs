using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinModifiedModesIcon : CompositeDrawable, IHasTooltip
    {
        public LocalisableString TooltipText => ruleset.Name;

        private readonly RulesetInfo ruleset;

        public SkinModifiedModesIcon(RulesetInfo ruleset)
        {
            this.ruleset = ruleset;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChild = new ConstrainedIconContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(20f),
                Icon = ruleset.CreateInstance().CreateIcon(),
                Colour = colourProvider.Highlight1,
            };
        }

        public override bool PropagatePositionalInputSubTree => false;
    }
}
