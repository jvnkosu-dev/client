using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinSetHeader : TabControlOverlayHeader<SkinSetTabs>
    {
        public readonly Bindable<APIOnlineSkin?> Skin = new Bindable<APIOnlineSkin?>();

        public SkinSetHeaderContent HeaderContent { get; private set; } = null!;

        public SkinSetHeader()
        {
            Masking = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0.25f),
                Type = EdgeEffectType.Shadow,
                Radius = 3,
                Offset = new Vector2(0f, 1f),
            };
        }

        protected override Drawable CreateContent() => HeaderContent = new SkinSetHeaderContent
        {
            Skin = { BindTarget = Skin }
        };

        protected override Drawable CreateTabControlContent() => new SkinModifiedModesSelector
        {
            Skin = { BindTarget = Skin }
        };

        protected override OverlayTitle CreateTitle() => new SkinSetHeaderTitle();

        private partial class SkinSetHeaderTitle : OverlayTitle
        {
            public SkinSetHeaderTitle()
            {
                Title = "skin info";
                Description = "skin details";
                Icon = OsuIcon.SkinB;
            }
        }
    }

    public enum SkinSetTabs
    {
        [LocalisableDescription(typeof(LayoutStrings), nameof(LayoutStrings.HeaderBeatmapsetsShow))]
        Info,
    }
}
