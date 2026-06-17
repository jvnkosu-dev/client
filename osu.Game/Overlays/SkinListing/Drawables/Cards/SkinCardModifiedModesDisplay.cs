using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    /// <summary>
    /// Displays modified mode icons using the same visual style as <see cref="Beatmaps.Drawables.DifficultySpectrumDisplay"/> on beatmap listing cards.
    /// </summary>
    public partial class SkinCardModifiedModesDisplay : CompositeDrawable
    {
        private readonly APIOnlineSkin skin;

        public SkinCardModifiedModesDisplay(APIOnlineSkin skin)
        {
            this.skin = skin;
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            var matchingRulesets = SkinModifiedModesHelper.GetMatchingRulesetsInOrder(skin.ModifiedModes, rulesets).ToArray();

            if (matchingRulesets.Length == 0)
            {
                Alpha = 0;
                Height = 0;
                return;
            }

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(4, 0),
                Direction = FillDirection.Horizontal,
                ChildrenEnumerable = matchingRulesets.Select(r => new RulesetIcon(r)),
            };
        }

        private partial class RulesetIcon : CompositeDrawable, IHasTooltip
        {
            public LocalisableString TooltipText => ruleset.Name;

            private readonly RulesetInfo ruleset;

            public RulesetIcon(RulesetInfo ruleset)
            {
                this.ruleset = ruleset;
                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(RulesetStore rulesets)
            {
                var icon = rulesets.GetRuleset(ruleset.OnlineID)?.CreateInstance().CreateIcon()
                           ?? new SpriteIcon { Icon = FontAwesome.Regular.QuestionCircle };

                InternalChild = icon.With(i =>
                {
                    i.Size = new Vector2(14);
                    i.Anchor = i.Origin = Anchor.Centre;
                });
            }

            public override bool PropagatePositionalInputSubTree => false;
        }
    }
}
