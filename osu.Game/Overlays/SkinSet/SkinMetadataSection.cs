using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinSet
{
    public abstract partial class SkinMetadataSection : Container
    {
        private readonly FillFlowContainer textContainer;
        private TextFlowContainer? textFlow;

        protected const float TRANSITION_DURATION = 250;

        protected SkinMetadataSection(string label)
        {
            Alpha = 0;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = textContainer = new FillFlowContainer
            {
                Alpha = 0,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Margin = new MarginPadding { Top = 15 },
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = new OsuSpriteText
                        {
                            Text = label,
                            Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14),
                        },
                    },
                }
            };
        }

        public virtual string Metadata
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.FadeOut(TRANSITION_DURATION);
                    return;
                }

                this.FadeIn(TRANSITION_DURATION);
                setTextFlowAsync(value);
            }
        }

        private void setTextFlowAsync(string metadata)
        {
            LoadComponentAsync(new LinkFlowContainer(s => s.Font = s.Font.With(size: 14))
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Colour = Color4.White.Opacity(0.75f),
            }, loaded =>
            {
                textFlow?.Expire();
                AddMetadata(metadata, loaded);
                textContainer.Add(textFlow = loaded);
                textContainer.FadeIn(TRANSITION_DURATION);
            });
        }

        protected abstract void AddMetadata(string metadata, LinkFlowContainer loaded);
    }
}
