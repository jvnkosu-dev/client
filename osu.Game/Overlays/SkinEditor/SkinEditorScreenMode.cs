// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Overlays.SkinEditor
{
    public enum SkinEditorScreenMode
    {
        [LocalisableDescription(typeof(EditorStrings), nameof(EditorStrings.SetupScreen))]
        Setup,

        [LocalisableDescription(typeof(EditorStrings), nameof(EditorStrings.ComposeScreen))]
        Compose,
    }
}
