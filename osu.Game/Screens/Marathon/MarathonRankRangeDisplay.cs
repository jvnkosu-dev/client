// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Marathon
{
    /// <summary>
    /// Single-colour pill showing the recommended world-rank band for a marathon.
    /// Colour is a traffic light against the local user's rank: green if they sit
    /// in (or within 100k of) the band, yellow at a 100–200k gap, red beyond that.
    /// </summary>
    public partial class MarathonRankRangeDisplay : CompositeDrawable, IHasTooltip
    {
        private int rankMin;
        private int rankMax;
        private bool hasRange;

        private Box background = null!;
        private OsuSpriteText rankText = null!;
        private SpriteIcon icon = null!;
        private OsuColour colours = null!;

        private readonly IBindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        [Resolved]
        private IBindable<RulesetInfo>? gameRuleset { get; set; }

        [Resolved]
        private LocalUserStatisticsProvider? statistics { get; set; }

        public LocalisableString TooltipText { get; private set; }

        public MarathonRankRangeDisplay()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            Scale = new Vector2(0.8f);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            this.colours = colours;

            InternalChild = new CircularContainer
            {
                Masking = true,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(5),
                        Padding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                        Children = new Drawable[]
                        {
                            icon = new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Icon = FontAwesome.Solid.Globe,
                                Size = new Vector2(9),
                            },
                            rankText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.Bold),
                                Shadow = false,
                                Margin = new MarginPadding { Bottom = 1.5f },
                            },
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (gameRuleset != null)
                ruleset.BindTo(gameRuleset);

            ruleset.BindValueChanged(_ => updateDisplay());

            if (statistics != null)
                statistics.StatisticsUpdated += onStatisticsUpdated;

            updateDisplay();
        }

        public void SetFrom(APIMarathon marathon)
        {
            if (MarathonRankRange.TryGet(marathon, out int min, out int max))
                SetRange(min, max);
            else
                ClearRange();
        }

        public void SetRange(int min, int max)
        {
            if (!MarathonRankRange.TryNormalise(min, max, out rankMin, out rankMax))
            {
                ClearRange();
                return;
            }

            hasRange = true;
            BypassAutoSizeAxes = Axes.None;
            Show();
            updateDisplay();
        }

        public void ClearRange()
        {
            hasRange = false;
            rankMin = rankMax = 0;
            Hide();
            BypassAutoSizeAxes = Axes.Both;
            TooltipText = default;
        }

        private void onStatisticsUpdated(UserStatisticsUpdate _) => Scheduler.AddOnce(updateDisplay);

        private void updateDisplay()
        {
            if (!hasRange || !IsLoaded)
                return;

            int? viewerRank = null;

            if (ruleset.Value != null)
                viewerRank = statistics?.GetStatisticsFor(ruleset.Value)?.GlobalRank;

            var fit = MarathonRankRange.Fit(rankMin, rankMax, viewerRank);

            rankText.Text = $"#{rankMin:N0} – #{rankMax:N0}";
            background.Colour = colourFor(fit);

            var textColour = textColourFor(fit);
            rankText.Colour = textColour;
            icon.Colour = textColour;

            TooltipText = fit switch
            {
                RankFit.Close => MarathonStrings.RankFitsYou,
                RankFit.Medium => MarathonStrings.RankMediumGap,
                RankFit.Far => MarathonStrings.RankFarGap,
                _ => MarathonStrings.RankAudience,
            };
        }

        private Colour4 colourFor(RankFit fit) => fit switch
        {
            RankFit.Close => colours.Lime1,
            RankFit.Medium => colours.Yellow,
            RankFit.Far => colours.Red1,
            _ => colours.Gray5,
        };

        private static Color4 textColourFor(RankFit fit) => fit switch
        {
            RankFit.Close or RankFit.Medium => Color4.Black.Opacity(0.75f),
            _ => Color4.White,
        };

        protected override void Dispose(bool isDisposing)
        {
            if (statistics.IsNotNull())
                statistics.StatisticsUpdated -= onStatisticsUpdated;

            ruleset.UnbindAll();
            base.Dispose(isDisposing);
        }
    }
}
