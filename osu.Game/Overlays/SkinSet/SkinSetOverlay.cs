using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinSetOverlay : OnlineOverlay<SkinSetHeader>
    {
        public const float Y_PADDING = 25;
        public const float RIGHT_WIDTH = 275;

        private readonly Bindable<APIOnlineSkin?> skin = new Bindable<APIOnlineSkin?>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private IBindable<APIUser> apiUser = null!;

        private int? lastLookupId;
        private APIOnlineSkin? fallbackSkin;

        public SkinSetOverlay()
            : base(OverlayColourScheme.Blue)
        {
            SkinSetInfo info;

            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Children = new Drawable[]
                {
                    info = new SkinSetInfo(),
                }
            };

            Header.Skin.BindTo(skin);
            Header.Current.Value = SkinSetTabs.Info;
            info.Skin.BindTo(skin);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            apiUser = api.LocalUser.GetBoundCopy();
            apiUser.BindValueChanged(_ => Schedule(performFetch));
        }

        protected override SkinSetHeader CreateHeader() => new SkinSetHeader();

        protected override Color4 BackgroundColour => ColourProvider.Background6;

        protected override void PopOutComplete()
        {
            base.PopOutComplete();
            skin.Value = null;
            lastLookupId = null;
            fallbackSkin = null;
        }

        public void FetchAndShowSkin(int skinId, APIOnlineSkin? fallback = null)
        {
            lastLookupId = skinId;
            fallbackSkin = fallback;
            skin.Value = null;

            performFetch();
            Show();
        }

        /// <summary>
        /// Show an already fully-populated skin entry.
        /// </summary>
        public void ShowSkin(APIOnlineSkin skinData)
        {
            lastLookupId = skinData.OnlineID;
            fallbackSkin = skinData;
            skin.Value = skinData;
            Show();
            performFetch();
        }

        private void performFetch()
        {
            if (!api.IsLoggedIn || lastLookupId == null)
                return;

            fetchFromListing(lastLookupId.Value);
        }

        private void fetchFromListing(int skinId)
        {
            var req = new GetSkinsRequest();
            req.Success += skins =>
            {
                var found = skins.FirstOrDefault(s => s.OnlineID == skinId);

                if (found != null)
                    skin.Value = found;
                else if (fallbackSkin != null)
                    skin.Value = fallbackSkin;
            };
            req.Failure += _ =>
            {
                if (fallbackSkin != null)
                    skin.Value = fallbackSkin;
            };
            API.Queue(req);
        }
    }
}
