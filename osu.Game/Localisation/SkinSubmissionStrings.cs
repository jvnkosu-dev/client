// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SkinSubmissionStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SkinSubmission";

        /// <summary>
        /// "Upload skin"
        /// </summary>
        public static LocalisableString SkinSubmissionTitle => new TranslatableString(getKey(@"skin_submission_title"), @"Upload skin");

        /// <summary>
        /// "Share your skin with the world!"
        /// </summary>
        public static LocalisableString SkinSubmissionDescription => new TranslatableString(getKey(@"skin_submission_description"), @"Share your skin with the world!");

        /// <summary>
        /// "Update skin"
        /// </summary>
        public static LocalisableString SkinUpdateTitle => new TranslatableString(getKey(@"skin_update_title"), @"Update skin");

        /// <summary>
        /// "Done"
        /// </summary>
        public static LocalisableString Done => new TranslatableString(getKey(@"done"), @"Done");

        /// <summary>
        /// "Confirm skin details, then update on the server"
        /// </summary>
        public static LocalisableString SkinUpdateDescription => new TranslatableString(getKey(@"skin_update_description"), @"Confirm skin details, then update on the server");

        /// <summary>
        /// "What status of your skin?"
        /// </summary>
        public static LocalisableString SkinSubmissionTargetCaption => new TranslatableString(getKey(@"skin_submission_target_caption"), @"What status of your skin?");

        /// <summary>
        /// "Status selection is currently unavailable."
        /// </summary>
        public static LocalisableString StatusSelectionUnavailable => new TranslatableString(getKey(@"status_selection_unavailable"), @"Status selection is currently unavailable.");

        /// <summary>
        /// "Open skin page after submission"
        /// </summary>
        public static LocalisableString OpenSkinPageAfterSubmission => new TranslatableString(getKey(@"open_skin_page_after_submission"), @"Open skin page after submission");

        /// <summary>
        /// "Preparing skin for upload..."
        /// </summary>
        public static LocalisableString Preparing => new TranslatableString(getKey(@"preparing"), @"Preparing skin for upload...");

        /// <summary>
        /// "Uploading skin..."
        /// </summary>
        public static LocalisableString Uploading => new TranslatableString(getKey(@"uploading"), @"Uploading skin...");

        /// <summary>
        /// "Finishing up..."
        /// </summary>
        public static LocalisableString Finishing => new TranslatableString(getKey(@"finishing"), @"Finishing up...");

        /// <summary>
        /// "Submission process"
        /// </summary>
        public static LocalisableString SubmissionProcess => new TranslatableString(getKey(@"submission_process"), @"Submission process");

        /// <summary>
        /// "Unsure about the skin submission process? Check out the wiki entry!"
        /// </summary>
        public static LocalisableString SubmissionProcessDescription => new TranslatableString(getKey(@"submission_process_description"), @"Unsure about the skin submission process? Check out the wiki entry!");

        /// <summary>
        /// "Skinning help forum"
        /// </summary>
        public static LocalisableString SkinningHelpForum => new TranslatableString(getKey(@"skinning_help_forum"), @"Skinning help forum");

        /// <summary>
        /// "Got some questions about skinning and submission? Ask them in the forums!"
        /// </summary>
        public static LocalisableString SkinningHelpForumDescription => new TranslatableString(getKey(@"skinning_help_forum_description"), @"Got some questions about skinning and submission? Ask them in the forums!");

        /// <summary>
        /// "Feedback queues forum"
        /// </summary>
        public static LocalisableString FeedbackQueuesForum => new TranslatableString(getKey(@"feedback_queues_forum"), @"Feedback queues forum");

        /// <summary>
        /// "Having trouble getting feedback on your skin? Why not ask in a feedback queue!"
        /// </summary>
        public static LocalisableString FeedbackQueuesForumDescription => new TranslatableString(getKey(@"feedback_queues_forum_description"), @"Having trouble getting feedback on your skin? Why not ask in a feedback queue!");

        /// <summary>
        /// "Note: Because jvnkosu! includes custom elements implemented in code, those elements will not be compatible with osu!(lazer) or osu!(stable). It means if you export a jvnkosu! skin and import it to osu!(lazer) or osu!(stable), the skin will not work as expected or will breaks completely."
        /// </summary>
        public static LocalisableString CustomElementsDisclaimer => new TranslatableString(getKey(@"custom_elements_disclaimer"), @"Note: Because jvnkosu! includes custom elements implemented in code, those elements will not be compatible with osu!(lazer) or osu!(stable). It means if you export a jvnkosu! skin and import it to osu!(lazer) or osu!(stable), the skin will not work as expected or will breaks completely.");

        /// <summary>
        /// "Works in Progress / Help (incomplete, not ready for ranking)"
        /// </summary>
        public static LocalisableString SkinSubmissionTargetWIP => new TranslatableString(getKey(@"skin_submission_target_wip"), @"Works in Progress / Help (incomplete, not ready for ranking)");

        /// <summary>
        /// "Pending (complete, ready for ranking)"
        /// </summary>
        public static LocalisableString SkinSubmissionTargetPending => new TranslatableString(getKey(@"skin_submission_target_pending"), @"Pending (complete, ready for ranking)");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
