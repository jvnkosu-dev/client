using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Skinning;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Submission
{
    public partial class SkinSubmissionModifiedModesControl : CompositeDrawable
    {
        public BindableList<string> ModifiedModes { get; } = new BindableList<string>();

        private readonly Dictionary<string, BindableBool> toggles = new Dictionary<string, BindableBool>();

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            var rulesetFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 5),
            };

            foreach (var ruleset in SkinModifiedModesHelper.GetLegacyRulesetsInOrder(rulesets))
            {
                var toggle = new BindableBool();
                toggles[ruleset.ShortName] = toggle;

                toggle.BindValueChanged(state => updateModifiedModes(ruleset.ShortName, state.NewValue));

                rulesetFlow.Add(new FormCheckBox
                {
                    RelativeSizeAxes = Axes.X,
                    Caption = ruleset.Name,
                    HintText = $"Skin contains changes for {ruleset.Name}",
                    Current = { BindTarget = toggle },
                });
            }

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    new FormFieldCaption
                    {
                        Caption = "Modified rulesets",
                        TooltipText = "Select which rulesets this skin modifies",
                    },
                    rulesetFlow,
                }
            };

            ModifiedModes.BindCollectionChanged((_, _) => syncTogglesFromList());
        }

        private void syncTogglesFromList()
        {
            foreach (var (shortName, toggle) in toggles)
            {
                bool shouldBeOn = ModifiedModes.Contains(shortName, StringComparer.OrdinalIgnoreCase);

                if (toggle.Value != shouldBeOn)
                    toggle.Value = shouldBeOn;
            }
        }

        private void updateModifiedModes(string shortName, bool enabled)
        {
            if (enabled)
            {
                if (!ModifiedModes.Contains(shortName, StringComparer.OrdinalIgnoreCase))
                    ModifiedModes.Add(shortName);
            }
            else
            {
                ModifiedModes.RemoveAll(m => string.Equals(m, shortName, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
