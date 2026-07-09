using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinModifiedModesSelector : CompositeDrawable
    {
        public readonly Bindable<APIOnlineSkin?> Skin = new Bindable<APIOnlineSkin?>();

        private FillFlowContainer iconsFlow = null!;
        private OsuSpriteText madeForText = null!;

        public SkinModifiedModesSelector()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets, OverlayColourProvider colourProvider)
        {
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(8, 0),
                Children = new Drawable[]
                {
                    madeForText = new OsuSpriteText
                    {
                        Text = "made for",
                        Font = OsuFont.GetFont(size: 14),
                        Colour = colourProvider.Content2,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    iconsFlow = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(20, 0),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                },
            };

            Skin.BindValueChanged(s => updateModes(s.NewValue, rulesets), true);
        }

        private void updateModes(APIOnlineSkin? skin, RulesetStore rulesets)
        {
            iconsFlow.Clear();

            if (skin == null)
            {
                Hide();
                return;
            }

            var matchingRulesets = SkinModifiedModesHelper.GetMatchingRulesetsInOrder(skin.ModifiedModes, rulesets).ToArray();

            if (matchingRulesets.Length == 0)
            {
                Hide();
                return;
            }

            Show();

            foreach (var ruleset in matchingRulesets)
                iconsFlow.Add(new SkinModifiedModesIcon(ruleset));
        }
    }
}
