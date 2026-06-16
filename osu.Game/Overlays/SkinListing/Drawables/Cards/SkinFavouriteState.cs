using osu.Game.Overlays.SkinListing.Drawables.Cards.Statistics;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    /// <summary>
    /// Stores the current favourite state of an online skin.
    /// Used to coordinate between <see cref="SkinFavouriteButton"/> and <see cref="SkinFavouritesStatistic"/>.
    /// </summary>
    public readonly struct SkinFavouriteState
    {
        public bool Favourited { get; }

        public int FavouriteCount { get; }

        public SkinFavouriteState(bool favourited, int favouriteCount)
        {
            Favourited = favourited;
            FavouriteCount = favouriteCount;
        }
    }
}
