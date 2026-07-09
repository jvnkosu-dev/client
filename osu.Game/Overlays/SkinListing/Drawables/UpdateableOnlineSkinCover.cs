// Copyright (c) jvnkosu! team, MIT license
// See the LICENCE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.SkinListing.Drawables.Cards;
using osu.Game.Skinning.Preview;

namespace osu.Game.Overlays.SkinListing.Drawables
{
    public partial class UpdateableOnlineSkinCover : ModelBackedDrawable<APIOnlineSkin?>
    {
        public APIOnlineSkin? Skin
        {
            get => Model;
            set => Model = value;
        }

        public new bool Masking
        {
            get => base.Masking;
            set => base.Masking = value;
        }

        protected override double LoadDelay { get; }

        private readonly double timeBeforeUnload;

        protected override double TransformDuration => 400;

        [Resolved]
        private OnlineSkinPreviewProvider previewProvider { get; set; } = null!;

        public UpdateableOnlineSkinCover(double timeBeforeLoad = 500, double timeBeforeUnload = 1000)
            : base((left, right) => left?.OnlineID == right?.OnlineID)
        {
            LoadDelay = timeBeforeLoad;
            this.timeBeforeUnload = timeBeforeUnload;

            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(0.2f),
            };
        }

        protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Func<Drawable> createContentFunc, double timeBeforeLoad)
            => new SkinDelayedLoadUnloadWrapper(createContentFunc, previewProvider, Model?.OnlineID ?? 0, timeBeforeLoad, timeBeforeUnload)
            {
                RelativeSizeAxes = Axes.Both,
            };

        protected override Drawable? CreateDrawable(APIOnlineSkin? model)
        {
            if (model == null)
                return null;

            string? url = model.GetThumbnailRequestUrl();

            if (string.IsNullOrEmpty(url))
                return null;

            return new OnlineSkinSprite(url)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
            };
        }
    }
}
