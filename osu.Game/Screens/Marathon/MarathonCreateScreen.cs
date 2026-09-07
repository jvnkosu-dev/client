// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay;
using osuTK;

namespace osu.Game.Screens.Marathon
{
    public partial class MarathonCreateScreen : OsuScreen
    {
        public override string Title => mode switch
        {
            EditorMode.Edit => "Edit marathon",
            EditorMode.Duplicate => "Duplicate marathon",
            _ => "Create marathon"
        };

        public override bool ShowFooter => true;

        public readonly Bindable<string> PlaylistName = new Bindable<string>();
        public readonly Bindable<string> PlaylistDescription = new Bindable<string>();
        public readonly Bindable<string> RankFrom = new Bindable<string>();
        public readonly Bindable<string> RankTo = new Bindable<string>();
        public readonly BindableList<PlaylistItem> Playlist = new BindableList<PlaylistItem>();

        private readonly EditorMode mode;
        private readonly APIMarathon? existing;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        private OnlinePlayScreenWaveContainer waves = null!;
        private SectionHeader mapPoolHeader = null!;
        private MarathonStarRangeDisplay starRange = null!;
        private MarathonRankRangeDisplay rankRange = null!;
        private DrawableRoomPlaylist drawablePlaylist = null!;
        private OsuSpriteText placeholder = null!;
        private CreateMarathonRequest? createRequest;
        private UpdateMarathonRequest? updateRequest;
        private bool submitting;
        private bool syncingPlaylist;

        private enum EditorMode
        {
            Create,
            Edit,
            Duplicate
        }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBlack(WaveContainer.APPEAR_DURATION);

        public MarathonCreateScreen()
            : this(null, EditorMode.Create)
        {
        }

        public MarathonCreateScreen(APIMarathon marathon, bool duplicate)
            : this(marathon, duplicate ? EditorMode.Duplicate : EditorMode.Edit)
        {
        }

        private MarathonCreateScreen(APIMarathon? marathon, EditorMode editorMode)
        {
            existing = marathon;
            mode = editorMode;
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };

            if (marathon == null)
                return;

            PlaylistName.Value = mode == EditorMode.Duplicate
                ? $"{marathon.Name} (copy)"
                : marathon.Name;
            PlaylistDescription.Value = marathon.Description;

            if (MarathonRankRange.TryGet(marathon, out int minRank, out int maxRank))
            {
                RankFrom.Value = minRank.ToString();
                RankTo.Value = maxRank.ToString();
            }

            Playlist.AddRange(MarathonMaps.ToPlaylistItems(marathon.Maps));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = waves = new OnlinePlayScreenWaveContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5,
                    },
                    new Header(Title, null),
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Horizontal = WaveOverlayContainer.WIDTH_PADDING,
                            Top = Header.HEIGHT,
                            Bottom = ScreenFooter.HEIGHT + 20,
                        },
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 10,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background3,
                                },
                                new GridContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(20),
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.Relative, 0.42f),
                                        new Dimension(),
                                    },
                                    Content = new[]
                                    {
                                        new[]
                                        {
                                            createSettingsColumn(),
                                            createMapPoolColumn(),
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playlist.BindCollectionChanged((_, _) =>
            {
                if (this.IsCurrentScreen())
                    applyPlaylistToDisplay();
                else
                    updateMapPoolState();
            }, true);

            drawablePlaylist.Items.BindCollectionChanged((_, _) =>
            {
                if (syncingPlaylist)
                    return;

                syncingPlaylist = true;
                Playlist.Clear();
                Playlist.AddRange(drawablePlaylist.Items);
                syncingPlaylist = false;
                updateMapPoolState();
            });

            RankFrom.BindValueChanged(_ => updateRankRange());
            RankTo.BindValueChanged(_ => updateRankRange(), true);
        }

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() =>
        [
            new AddMapsFooterButton
            {
                Action = openSongSelect
            },
            new SubmitFooterButton(mode)
            {
                Action = submit
            }
        ];

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            this.FadeIn();
            waves.Show();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            this.FadeIn(250);
            this.ScaleTo(1, 250, Easing.OutSine);
            applyPlaylistToDisplay();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);
            this.ScaleTo(1.1f, 250, Easing.InSine);
            this.FadeOut(250);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            createRequest?.Cancel();
            updateRequest?.Cancel();
            waves.Hide();
            this.Delay(WaveContainer.DISAPPEAR_DURATION).FadeOut();
            return base.OnExiting(e);
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);
            logo.Delay(WaveContainer.DISAPPEAR_DURATION / 2).FadeOut();
        }

        private Drawable createSettingsColumn()
        {
            var flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Padding = new MarginPadding { Right = 14 },
            };

            flow.Children = new Drawable[]
            {
                new SectionHeader(MarathonStrings.SettingsHeader)
                {
                    Margin = new MarginPadding { Left = 9 },
                },
                new FormTextBox
                {
                    Caption = MarathonStrings.NameCaption,
                    PlaceholderText = MarathonStrings.NamePlaceholder,
                    LengthLimit = 100,
                    Current = PlaylistName,
                    TabbableContentContainer = flow,
                },
                new FormTextBox
                {
                    Caption = MarathonStrings.DescriptionCaption,
                    PlaceholderText = MarathonStrings.DescriptionPlaceholder,
                    LengthLimit = 200,
                    Current = PlaylistDescription,
                    TabbableContentContainer = flow,
                },
                new FormNumberBox
                {
                    Caption = MarathonStrings.RankFromCaption,
                    PlaceholderText = MarathonStrings.RankFromPlaceholder,
                    Current = RankFrom,
                    TabbableContentContainer = flow,
                },
                new FormNumberBox
                {
                    Caption = MarathonStrings.RankToCaption,
                    PlaceholderText = MarathonStrings.RankToPlaceholder,
                    Current = RankTo,
                    TabbableContentContainer = flow,
                },
            };

            return new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = flow,
            };
        }

        private Drawable createMapPoolColumn() => new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding { Left = 14 },
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize),
                new Dimension(GridSizeMode.AutoSize),
                new Dimension(),
            },
            Content = new[]
            {
                new Drawable[]
                {
                    mapPoolHeader = new SectionHeader(MarathonStrings.MapPoolHeader),
                },
                new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10),
                        Margin = new MarginPadding { Bottom = 8, Left = 5 },
                        Children = new Drawable[]
                        {
                            starRange = new MarathonStarRangeDisplay(),
                            rankRange = new MarathonRankRangeDisplay(),
                        }
                    },
                },
                new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            drawablePlaylist = new MarathonMapPoolPlaylist
                            {
                                RelativeSizeAxes = Axes.Both,
                                AllowDeletion = true,
                                AllowReordering = true,
                                RequestDeletion = item => Playlist.Remove(item),
                            },
                            placeholder = new OsuSpriteText
                            {
                                Text = MarathonStrings.Placeholder,
                                Font = OsuFont.GetFont(size: 20),
                                Colour = colourProvider.Content2,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                        }
                    }
                },
            }
        };

        private void openSongSelect()
        {
            if (this.IsCurrentScreen())
                this.Push(new MarathonSongSelect(Playlist));
        }

        private void applyPlaylistToDisplay()
        {
            syncingPlaylist = true;
            drawablePlaylist.Items.Clear();
            drawablePlaylist.Items.AddRange(Playlist);
            syncingPlaylist = false;
            updateMapPoolState();
        }

        private void updateMapPoolState()
        {
            mapPoolHeader.DetailsText.Value = Playlist.Count > 0
                ? Playlist.Count.ToString()
                : string.Empty;

            placeholder.FadeTo(Playlist.Count == 0 ? 1 : 0, 200, Easing.OutQuint);
            updateStarRange();
            updateRankRange();
        }

        private void updateStarRange()
        {
            fillMissingStarRatings();

            if (MarathonStarRangeDisplay.TryGetRange(Playlist.Select(item => item.Beatmap.StarRating), out double min, out double max))
                starRange.SetRange(min, max);
            else
                starRange.ClearRange();
        }

        private void updateRankRange()
        {
            if (tryReadRankRange(out int min, out int max))
                rankRange.SetRange(min, max);
            else
                rankRange.ClearRange();
        }

        private bool tryReadRankRange(out int min, out int max)
        {
            min = max = 0;

            bool hasFrom = int.TryParse(RankFrom.Value, out min) && min > 0;
            bool hasTo = int.TryParse(RankTo.Value, out max) && max > 0;

            if (!hasFrom && !hasTo)
                return false;

            if (!hasFrom || !hasTo)
                return false;

            return MarathonRankRange.TryNormalise(min, max, out min, out max);
        }

        private void fillMissingStarRatings()
        {
            foreach (var item in Playlist)
            {
                if (item.Beatmap.StarRating > 0 || item.Beatmap is not APIBeatmap apiBeatmap)
                    continue;

                if (string.IsNullOrEmpty(apiBeatmap.MD5Hash))
                    continue;

                var local = beatmaps.QueryBeatmap(b => b.MD5Hash == apiBeatmap.MD5Hash);
                if (local != null && local.StarRating > 0)
                    apiBeatmap.StarRating = local.StarRating;
            }
        }

        private void submit()
        {
            if (submitting)
                return;

            var name = PlaylistName.Value?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(name))
            {
                notifications?.Post(new SimpleNotification { Text = MarathonStrings.CreateNeedsName });
                return;
            }

            if (Playlist.Count == 0)
            {
                notifications?.Post(new SimpleNotification { Text = MarathonStrings.CreateNeedsMaps });
                return;
            }

            string fromText = RankFrom.Value?.Trim() ?? string.Empty;
            string toText = RankTo.Value?.Trim() ?? string.Empty;
            int rankMin = 0;
            int rankMax = 0;

            if (!string.IsNullOrEmpty(fromText) || !string.IsNullOrEmpty(toText))
            {
                if (!tryReadRankRange(out rankMin, out rankMax))
                {
                    notifications?.Post(new SimpleNotification { Text = MarathonStrings.RankInvalid });
                    return;
                }
            }

            if (api.State.Value != APIState.Online || api.LocalUser.Value.Id <= 0)
            {
                notifications?.Post(new SimpleNotification { Text = MarathonStrings.MustBeLoggedIn });
                return;
            }

            if (mode == EditorMode.Edit && existing != null && existing.OwnerId != api.LocalUser.Value.Id)
            {
                notifications?.Post(new SimpleNotification { Text = MarathonStrings.NotOwner });
                return;
            }

            submitting = true;
            createRequest?.Cancel();
            updateRequest?.Cancel();

            var payload = new APIMarathon
            {
                Name = name,
                Description = PlaylistDescription.Value?.Trim() ?? string.Empty,
                OwnerId = api.LocalUser.Value.Id,
                OwnerUsername = api.LocalUser.Value.Username,
                RankMin = rankMin,
                RankMax = rankMax,
                Maps = Playlist.Select((item, index) => new APIMarathonMap
                {
                    BeatmapId = item.Beatmap.OnlineID,
                    Checksum = item.Beatmap.MD5Hash,
                    RulesetId = item.RulesetID,
                    RequiredMods = item.RequiredMods,
                    PlaylistOrder = index,
                    StarRating = item.Beatmap.StarRating > 0 ? item.Beatmap.StarRating : 0,
                }).ToArray(),
            };

            if (mode == EditorMode.Edit && existing != null)
            {
                payload.SourceOwnerId = existing.SourceOwnerId;
                payload.SourceOwnerUsername = existing.SourceOwnerUsername;

                updateRequest = new UpdateMarathonRequest(existing.Id, payload);
                updateRequest.Success += _ => finish(MarathonStrings.UpdateSuccess);
                updateRequest.Failure += onSubmitFailed;
                api.Queue(updateRequest);
                return;
            }

            if (mode == EditorMode.Duplicate && existing != null)
            {
                payload.SourceOwnerId = existing.SourceOwnerId != 0 ? existing.SourceOwnerId : existing.OwnerId;
                payload.SourceOwnerUsername = !string.IsNullOrEmpty(existing.SourceOwnerUsername)
                    ? existing.SourceOwnerUsername
                    : existing.OwnerUsername;
            }

            createRequest = new CreateMarathonRequest(payload);
            createRequest.Success += _ => finish(mode == EditorMode.Duplicate ? MarathonStrings.DuplicateSuccess : MarathonStrings.CreateSuccess);
            createRequest.Failure += onSubmitFailed;
            api.Queue(createRequest);
        }

        private void finish(LocalisableString message)
        {
            submitting = false;
            notifications?.Post(new SimpleNotification { Text = message });

            if (this.IsCurrentScreen())
                this.Exit();
        }

        private void onSubmitFailed(Exception exception)
        {
            if (exception is OperationCanceledException)
                return;

            submitting = false;
            notifications?.Post(new SimpleNotification
            {
                Text = string.IsNullOrWhiteSpace(exception.Message)
                    ? MarathonStrings.ListingUnavailable
                    : exception.Message
            });
        }

        private partial class AddMapsFooterButton : ScreenFooterButton
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Text = MarathonStrings.AddMapsButton;
                Icon = FontAwesome.Solid.Plus;
                AccentColour = colours.Pink1;
            }
        }

        private partial class SubmitFooterButton : ScreenFooterButton
        {
            private readonly EditorMode mode;

            public SubmitFooterButton(EditorMode mode)
            {
                this.mode = mode;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Text = mode switch
                {
                    EditorMode.Edit => MarathonStrings.SaveButton,
                    EditorMode.Duplicate => MarathonStrings.DuplicateButton,
                    _ => MarathonStrings.CreateButton
                };
                Icon = FontAwesome.Solid.Check;
                AccentColour = colours.Lime1;
            }
        }
    }
}
