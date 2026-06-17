using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Online;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class GoToSkinButton : BeatmapCardIconButton
    {
        public Bindable<DownloadState> State { get; } = new Bindable<DownloadState>();

        private readonly APIOnlineSkin skin;

        [Resolved]
        private SkinManager skinManager { get; set; } = null!;

        [Resolved]
        private SkinDownloader skinDownloader { get; set; } = null!;

        [Resolved]
        private SettingsOverlay settings { get; set; } = null!;

        public GoToSkinButton(APIOnlineSkin skin)
        {
            this.skin = skin;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Icon.Icon = FontAwesome.Solid.AngleDoubleRight;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Action = presentSkin;
            State.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void presentSkin()
        {
            var installedSkin = skinDownloader.GetInstalledSkin(skin);

            if (installedSkin == null)
                return;

            var databasedSkin = skinManager.Query(s => s.ID == installedSkin.ID);

            if (databasedSkin == null)
                return;

            skinManager.CurrentSkinInfo.Value = databasedSkin;
            settings.ShowSkinSection();
        }

        private void updateState()
        {
            bool available = State.Value == DownloadState.LocallyAvailable;
            Enabled.Value = available;

            if (available)
            {
                TooltipText = "Go to skin";
                this.FadeTo(1f, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            }
            else
            {
                TooltipText = string.Empty;
                this.FadeTo(0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            }
        }
    }
}
