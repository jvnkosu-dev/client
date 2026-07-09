// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Preview
{
    internal partial class ManiaSkinPreviewDependencyHost : Container
    {
        private readonly StageDefinition stageDefinition;

        public ManiaSkinPreviewDependencyHost(Drawable child, StageDefinition? stageDefinition = null)
        {
            this.stageDefinition = stageDefinition ?? new StageDefinition(5);

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Child = child;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs(stageDefinition);
            dependencies.CacheAs<IScrollingInfo>(new PreviewScrollingInfo());
            return dependencies;
        }

        private class PreviewScrollingInfo : IScrollingInfo
        {
            public Bindable<ScrollingDirection> Direction { get; } = new Bindable<ScrollingDirection>(ScrollingDirection.Down);

            IBindable<ScrollingDirection> IScrollingInfo.Direction => Direction;

            IBindable<double> IScrollingInfo.TimeRange { get; } = new Bindable<double>(5000);

            IBindable<IScrollAlgorithm> IScrollingInfo.Algorithm { get; } = new Bindable<IScrollAlgorithm>(new ConstantScrollAlgorithm());
        }
    }

    /// <summary>
    /// Hosts a single mania column preview using the same drawable contract as gameplay
    /// (<see cref="Column"/>): full-size key area, hit-target padding, and TopLevelContainer for Argon proxies.
    /// Sized by <see cref="ColumnFlow{T}"/> like real stage columns.
    /// </summary>
    internal partial class SkinPreviewManiaColumnHost : Container
    {
        /// <summary>
        /// Enough height for the key area (<see cref="Stage.HIT_TARGET_POSITION"/>) plus a short note field above it.
        /// </summary>
        public const float COLUMN_HEIGHT = Stage.HIT_TARGET_POSITION + 50;

        private readonly int columnIndex;
        private readonly StageDefinition stageDefinition;
        private readonly Column column;

        public SkinPreviewManiaColumnHost(int columnIndex, StageDefinition stageDefinition)
        {
            this.columnIndex = columnIndex;
            this.stageDefinition = stageDefinition;

            column = new Column(columnIndex, stageDefinition.IsSpecialColumn(columnIndex));

            // Match Stage column placement inside ColumnFlow: fill the flow cell completely.
            RelativeSizeAxes = Axes.Both;
            Width = 1;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            column.AccentColour.Value = skin.GetManiaSkinConfig<Color4>(LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour, columnIndex)?.Value
                                        ?? getFallbackAccent(columnIndex);

            // Mirror Column's visual stack (background → hit area → key area → top-level proxies).
            Children = new Drawable[]
            {
                new SkinnableDrawable(new ManiaSkinComponentLookup(ManiaSkinComponents.ColumnBackground), _ => new DefaultColumnBackground())
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Bottom = Stage.HIT_TARGET_POSITION },
                    Children = new Drawable[]
                    {
                        new SkinnableDrawable(new ManiaSkinComponentLookup(ManiaSkinComponents.HitTarget), _ => new DefaultHitTarget())
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new Container
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = DefaultNotePiece.NOTE_HEIGHT,
                            Y = -4,
                            Child = new SkinnableDrawable(new ManiaSkinComponentLookup(ManiaSkinComponents.Note), _ => new DefaultNotePiece())
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                    },
                },
                new SkinnableDrawable(new ManiaSkinComponentLookup(ManiaSkinComponents.KeyArea), _ => new DefaultKeyArea())
                {
                    RelativeSizeAxes = Axes.Both,
                },
                // Required so ArgonKeyArea can proxy itself above notes, same as gameplay Column.
                column.TopLevelContainer,
            };
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs(stageDefinition);
            dependencies.CacheAs(column);
            dependencies.CacheAs<IBindable<ManiaAction>>(column.Action);
            return dependencies;
        }

        private static Color4 getFallbackAccent(int index)
        {
            Color4[] colours =
            {
                Color4Extensions.FromHex("#55ccff"),
                Color4Extensions.FromHex("#ed1121"),
                Color4Extensions.FromHex("#ffffff"),
                Color4Extensions.FromHex("#ffff00"),
                Color4Extensions.FromHex("#8866ff"),
            };

            return colours[index % colours.Length];
        }
    }

    public partial class SkinPreviewManiaStage : Container
    {
        private const int column_count = 5;

        private ColumnFlow<SkinPreviewManiaColumnHost> columnFlow = null!;

        public SkinPreviewManiaStage()
        {
            // Explicit non-relative size: SkinPreviewCard skips RelativeSizeAxes drawables when
            // measuring, and almost everything inside a mania stage is relative-sized.
            // Width is refined from ColumnFlow after skin.ini widths apply.
            Height = SkinPreviewManiaColumnHost.COLUMN_HEIGHT;
            Width = estimateDefaultWidth();
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var stageDefinition = new StageDefinition(column_count);
            columnFlow = new ColumnFlow<SkinPreviewManiaColumnHost>(stageDefinition)
            {
                RelativeSizeAxes = Axes.Y,
            };

            for (int i = 0; i < column_count; i++)
                columnFlow.SetContentForColumn(i, new SkinPreviewManiaColumnHost(i, stageDefinition));

            // Full skin.ini / gameplay column widths (same basis as LegacyStageBackground).
            // SkinPreviewCard then scales this whole stage to fit the card.
            Child = new ManiaSkinPreviewDependencyHost(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new SkinnableDrawable(new ManiaSkinComponentLookup(ManiaSkinComponents.StageBackground), _ => new DefaultStageBackground())
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    columnFlow,
                },
            }, stageDefinition)
            {
                // Constructor defaults to RelativeSizeAxes.X + AutoSizeAxes.Y; clear AutoSize first.
                AutoSizeAxes = Axes.None,
                RelativeSizeAxes = Axes.Both,
            };
        }

        protected override void Update()
        {
            base.Update();

            if (columnFlow.DrawWidth > 0)
                Width = columnFlow.DrawWidth;
        }

        private static float estimateDefaultWidth()
        {
            // 5K: one special + four normal columns, plus default left/right spacing per column.
            return Column.SPECIAL_COLUMN_WIDTH
                   + Column.COLUMN_WIDTH * (column_count - 1)
                   + Stage.COLUMN_SPACING * column_count * 2;
        }
    }

    public partial class SkinPreviewManiaFont : Container
    {
        private readonly ISkin skin;

        public SkinPreviewManiaFont(ISkin skin)
        {
            this.skin = skin;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skinSource)
        {
            Child = createPreviewText(skinSource);
        }

        private Drawable createPreviewText(ISkinSource skinSource)
        {
            if (skin is LegacySkin && skinSource.HasFont(LegacyFont.Combo))
            {
                return new LegacySpriteText(LegacyFont.Combo)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "42",
                };
            }

            return new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Default.With(size: 24, weight: FontWeight.Bold),
                Text = "42",
            };
        }
    }
}
