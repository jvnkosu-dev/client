// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Submission
{
    [LocalisableDescription(typeof(BeatmapSubmissionStrings), nameof(BeatmapSubmissionStrings.FrequentlyAskedQuestions))]
    public partial class ScreenSkinFrequentlyAskedQuestions : WizardScreen
    {
        [BackgroundDependencyLoader]
        private void load(OsuGame? game, IAPIProvider api)
        {
            Content.Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    new FormButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = SkinSubmissionStrings.SubmissionProcessDescription,
                        ButtonText = SkinSubmissionStrings.SubmissionProcess,
                        Action = () => game?.ShowWiki(@"Beatmap_ranking_procedure"),
                    },
                    new FormButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = SkinSubmissionStrings.SkinningHelpForumDescription,
                        ButtonText = SkinSubmissionStrings.SkinningHelpForum,
                        Action = () => game?.OpenUrlExternally($@"{api.Endpoints.WebsiteUrl}/community/forums/56"),
                    },
                    new FormButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = SkinSubmissionStrings.FeedbackQueuesForumDescription,
                        ButtonText = SkinSubmissionStrings.FeedbackQueuesForum,
                        Action = () => game?.OpenUrlExternally($@"{api.Endpoints.WebsiteUrl}/community/forums/60"),
                    },
                },
            });
        }
    }
}
