// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;

namespace osu.Game.Overlays.SkinEditor
{
    public abstract partial class SkinEditorScreen : VisibilityContainer
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public readonly SkinEditorScreenMode Type;

        protected SkinEditorScreen(SkinEditorScreenMode type)
        {
            Type = type;

            RelativeSizeAxes = Axes.Both;

            InternalChild = content = new PopoverContainer { RelativeSizeAxes = Axes.Both };
        }

        protected override void PopIn() => this.FadeIn();

        protected override void PopOut() => this.FadeOut();
    }
}
