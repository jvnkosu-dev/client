// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class MarathonStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Marathon";

        /// <summary>
        /// "marathon"
        /// </summary>
        public static LocalisableString MenuButton => new TranslatableString(getKey(@"menu_button"), @"marathon");

        /// <summary>
        /// "Marathon"
        /// </summary>
        public static LocalisableString HeaderTitle => new TranslatableString(getKey(@"header_title"), @"Marathon");

        /// <summary>
        /// "Marathons"
        /// </summary>
        public static LocalisableString ListingHeader => new TranslatableString(getKey(@"listing_header"), @"Marathons");

        /// <summary>
        /// "No marathons yet."
        /// </summary>
        public static LocalisableString ListingEmpty => new TranslatableString(getKey(@"listing_empty"), @"No marathons yet.");

        /// <summary>
        /// "Couldn't reach the marathon service. Start it locally, then try again."
        /// </summary>
        public static LocalisableString ListingUnavailable => new TranslatableString(getKey(@"listing_unavailable"), @"Couldn't reach the marathon service. Start it locally, then try again.");

        /// <summary>
        /// "Create marathon"
        /// </summary>
        public static LocalisableString CreateButton => new TranslatableString(getKey(@"create_button"), @"Create marathon");

        /// <summary>
        /// "Give the marathon a name."
        /// </summary>
        public static LocalisableString CreateNeedsName => new TranslatableString(getKey(@"create_needs_name"), @"Give the marathon a name.");

        /// <summary>
        /// "Add at least one map."
        /// </summary>
        public static LocalisableString CreateNeedsMaps => new TranslatableString(getKey(@"create_needs_maps"), @"Add at least one map.");

        /// <summary>
        /// "Marathon created."
        /// </summary>
        public static LocalisableString CreateSuccess => new TranslatableString(getKey(@"create_success"), @"Marathon created.");

        /// <summary>
        /// "Playlist settings"
        /// </summary>
        public static LocalisableString SettingsHeader => new TranslatableString(getKey(@"settings_header"), @"Playlist settings");

        /// <summary>
        /// "Name"
        /// </summary>
        public static LocalisableString NameCaption => new TranslatableString(getKey(@"name_caption"), @"Name");

        /// <summary>
        /// "My marathon"
        /// </summary>
        public static LocalisableString NamePlaceholder => new TranslatableString(getKey(@"name_placeholder"), @"My marathon");

        /// <summary>
        /// "Description"
        /// </summary>
        public static LocalisableString DescriptionCaption => new TranslatableString(getKey(@"description_caption"), @"Description");

        /// <summary>
        /// "Optional"
        /// </summary>
        public static LocalisableString DescriptionPlaceholder => new TranslatableString(getKey(@"description_placeholder"), @"Optional");

        /// <summary>
        /// "Map pool"
        /// </summary>
        public static LocalisableString MapPoolHeader => new TranslatableString(getKey(@"map_pool_header"), @"Map pool");

        /// <summary>
        /// "This is a test feature"
        /// </summary>
        public static LocalisableString DialogHeader => new TranslatableString(getKey(@"dialog_header"), @"This is a test feature");

        /// <summary>
        /// "Nothing is awarded yet."
        /// </summary>
        public static LocalisableString DialogBody => new TranslatableString(getKey(@"dialog_body"), @"Nothing is awarded yet.");

        /// <summary>
        /// "Understood"
        /// </summary>
        public static LocalisableString DialogOk => new TranslatableString(getKey(@"dialog_ok"), @"Understood");

        /// <summary>
        /// "Add maps"
        /// </summary>
        public static LocalisableString AddMapsButton => new TranslatableString(getKey(@"add_maps_button"), @"Add maps");

        /// <summary>
        /// "Add maps to build your marathon."
        /// </summary>
        public static LocalisableString Placeholder => new TranslatableString(getKey(@"placeholder"), @"Add maps to build your marathon.");

        /// <summary>
        /// "Added {0} to the marathon."
        /// </summary>
        public static LocalisableString MapAdded(string title) => new TranslatableString(getKey(@"map_added"), @"Added {0} to the marathon.", title);

        /// <summary>
        /// "Play"
        /// </summary>
        public static LocalisableString PlayButton => new TranslatableString(getKey(@"play_button"), @"Play");

        /// <summary>
        /// "This marathon has no maps."
        /// </summary>
        public static LocalisableString PlayNeedsMaps => new TranslatableString(getKey(@"play_needs_maps"), @"This marathon has no maps.");

        /// <summary>
        /// "Downloading map pool…"
        /// </summary>
        public static LocalisableString DownloadingPool => new TranslatableString(getKey(@"downloading_pool"), @"Downloading map pool…");

        /// <summary>
        /// "{0} / {1} maps ready"
        /// </summary>
        public static LocalisableString DownloadProgress(int ready, int total) =>
            new TranslatableString(getKey(@"download_progress"), @"{0} / {1} maps ready", ready, total);

        /// <summary>
        /// "Couldn't download every map. Playing the ones that are available."
        /// </summary>
        public static LocalisableString DownloadPartial => new TranslatableString(getKey(@"download_partial"), @"Couldn't download every map. Playing the ones that are available.");

        /// <summary>
        /// "None of the maps in this marathon could be downloaded."
        /// </summary>
        public static LocalisableString DownloadNone => new TranslatableString(getKey(@"download_none"), @"None of the maps in this marathon could be downloaded.");

        /// <summary>
        /// "Next map in {0}"
        /// </summary>
        public static LocalisableString NextMapIn(int seconds) => new TranslatableString(getKey(@"next_map_in"), @"Next map in {0}", seconds);

        /// <summary>
        /// "Map {0} of {1}"
        /// </summary>
        public static LocalisableString MapProgress(int current, int total) =>
            new TranslatableString(getKey(@"map_progress"), @"Map {0} of {1}", current, total);

        /// <summary>
        /// "Next map"
        /// </summary>
        public static LocalisableString NextMapButton => new TranslatableString(getKey(@"next_map_button"), @"Next map");

        /// <summary>
        /// "Marathon summary"
        /// </summary>
        public static LocalisableString SummaryHeader => new TranslatableString(getKey(@"summary_header"), @"Marathon summary");

        /// <summary>
        /// "Show maps"
        /// </summary>
        public static LocalisableString ShowMapsButton => new TranslatableString(getKey(@"show_maps_button"), @"Show maps");

        /// <summary>
        /// "Finish"
        /// </summary>
        public static LocalisableString FinishButton => new TranslatableString(getKey(@"finish_button"), @"Finish");

        /// <summary>
        /// "{0} of {1} maps cleared"
        /// </summary>
        public static LocalisableString MapsCleared(int cleared, int total) =>
            new TranslatableString(getKey(@"maps_cleared"), @"{0} of {1} maps cleared", cleared, total);

        /// <summary>
        /// "Total score"
        /// </summary>
        public static LocalisableString TotalScore => new TranslatableString(getKey(@"total_score"), @"Total score");

        /// <summary>
        /// "Average accuracy"
        /// </summary>
        public static LocalisableString AverageAccuracy => new TranslatableString(getKey(@"average_accuracy"), @"Average accuracy");

        /// <summary>
        /// "Best combo"
        /// </summary>
        public static LocalisableString BestCombo => new TranslatableString(getKey(@"best_combo"), @"Best combo");

        /// <summary>
        /// "Total pp"
        /// </summary>
        public static LocalisableString TotalPerformance => new TranslatableString(getKey(@"total_performance"), @"Total pp");

        /// <summary>
        /// "Passed"
        /// </summary>
        public static LocalisableString MapPassed => new TranslatableString(getKey(@"map_passed"), @"Passed");

        /// <summary>
        /// "Failed"
        /// </summary>
        public static LocalisableString MapFailed => new TranslatableString(getKey(@"map_failed"), @"Failed");

        /// <summary>
        /// "Played maps"
        /// </summary>
        public static LocalisableString PlayedMaps => new TranslatableString(getKey(@"played_maps"), @"Played maps");

        /// <summary>
        /// "Add to marathon"
        /// </summary>
        public static LocalisableString AddToMarathonButton => new TranslatableString(getKey(@"add_to_marathon_button"), @"Add to marathon");

        /// <summary>
        /// "Edit"
        /// </summary>
        public static LocalisableString EditButton => new TranslatableString(getKey(@"edit_button"), @"Edit");

        /// <summary>
        /// "Save changes"
        /// </summary>
        public static LocalisableString SaveButton => new TranslatableString(getKey(@"save_button"), @"Save changes");

        /// <summary>
        /// "Edit marathon"
        /// </summary>
        public static LocalisableString EditHeader => new TranslatableString(getKey(@"edit_header"), @"Edit marathon");

        /// <summary>
        /// "Duplicate marathon"
        /// </summary>
        public static LocalisableString DuplicateHeader => new TranslatableString(getKey(@"duplicate_header"), @"Duplicate marathon");

        /// <summary>
        /// "Duplicate"
        /// </summary>
        public static LocalisableString DuplicateButton => new TranslatableString(getKey(@"duplicate_button"), @"Duplicate");

        /// <summary>
        /// "Delete"
        /// </summary>
        public static LocalisableString DeleteButton => new TranslatableString(getKey(@"delete_button"), @"Delete");

        /// <summary>
        /// "Delete {0}?"
        /// </summary>
        public static LocalisableString DeleteHeader(string name) => new TranslatableString(getKey(@"delete_header"), @"Delete {0}?", name);

        /// <summary>
        /// "This cannot be undone."
        /// </summary>
        public static LocalisableString DeleteBody => new TranslatableString(getKey(@"delete_body"), @"This cannot be undone.");

        /// <summary>
        /// "Marathon updated."
        /// </summary>
        public static LocalisableString UpdateSuccess => new TranslatableString(getKey(@"update_success"), @"Marathon updated.");

        /// <summary>
        /// "Copy created."
        /// </summary>
        public static LocalisableString DuplicateSuccess => new TranslatableString(getKey(@"duplicate_success"), @"Copy created.");

        /// <summary>
        /// "Marathon deleted."
        /// </summary>
        public static LocalisableString DeleteSuccess => new TranslatableString(getKey(@"delete_success"), @"Marathon deleted.");

        /// <summary>
        /// "You can only change your own marathons."
        /// </summary>
        public static LocalisableString NotOwner => new TranslatableString(getKey(@"not_owner"), @"You can only change your own marathons.");

        /// <summary>
        /// "Log in to create or edit marathons."
        /// </summary>
        public static LocalisableString MustBeLoggedIn => new TranslatableString(getKey(@"must_be_logged_in"), @"Log in to create or edit marathons.");

        /// <summary>
        /// "Sign in to view marathons"
        /// </summary>
        public static LocalisableString SignInToView => new TranslatableString(getKey(@"sign_in_to_view"), @"Sign in to view marathons");

        /// <summary>
        /// "World rank from"
        /// </summary>
        public static LocalisableString RankFromCaption => new TranslatableString(getKey(@"rank_from_caption"), @"World rank from");

        /// <summary>
        /// "World rank to"
        /// </summary>
        public static LocalisableString RankToCaption => new TranslatableString(getKey(@"rank_to_caption"), @"World rank to");

        /// <summary>
        /// "1000"
        /// </summary>
        public static LocalisableString RankFromPlaceholder => new TranslatableString(getKey(@"rank_from_placeholder"), @"1000");

        /// <summary>
        /// "50000"
        /// </summary>
        public static LocalisableString RankToPlaceholder => new TranslatableString(getKey(@"rank_to_placeholder"), @"50000");

        /// <summary>
        /// "Set both ends of the rank range, or leave both empty."
        /// </summary>
        public static LocalisableString RankInvalid => new TranslatableString(getKey(@"rank_invalid"), @"Set both ends of the rank range, or leave both empty.");

        /// <summary>
        /// "Aimed at this global rank range"
        /// </summary>
        public static LocalisableString RankAudience => new TranslatableString(getKey(@"rank_audience"), @"Aimed at this global rank range");

        /// <summary>
        /// "Around your global rank"
        /// </summary>
        public static LocalisableString RankFitsYou => new TranslatableString(getKey(@"rank_fits_you"), @"Around your global rank");

        /// <summary>
        /// "A bit off your global rank"
        /// </summary>
        public static LocalisableString RankMediumGap => new TranslatableString(getKey(@"rank_medium_gap"), @"A bit off your global rank");

        /// <summary>
        /// "Far from your global rank"
        /// </summary>
        public static LocalisableString RankFarGap => new TranslatableString(getKey(@"rank_far_gap"), @"Far from your global rank");

        /// <summary>
        /// "originally {0}"
        /// </summary>
        public static LocalisableString OriginallyBy(string username) => new TranslatableString(getKey(@"originally_by"), @"originally {0}", username);

        /// <summary>
        /// "{0} (copy)"
        /// </summary>
        public static LocalisableString CopyName(string name) => new TranslatableString(getKey(@"copy_name"), @"{0} (copy)", name);

        /// <summary>
        /// "{0}pp"
        /// </summary>
        public static LocalisableString Performance(string pp) => new TranslatableString(getKey(@"performance"), @"{0}pp", pp);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
