using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Online;
using osu.Game.Online.API.Requests;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class SkinDownloadButton : BeatmapCardIconButton
    {
        public Bindable<DownloadState> State { get; } = new Bindable<DownloadState>();

        private readonly APIOnlineSkin skin;

        [Resolved]
        private SkinDownloader skinDownloader { get; set; } = null!;

        public SkinDownloadButton(APIOnlineSkin skin)
        {
            this.skin = skin;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Icon.Icon = FontAwesome.Solid.Download;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            State.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            switch (State.Value)
            {
                case DownloadState.Unknown:
                    Action = null;
                    TooltipText = string.Empty;
                    break;

                case DownloadState.Downloading:
                case DownloadState.Importing:
                    Action = null;
                    TooltipText = string.Empty;
                    SetLoading(true);
                    break;

                case DownloadState.LocallyAvailable:
                    Action = null;
                    TooltipText = string.Empty;
                    this.FadeOut(BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
                    break;

                case DownloadState.NotDownloaded:
                    Action = () => skinDownloader.DownloadAndImport(skin);
                    this.FadeIn(BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
                    SetLoading(false);
                    TooltipText = "Download skin";
                    break;

                default:
                    throw new InvalidOperationException($"Unknown {nameof(DownloadState)} specified.");
            }
        }
    }
}
