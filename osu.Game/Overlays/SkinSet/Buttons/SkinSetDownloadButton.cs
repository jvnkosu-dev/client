using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.SkinListing.Drawables.Cards;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinSet.Buttons
{
    public partial class SkinSetDownloadButton : CompositeDrawable
    {
        private readonly APIOnlineSkin skin;
        private readonly SkinDownloadTracker downloadTracker;

        private ShakeContainer shakeContainer = null!;
        private DownloadButton button = null!;

        public SkinSetDownloadButton(APIOnlineSkin skin, SkinDownloadTracker downloadTracker)
        {
            this.skin = skin;
            this.downloadTracker = downloadTracker;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame game, SkinDownloader skinDownloader)
        {
            InternalChild = shakeContainer = new ShakeContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = button = new DownloadButton
                {
                    RelativeSizeAxes = Axes.Both,
                    State = { BindTarget = downloadTracker.State },
                },
            };

            button.Action = () =>
            {
                switch (downloadTracker.State.Value)
                {
                    case DownloadState.Downloading:
                    case DownloadState.Importing:
                        shakeContainer.Shake();
                        break;

                    case DownloadState.LocallyAvailable:
                        game.PresentSkinFromListing(skin);
                        break;

                    default:
                        skinDownloader.DownloadAndImport(skin);
                        break;
                }
            };

            downloadTracker.State.BindValueChanged(state =>
            {
                if (state.NewValue == DownloadState.LocallyAvailable)
                {
                    button.Enabled.Value = true;
                    button.TooltipText = "Go to skin";
                }
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            FinishTransforms(true);
        }
    }
}
