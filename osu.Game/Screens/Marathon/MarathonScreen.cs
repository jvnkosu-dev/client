// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Placeholders;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Screens.Marathon
{
    public partial class MarathonScreen : OsuScreen
    {
        public override string Title => "Marathon";

        public override bool ShowFooter => true;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        private OnlinePlayScreenWaveContainer waves = null!;
        private FillFlowContainer listFlow = null!;
        private Container placeholderContainer = null!;
        private LoadingLayer loadingLayer = null!;
        private GetMarathonsRequest? listingRequest;
        private readonly IBindable<APIUser> localUser = new Bindable<APIUser>();
        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBlack(WaveContainer.APPEAR_DURATION);

        public MarathonScreen()
        {
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };
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
                    new Header(MarathonStrings.HeaderTitle, null),
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
                                    RowDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(),
                                    },
                                    Content = new[]
                                    {
                                        new Drawable[]
                                        {
                                            new SectionHeader(MarathonStrings.ListingHeader),
                                        },
                                        new Drawable[]
                                        {
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Children = new Drawable[]
                                                {
                                                    new OsuScrollContainer
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Child = listFlow = new FillFlowContainer
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Direction = FillDirection.Vertical,
                                                            Spacing = new osuTK.Vector2(8),
                                                        }
                                                    },
                                                    placeholderContainer = new Container
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                    },
                                                    loadingLayer = new LoadingLayer(true),
                                                }
                                            }
                                        },
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

            localUser.BindTo(api.LocalUser);
            apiState.BindTo(api.State);
            localUser.BindValueChanged(_ => Schedule(reloadListingIfLoaded));
            apiState.BindValueChanged(_ => Schedule(reloadListingIfLoaded));
        }

        protected override void Dispose(bool isDisposing)
        {
            localUser.UnbindAll();
            apiState.UnbindAll();
            base.Dispose(isDisposing);
        }

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() =>
        [
            new CreateMarathonFooterButton
            {
                Action = openCreateScreen
            }
        ];

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            this.FadeIn();
            waves.Show();
            reloadListing();

            Scheduler.AddDelayed(() =>
            {
                if (this.IsCurrentScreen())
                    dialogOverlay?.Push(new MarathonTestDialog());
            }, 450);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            this.FadeIn(250);
            this.ScaleTo(1, 250, Easing.OutSine);
            reloadListing();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);
            this.ScaleTo(1.1f, 250, Easing.InSine);
            this.FadeOut(250);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            listingRequest?.Cancel();
            waves.Hide();
            this.Delay(WaveContainer.DISAPPEAR_DURATION).FadeOut();
            return base.OnExiting(e);
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);
            logo.Delay(WaveContainer.DISAPPEAR_DURATION / 2).FadeOut();
        }

        private bool isLoggedIn => api.State.Value == APIState.Online && api.LocalUser.Value.Id > 0;

        private bool owns(APIMarathon marathon) =>
            isLoggedIn && marathon.OwnerId != 0 && api.LocalUser.Value.Id == marathon.OwnerId;

        private bool requireLogin()
        {
            if (isLoggedIn)
                return true;

            notifications?.Post(new SimpleNotification { Text = MarathonStrings.MustBeLoggedIn });
            return false;
        }

        private void openCreateScreen()
        {
            if (!this.IsCurrentScreen() || !requireLogin())
                return;

            this.Push(new MarathonCreateScreen());
        }

        private void startPlay(APIMarathon marathon)
        {
            if (!this.IsCurrentScreen())
                return;

            if (marathon.Maps.Length == 0)
            {
                notifications?.Post(new SimpleNotification { Text = MarathonStrings.PlayNeedsMaps });
                return;
            }

            this.Push(new MarathonPlayScreen(marathon));
        }

        private void openDetail(APIMarathon marathon)
        {
            if (!this.IsCurrentScreen())
                return;

            this.Push(new MarathonDetailScreen(marathon));
        }

        private void openEditor(APIMarathon marathon, bool duplicate)
        {
            if (!this.IsCurrentScreen() || !requireLogin())
                return;

            if (!duplicate && !owns(marathon))
            {
                notifications?.Post(new SimpleNotification { Text = MarathonStrings.NotOwner });
                return;
            }

            this.Push(new MarathonCreateScreen(marathon, duplicate));
        }

        private void confirmDelete(APIMarathon marathon)
        {
            if (!requireLogin())
                return;

            if (!owns(marathon))
            {
                notifications?.Post(new SimpleNotification { Text = MarathonStrings.NotOwner });
                return;
            }

            dialogOverlay?.Push(new DeleteMarathonDialog(marathon.Name, () => deleteMarathon(marathon)));
        }

        private void deleteMarathon(APIMarathon marathon)
        {
            if (!owns(marathon))
                return;

            var request = new DeleteMarathonRequest(marathon.Id);
            request.Success += () =>
            {
                notifications?.Post(new SimpleNotification { Text = MarathonStrings.DeleteSuccess });
                reloadListing();
            };
            request.Failure += exception =>
            {
                if (exception is OperationCanceledException)
                    return;

                notifications?.Post(new SimpleNotification { Text = MarathonStrings.ListingUnavailable });
            };
            api.Queue(request);
        }

        private void reloadListingIfLoaded()
        {
            if (!IsLoaded)
                return;

            reloadListing();
        }

        private void reloadListing()
        {
            listingRequest?.Cancel();
            listFlow.Clear();
            showPlaceholder(null);

            // Requests are refused outright while the API is offline, which would otherwise
            // surface as a service failure rather than the login prompt the user actually needs.
            if (api.State.Value == APIState.Offline)
            {
                loadingLayer.Hide();
                showPlaceholder(new LoginPlaceholder(MarathonStrings.SignInToView));
                return;
            }

            loadingLayer.Show();

            listingRequest = new GetMarathonsRequest();
            listingRequest.Success += onListingLoaded;
            listingRequest.Failure += onListingFailed;
            api.Queue(listingRequest);
        }

        private void onListingLoaded(GetMarathonsResponse response)
        {
            loadingLayer.Hide();

            var marathons = response.Marathons;

            if (marathons.Length == 0)
            {
                showPlaceholder(new MessagePlaceholder(MarathonStrings.ListingEmpty));
                return;
            }

            showPlaceholder(null);

            foreach (var marathon in marathons)
            {
                listFlow.Add(new MarathonPanel(marathon)
                {
                    Action = () => openDetail(marathon),
                    PlayAction = () => startPlay(marathon),
                    EditAction = () => openEditor(marathon, false),
                    DuplicateAction = () => openEditor(marathon, true),
                    DeleteAction = () => confirmDelete(marathon),
                });
            }
        }

        private void onListingFailed(Exception exception)
        {
            if (exception is OperationCanceledException)
                return;

            loadingLayer.Hide();
            listFlow.Clear();

            showPlaceholder(new ClickablePlaceholder(MarathonStrings.ListingUnavailable, FontAwesome.Solid.Sync)
            {
                Action = reloadListing
            });
        }

        private void showPlaceholder(Drawable? content)
        {
            placeholderContainer.Clear();

            if (content == null)
                return;

            placeholderContainer.Add(content);
            content.FadeInFromZero(250, Easing.OutQuint);
        }

        private partial class CreateMarathonFooterButton : ScreenFooterButton
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Text = MarathonStrings.CreateButton;
                Icon = FontAwesome.Solid.Plus;
                AccentColour = colours.Pink1;
            }
        }
    }
}
