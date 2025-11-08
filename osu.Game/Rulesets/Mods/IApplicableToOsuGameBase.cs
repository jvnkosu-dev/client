// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that apply changes to the <see cref="OsuGameBase"/>.
    /// This is really stupid and f%%king dangerous, possibly disasterous even.
    /// </summary>
    public interface IApplicableToOsuGameBase : IApplicableMod
    {
        /// <summary>
        /// Provide a <see cref="OsuGameBase"/>. Called once on initialisation of a play instance.
        /// </summary>
        void ApplyToOsuGameBase(OsuGameBase game);
    }
}
