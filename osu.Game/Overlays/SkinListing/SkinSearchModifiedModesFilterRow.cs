using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinListing
{
    public partial class SkinSearchModifiedModesFilterRow : BeatmapSearchMultipleSelectionFilterRow<SkinListingModifiedMode>
    {
        public SkinSearchModifiedModesFilterRow()
            : base(BeatmapsStrings.ListingSearchFiltersMode)
        {
        }

        protected override MultipleSelectionFilter CreateMultipleSelectionFilter() => new ModifiedModesFilter();

        private partial class ModifiedModesFilter : MultipleSelectionFilter
        {
            protected override MultipleSelectionFilterTabItem CreateTabItem(SkinListingModifiedMode value) =>
                new ModifiedModeTabItem(value);
        }

        private partial class ModifiedModeTabItem : MultipleSelectionFilterTabItem
        {
            public ModifiedModeTabItem(SkinListingModifiedMode value)
                : base(value)
            {
            }

            protected override LocalisableString LabelFor(SkinListingModifiedMode value) =>
                SkinModifiedModesHelper.ToShortName(value);

            [BackgroundDependencyLoader]
            private void load(RulesetStore rulesets)
            {
                string shortName = SkinModifiedModesHelper.ToShortName(Value);
                Text.Text = rulesets.GetRuleset(shortName)?.Name ?? shortName;
            }
        }
    }
}
