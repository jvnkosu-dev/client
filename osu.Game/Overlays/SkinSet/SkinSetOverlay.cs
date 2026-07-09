using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
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
        // Fits two SkinPreviewCard columns (110 + 10 + 110) with equal 15px side padding.
        public const float RIGHT_WIDTH = 260;

        private readonly Bindable<APIOnlineSkin?> skin = new Bindable<APIOnlineSkin?>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private SkinLookupCache skinLookupCache { get; set; } = null!;

        private IBindable<APIUser> apiUser = null!;

        private int? lastLookupId;
        private APIOnlineSkin? fallbackSkin;

        public SkinSetOverlay()
            : base(OverlayColourScheme.Blue)
        {
            SkinSetInfo info;
            SkinCommentsSection comments;

            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Children = new Drawable[]
                {
                    info = new SkinSetInfo(),
                    comments = new SkinCommentsSection(),
                }
            };

            Header.Skin.BindTo(skin);
            Header.Current.Value = SkinSetTabs.Info;
            info.Skin.BindTo(skin);
            comments.Skin.BindTo(skin);
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
            skinLookupCache.StoreSkins(new[] { skinData });
            lastLookupId = skinData.OnlineID;
            fallbackSkin = skinData;
            skin.Value = skinData;
            Show();
        }

        private void performFetch()
        {
            if (!api.IsLoggedIn || lastLookupId == null)
                return;

            int skinId = lastLookupId.Value;

            if (skinLookupCache.TryGetCached(skinId, out var cached))
            {
                skin.Value = cached;
                return;
            }

            Task.Run(async () =>
            {
                var result = await skinLookupCache.GetSkinAsync(skinId).ConfigureAwait(false);

                Schedule(() =>
                {
                    if (lastLookupId != skinId)
                        return;

                    skin.Value = result ?? fallbackSkin;
                });
            });
        }
    }
}
