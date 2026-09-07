// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osuTK;

namespace osu.Game.Screens.Marathon
{
    public partial class MarathonStarRangeDisplay : CompositeDrawable
    {
        private readonly Room room = new Room();

        public MarathonStarRangeDisplay()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            Scale = new Vector2(0.8f);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new StarRatingRangeDisplay(room);
        }

        public void SetRange(double min, double max)
        {
            room.DifficultyRange = new Room.RoomDifficultyRange
            {
                Min = min,
                Max = max,
            };

            BypassAutoSizeAxes = Axes.None;
            Show();
        }

        public void ClearRange()
        {
            room.DifficultyRange = null;
            Hide();
            BypassAutoSizeAxes = Axes.Both;
        }

        public static bool TryGetRange(IEnumerable<double> ratings, out double min, out double max)
        {
            var stars = ratings.Where(s => s > 0).ToArray();

            if (stars.Length == 0)
            {
                min = max = 0;
                return false;
            }

            min = stars.Min();
            max = stars.Max();
            return true;
        }
    }
}
