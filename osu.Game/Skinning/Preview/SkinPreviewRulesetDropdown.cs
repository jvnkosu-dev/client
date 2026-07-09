// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;

namespace osu.Game.Skinning.Preview
{
    public partial class SkinPreviewRulesetDropdown : OsuDropdown<RulesetInfo>
    {
        public event Action<MenuState>? MenuStateChanged;

        public SkinPreviewRulesetDropdown()
        {
            RelativeSizeAxes = Axes.None;
            AutoSizeAxes = Axes.None;
            Width = 130;
            Height = 22;
        }

        protected override DropdownMenu CreateMenu() => base.CreateMenu().With(m => m.MaxHeight = 200);

        protected override DropdownHeader CreateHeader() => new SlimDropdownHeader();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Menu.StateChanged += state => MenuStateChanged?.Invoke(state);
        }

        private partial class SlimDropdownHeader : OsuDropdownHeader
        {
            public SlimDropdownHeader()
            {
                Height = 22;
                Foreground.Padding = new MarginPadding { Top = 2, Bottom = 2, Left = 6, Right = 4 };
            }
        }
    }
}
