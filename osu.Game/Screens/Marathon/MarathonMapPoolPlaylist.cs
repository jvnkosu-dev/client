// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Screens.Marathon
{
    public partial class MarathonMapPoolPlaylist : DrawableRoomPlaylist
    {
        protected override DrawableRoomPlaylistItem CreateDrawablePlaylistItem(PlaylistItem item) => new MarathonMapPoolPlaylistItem(item);
    }

    public partial class MarathonMapPoolPlaylistItem : DrawableRoomPlaylistItem
    {
        public MarathonMapPoolPlaylistItem(PlaylistItem item)
            : base(item)
        {
        }

        protected override float ItemFontSize => 16;
    }
}
