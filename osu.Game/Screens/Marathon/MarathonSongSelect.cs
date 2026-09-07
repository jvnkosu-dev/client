// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.Marathon
{
    public partial class MarathonSongSelect : SongSelect
    {
        private readonly BindableList<PlaylistItem> playlist;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        public MarathonSongSelect(BindableList<PlaylistItem> playlist)
        {
            this.playlist = playlist;
            ShowOsuLogo = false;
        }

        protected override void OnStart() => addCurrentItem();

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons()
        {
            var buttons = base.CreateFooterButtons().ToList();
            buttons.Add(new AddToMarathonFooterButton
            {
                Action = addCurrentItem
            });
            return buttons;
        }

        private void addCurrentItem()
        {
            if (Beatmap.IsDefault)
                return;

            var beatmap = Beatmap.Value.BeatmapInfo;

            playlist.Add(new PlaylistItem(beatmap)
            {
                ID = playlist.Count == 0 ? 1 : playlist.Max(p => p.ID) + 1,
                RulesetID = Ruleset.Value.OnlineID,
                RequiredMods = Mods.Value.Select(m => new APIMod(m)).ToArray(),
            });

            notifications?.Post(new SimpleNotification
            {
                Text = MarathonStrings.MapAdded(beatmap.GetDisplayTitle()),
                Icon = FontAwesome.Solid.Check,
            });
        }

        private partial class AddToMarathonFooterButton : ScreenFooterButton
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Text = MarathonStrings.AddToMarathonButton;
                Icon = FontAwesome.Solid.Plus;
                AccentColour = colours.Blue1;
            }
        }
    }
}
