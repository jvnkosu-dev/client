using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class SkinFavouriteButton : BeatmapCardIconButton
    {
        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Icon.Icon = FontAwesome.Regular.Heart;
            TooltipText = "Add to favourites";
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Action = () => notifications.Post(new SimpleNotification { Text = "this feature not implemented yet!" });
        }
    }
}
