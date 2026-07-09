// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.SkinListing;

namespace osu.Game.Overlays.Toolbar
{
    public partial class ToolbarSkinListingButton : ToolbarOverlayToggleButton
    {
        protected override Anchor TooltipAnchor => Anchor.TopRight;

        public ToolbarSkinListingButton()
        {
            Hotkey = GlobalAction.ToggleSkinListing;
        }

        [BackgroundDependencyLoader(true)]
        private void load(SkinListingOverlay skinListing)
        {
            StateContainer = skinListing;
        }
    }
}
