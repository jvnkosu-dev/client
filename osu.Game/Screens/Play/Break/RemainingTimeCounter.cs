// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.Play.Break
{
    public partial class RemainingTimeCounter : Counter
    {
        private readonly ArgonCounterTextComponent counter;

        public RemainingTimeCounter()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = counter = new ArgonCounterTextComponent(Anchor.Centre);
            counter.Scale *= 1.25f; // this seems to be the only way to make the counter bigger, I hope I'm wrong
            counter.WireframeOpacity.BindTo(new BindableFloat(0.125f));
        }
        private string lookup(char c)
        {
            return c.ToString();
        }
        protected override void OnCountChanged(double count)
        {
            string displayText = ((int)Math.Ceiling(count / 1000)).ToString();
            counter.Text = displayText;
            counter.WireframeTemplate = new string('#', displayText.Length);
        }
    }
}
