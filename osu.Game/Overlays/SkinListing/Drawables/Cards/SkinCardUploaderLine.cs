using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class SkinCardUploaderLine : LinkFlowContainer
    {
        private readonly APIOnlineSkin skin;

        public SkinCardUploaderLine(APIOnlineSkin skin)
            : base(s =>
            {
                s.Shadow = false;
                s.Font = OsuFont.GetFont(size: 11f, weight: FontWeight.SemiBold);
            })
        {
            this.skin = skin;
            AutoSizeAxes = Axes.Both;
            Margin = new MarginPadding { Top = 1 };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            string? uploader = skin.GetUploaderDisplayName();

            if (string.IsNullOrWhiteSpace(uploader))
            {
                Hide();
                return;
            }

            AddText("uploaded by ", t => t.Colour = colourProvider.Content2);
            AddUserLink(new APIUser { Username = uploader });
        }
    }
}
