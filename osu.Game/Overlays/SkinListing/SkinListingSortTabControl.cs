using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinListing
{
    public partial class SkinListingSortTabControl : OverlaySortTabControl<SortCriteria>
    {
        public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>(Overlays.SortDirection.Descending);

        private bool hasQuery;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Reset(false);
            Current.BindValueChanged(_ => SortDirection.Value = Overlays.SortDirection.Descending);
        }

        public void Reset(bool hasQuery)
        {
            this.hasQuery = hasQuery;

            TabControl.Clear();
            TabControl.AddItem(SortCriteria.Name);
            TabControl.AddItem(SortCriteria.Creator);
            TabControl.AddItem(SortCriteria.Updated);
            TabControl.AddItem(SortCriteria.Favourites);

            if (hasQuery)
                TabControl.AddItem(SortCriteria.Relevance);

            Current.Value = hasQuery ? SortCriteria.Relevance : SortCriteria.Updated;
            SortDirection.Value = Overlays.SortDirection.Descending;
            TabControl.Current.TriggerChange();
        }

        protected override SortTabControl CreateControl() => new SkinSortTabControl
        {
            SortDirection = { BindTarget = SortDirection },
        };

        private partial class SkinSortTabControl : SortTabControl
        {
            protected override bool AddEnumEntriesAutomatically => false;

            public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>();

            protected override TabItem<SortCriteria> CreateTabItem(SortCriteria value) => new SkinSortTabItem(value)
            {
                SortDirection = { BindTarget = SortDirection }
            };
        }

        private partial class SkinSortTabItem : SortTabItem
        {
            public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>();

            public SkinSortTabItem(SortCriteria value)
                : base(value)
            {
            }

            protected override TabButton CreateTabButton(SortCriteria value) => new SkinTabButton(value)
            {
                Active = { BindTarget = Active },
                SortDirection = { BindTarget = SortDirection }
            };
        }

        public partial class SkinTabButton : TabButton
        {
            public readonly Bindable<SortDirection> SortDirection = new Bindable<SortDirection>();

            protected override Color4 ContentColour
            {
                set
                {
                    base.ContentColour = value;
                    icon.Colour = value;
                }
            }

            private readonly SpriteIcon icon;

            public SkinTabButton(SortCriteria value)
                : base(value)
            {
                Add(icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AlwaysPresent = true,
                    Alpha = 0,
                    Size = new Vector2(6),
                    Icon = FontAwesome.Solid.CaretDown,
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                SortDirection.BindValueChanged(direction =>
                {
                    icon.ScaleTo(direction.NewValue == Overlays.SortDirection.Ascending && Active.Value ? new Vector2(1f, -1f) : Vector2.One, 300, Easing.OutQuint);
                }, true);
            }

            protected override void UpdateState()
            {
                base.UpdateState();
                icon.FadeTo(Active.Value || IsHovered ? 1 : 0, 200, Easing.OutQuint);
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (Active.Value)
                    SortDirection.Value = SortDirection.Value == Overlays.SortDirection.Ascending ? Overlays.SortDirection.Descending : Overlays.SortDirection.Ascending;

                return base.OnClick(e);
            }
        }
    }
}
