using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public abstract partial class SkinCard : OsuClickableContainer
    {
        public const float TRANSITION_DURATION = BeatmapCard.TRANSITION_DURATION;
        public const float CORNER_RADIUS = BeatmapCard.CORNER_RADIUS;
        public const float WIDTH = BeatmapCard.WIDTH;

        /// <summary>
        /// Reserved height for the version line so metrics align across cards with and without a version.
        /// </summary>
        internal const float VERSION_LINE_HEIGHT = 14f;

        protected readonly BindableBool ExpandedBindable = new BindableBool();

        public IBindable<bool> Expanded => ExpandedBindable;

        public readonly APIOnlineSkin Skin;

        protected readonly Bindable<SkinFavouriteState> FavouriteState;

        protected abstract Drawable IdleContent { get; }
        protected abstract Drawable DownloadInProgressContent { get; }

        protected readonly SkinDownloadTracker DownloadTracker;

        protected SkinCard(APIOnlineSkin skin, bool allowExpansion = true)
            : base(HoverSampleSet.Button)
        {
            ExpandedBindable.Disabled = !allowExpansion;
            Skin = skin;
            FavouriteState = new Bindable<SkinFavouriteState>(new SkinFavouriteState(skin.HasFavourited, skin.FavouriteCount));
            DownloadTracker = new SkinDownloadTracker(skin);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(DownloadTracker);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DownloadTracker.State.BindValueChanged(_ => UpdateState());
            Expanded.BindValueChanged(_ => UpdateState(), true);
            FinishTransforms(true);
        }

        protected virtual BeatmapCardContent? ExpansionContent => null;

        protected override bool OnHover(HoverEvent e)
        {
            UpdateState();

            if (!ExpandedBindable.Disabled && ExpansionContent != null)
                ExpansionContent.ExpandAfterDelay();

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            UpdateState();
            base.OnHoverLost(e);
        }

        protected virtual void UpdateState()
        {
            bool showProgress = DownloadTracker.State.Value == DownloadState.Downloading || DownloadTracker.State.Value == DownloadState.Importing;

            IdleContent.FadeTo(showProgress ? 0 : 1, TRANSITION_DURATION, Easing.OutQuint);
            DownloadInProgressContent.FadeTo(showProgress ? 1 : 0, TRANSITION_DURATION, Easing.OutQuint);
        }

        protected void SetExpansionEnabled(bool enabled, BeatmapCardContent? cardContent = null)
        {
            ExpandedBindable.Disabled = !enabled;

            if (cardContent == null)
                return;

            if (!enabled)
            {
                cardContent.CancelExpand();
                ExpandedBindable.Value = false;
                return;
            }

            if (IsHovered || cardContent.IsHovered)
                cardContent.ExpandAfterDelay();
        }

        public static SkinCard Create(APIOnlineSkin skin, BeatmapCardSize size, bool allowExpansion = true)
        {
            switch (size)
            {
                case BeatmapCardSize.Nano:
                    return new SkinCardNano(skin);

                case BeatmapCardSize.Normal:
                    return new SkinCardNormal(skin, allowExpansion);

                case BeatmapCardSize.Extra:
                    return new SkinCardExtra(skin, allowExpansion);

                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, @"Unsupported card size");
            }
        }
    }
}
