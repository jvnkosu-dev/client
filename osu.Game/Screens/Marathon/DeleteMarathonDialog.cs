// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Marathon
{
    public partial class DeleteMarathonDialog : DangerousActionDialog
    {
        public DeleteMarathonDialog(string name, Action confirm)
        {
            HeaderText = MarathonStrings.DeleteHeader(name);
            BodyText = MarathonStrings.DeleteBody;
            Icon = FontAwesome.Solid.Trash;
            DangerousAction = confirm;
        }
    }
}
