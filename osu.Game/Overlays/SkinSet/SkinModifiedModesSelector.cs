using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinModifiedModesSelector : CompositeDrawable
    {
        public readonly Bindable<APIOnlineSkin?> Skin = new Bindable<APIOnlineSkin?>();

        private FillFlowContainer flow = null!;

        public SkinModifiedModesSelector()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            InternalChild = flow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(20, 0),
            };

            Skin.BindValueChanged(s => updateModes(s.NewValue, rulesets), true);
        }

        private void updateModes(APIOnlineSkin? skin, RulesetStore rulesets)
        {
            flow.Clear();

            if (skin == null)
                return;

            foreach (var ruleset in SkinModifiedModesHelper.GetMatchingRulesetsInOrder(skin.ModifiedModes, rulesets))
                flow.Add(new SkinModifiedModesIcon(ruleset));
        }
    }
}
