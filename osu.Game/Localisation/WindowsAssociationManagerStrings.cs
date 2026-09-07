// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class WindowsAssociationManagerStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.WindowsAssociationManager";

        /// <summary>
        /// "jvnkosu! Beatmap"
        /// </summary>
        public static LocalisableString OsuBeatmap => new TranslatableString(getKey(@"jvnkosu_beatmap"), @"jvnkosu! Beatmap");

        /// <summary>
        /// "jvnkosu! Replay"
        /// </summary>
        public static LocalisableString OsuReplay => new TranslatableString(getKey(@"jvnkosu_replay"), @"jvnkosu! Replay");

        /// <summary>
        /// "jvnkosu! Skin"
        /// </summary>
        public static LocalisableString OsuSkin => new TranslatableString(getKey(@"jvnkosu_skin"), @"jvnkosu! Skin");

        /// <summary>
        /// "jvnkosu!"
        /// </summary>
        public static LocalisableString OsuProtocol => new TranslatableString(getKey(@"jvnkosu_protocol"), @"jvnkosu!");

        /// <summary>
        /// "jvnkosu! Multiplayer"
        /// </summary>
        public static LocalisableString OsuMultiplayer => new TranslatableString(getKey(@"jvnkosu_multiplayer"), @"jvnkosu! Multiplayer");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}