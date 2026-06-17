using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.SkinListing
{
    public enum SortCriteria
    {
        Name,
        Creator,
        Updated,
        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ListingSearchSortingFavourites))]
        Favourites,
        Relevance,
    }
}
