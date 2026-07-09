// Copyright (c) jvnkosu! team, MIT license
// See the LICENCE file in the repository root for full license text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning.Preview;

namespace osu.Game.Overlays.SkinListing.Drawables
{
    /// <summary>
    /// A <see cref="DelayedLoadUnloadWrapper"/> which notifies <see cref="OnlineSkinPreviewProvider"/> when skin thumbnail content is loaded or unloaded.
    /// </summary>
    public partial class SkinDelayedLoadUnloadWrapper : DelayedLoadUnloadWrapper
    {
        private readonly int skinOnlineId;
        private readonly OnlineSkinPreviewProvider previewProvider;
        private bool loadCompleted;

        public SkinDelayedLoadUnloadWrapper(
            Func<Drawable> createContentFunction,
            OnlineSkinPreviewProvider previewProvider,
            int skinOnlineId,
            double timeBeforeLoad = 500,
            double timeBeforeUnload = 1000)
            : base(createContentFunction, timeBeforeLoad, timeBeforeUnload)
        {
            this.previewProvider = previewProvider;
            this.skinOnlineId = skinOnlineId;

            DelayedLoadComplete += _ => loadCompleted = true;
        }

        protected override void EndDelayedLoad(Drawable content)
        {
            base.EndDelayedLoad(content);

            if (skinOnlineId > 0)
                previewProvider.NotifyThumbnailLoaded(skinOnlineId);
        }

        protected override void Update()
        {
            base.Update();

            if (!loadCompleted || skinOnlineId <= 0)
                return;

            if (Content != null || DelayedLoadTriggered)
                return;

            loadCompleted = false;
            previewProvider.NotifyThumbnailUnloaded(skinOnlineId);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && loadCompleted && skinOnlineId > 0)
                previewProvider.NotifyThumbnailUnloaded(skinOnlineId);

            base.Dispose(isDisposing);
        }
    }
}
