using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.SkinListing
{
    public partial class SkinListingHeader : OverlayHeader
    {
        public SkinListingFilterControl FilterControl { get; private set; } = null!;

        protected override OverlayTitle CreateTitle() => new SkinListingTitle();

        protected override Drawable CreateContent() => FilterControl = new SkinListingFilterControl();

        private partial class SkinListingTitle : OverlayTitle
        {
            public SkinListingTitle()
            {
                Title = "skin listing";
                Description = "browse for new skins";
                Icon = OsuIcon.SkinB;
            }
        }
    }
}
