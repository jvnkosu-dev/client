using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests;
using osuTK;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinSetStatistics : FillFlowContainer
    {
        private readonly Statistic downloads;
        private readonly Statistic favourites;

        public readonly Bindable<APIOnlineSkin?> Skin = new Bindable<APIOnlineSkin?>();

        public SkinSetStatistics()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Spacing = new Vector2(10f);

            Children = new Drawable[]
            {
                downloads = new Statistic(FontAwesome.Solid.Download)
                {
                    TooltipText = "Download count",
                },
                favourites = new Statistic(FontAwesome.Solid.Heart),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Skin.BindValueChanged(_ => updateDisplay(), true);
        }

        private void updateDisplay()
        {
            var skin = Skin.Value;

            if (skin == null)
            {
                downloads.Value = 0;
                favourites.Value = 0;
                favourites.TooltipText = "No favourites";
                return;
            }

            downloads.Value = skin.DownloadCount;
            favourites.Value = skin.FavouriteCount;
            favourites.TooltipText = skin.FavouriteCount > 0 ? "Favourited" : "No favourites";
        }

        private partial class Statistic : FillFlowContainer, IHasTooltip
        {
            private readonly OsuSpriteText text;

            private int value;

            public int Value
            {
                get => value;
                set
                {
                    this.value = value;
                    text.Text = Value.ToLocalisableString(@"N0");
                }
            }

            public Statistic(IconUsage icon)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(2f);

                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = icon,
                        Shadow = true,
                        Size = new Vector2(12),
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold, italics: true),
                    },
                };
            }

            public LocalisableString TooltipText { get; set; }
        }
    }
}
