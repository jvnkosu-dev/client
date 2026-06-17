using System;
using System.Linq;

using osu.Framework.Allocation;

using osu.Framework.Bindables;

using osu.Framework.Graphics;

using osu.Framework.Graphics.Containers;

using osu.Framework.Graphics.UserInterface;

using osu.Framework.Input.Events;

using osu.Framework.Localisation;

using osu.Game.Overlays.BeatmapListing;

using osu.Game.Skinning;

using osuTK;



namespace osu.Game.Overlays.SkinListing

{

    public partial class SkinSearchEngineTypeFilterRow : BeatmapSearchFilterRow<SkinEngineType?>

    {

        public SkinSearchEngineTypeFilterRow()

            : base("Skin Type")

        {

        }



        protected override Drawable CreateFilter() => new EngineTypeFilter();



        private partial class EngineTypeFilter : FillFlowContainer<EngineTypeTabItem>, IHasCurrentValue<SkinEngineType?>

        {

            private readonly BindableWithCurrent<SkinEngineType?> current = new BindableWithCurrent<SkinEngineType?>();



            public Bindable<SkinEngineType?> Current

            {

                get => current.Current;

                set => current.Current = value;

            }



            [BackgroundDependencyLoader]

            private void load()

            {

                RelativeSizeAxes = Axes.X;

                AutoSizeAxes = Axes.Y;

                Spacing = new Vector2(10, 5);



                Add(new AllEngineTypeTabItem { Clicked = onTabClicked });



                foreach (SkinEngineType type in Enum.GetValues<SkinEngineType>())

                {

                    Add(new SingleEngineTypeTabItem(type)

                    {

                        Clicked = onTabClicked,

                    });

                }

            }



            protected override void LoadComplete()

            {

                base.LoadComplete();

                onTabClicked(Children.First());

            }



            private void onTabClicked(EngineTypeTabItem item)

            {

                foreach (var child in Children)

                    child.Active.Value = child == item;



                Current.Value = item.SelectedType;

            }

        }



        private abstract partial class EngineTypeTabItem : FilterTabItem<SkinEngineType>

        {

            public Action<EngineTypeTabItem>? Clicked;



            protected EngineTypeTabItem(SkinEngineType value)

                : base(value)

            {

            }



            public abstract SkinEngineType? SelectedType { get; }



            protected override bool OnClick(ClickEvent e)

            {

                Clicked?.Invoke(this);

                return true;

            }

        }



        private partial class AllEngineTypeTabItem : EngineTypeTabItem

        {

            public AllEngineTypeTabItem()

                : base(default)

            {

            }



            public override SkinEngineType? SelectedType => null;



            protected override LocalisableString LabelFor(SkinEngineType value) => "All";

        }



        private partial class SingleEngineTypeTabItem : EngineTypeTabItem

        {

            public SingleEngineTypeTabItem(SkinEngineType value)

                : base(value)

            {

            }



            public override SkinEngineType? SelectedType => Value;



            protected override LocalisableString LabelFor(SkinEngineType value) => SkinEngineTypeHelper.GetDisplayName(value);

        }

    }

}


