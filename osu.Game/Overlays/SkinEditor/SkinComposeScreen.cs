// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.SkinEditor
{
    /// <summary>
    /// Hosts the existing skin layout editing UI (scene library, sidebars, blueprints).
    /// </summary>
    public partial class SkinComposeScreen : SkinEditorScreen
    {
        public SkinComposeScreen(Drawable composeContent)
            : base(SkinEditorScreenMode.Compose)
        {
            Add(composeContent);
        }
    }
}
