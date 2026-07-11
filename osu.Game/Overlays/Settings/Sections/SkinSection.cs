// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.IO.Network;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Overlays.SkinSet;
using osu.Game.Online.API.Requests;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;
using Realms;
using WebCommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class SkinSection : SettingsSection
    {
        private SkinDropdown skinDropdown;

        public override LocalisableString Header => SkinSettingsStrings.SkinSectionHeader;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = OsuIcon.SkinB
        };

        public override IEnumerable<LocalisableString> FilterTerms => base.FilterTerms.Concat(new LocalisableString[] { "skins" });

        private readonly List<Live<SkinInfo>> dropdownItems = new List<Live<SkinInfo>>();

        [Resolved]
        private SkinManager skins { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; }

        private IDisposable realmSubscription;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new SettingsItemV2(skinDropdown = new SkinDropdown
                {
                    AlwaysShowSearchBar = true,
                    AllowNonContiguousMatching = true,
                    Caption = SkinSettingsStrings.CurrentSkin,
                    Current = skins.CurrentSkinInfo,
                }),
                new SkinSettingsInfoCard(),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            realmSubscription = realm.RegisterForNotifications(_ => realm.Realm.All<SkinInfo>()
                                                                         .Where(s => !s.DeletePending)
                                                                         .OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase), skinsChanged);

            skinDropdown.Current.BindValueChanged(skin =>
            {
                if (skin.NewValue.ID == SkinInfo.RANDOM_SKIN)
                {
                    // before selecting random, set the skin back to the previous selection.
                    // this is done because at this point it will be random_skin_info, and would
                    // cause SelectRandomSkin to be unable to skip the previous selection.
                    skins.CurrentSkinInfo.Value = skin.OldValue;
                    skins.SelectRandomSkin();
                }
            });
        }

        private void skinsChanged(IRealmCollection<SkinInfo> sender, ChangeSet changes)
        {
            // This can only mean that realm is recycling, else we would see the protected skins.
            // Because we are using `Live<>` in this class, we don't need to worry about this scenario too much.
            if (!sender.Any())
                return;
            // For simplicity repopulate the full list.
            dropdownItems.Clear();
            dropdownItems.AddRange(skins.GetAllUsableSkins());

            Schedule(() => skinDropdown.Items = dropdownItems);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            realmSubscription?.Dispose();
        }

        private partial class SkinDropdown : FormDropdown<Live<SkinInfo>>
        {
            protected override LocalisableString GenerateItemText(Live<SkinInfo> item) => item.ToString();
        }

        public partial class EditSkinButton : ShearedButton
        {
            [BackgroundDependencyLoader(permitNulls: true)]
            private void load([CanBeNull] SkinEditorOverlay skinEditor)
            {
                Text = CommonStrings.MenuBarEdit;
                TextSize = 14;
                Action = () => skinEditor?.ToggleVisibility();
            }
        }

        /// <summary>
        /// Matches song select <c>PanelUpdateBeatmapButton</c> (sync icon + Update), with shear for sheared wedges.
        /// </summary>
        public partial class UpdateSkinButton : OsuAnimatedButton
        {
            private const float icon_size = 12;

            [Resolved]
            private SkinDownloader skinDownloader { get; set; }

            [Resolved]
            private SkinManager skins { get; set; }

            private APIOnlineSkin onlineSkin;
            private Live<SkinInfo> existingSkin;

            private SpriteIcon icon;
            private Box progressFill;

            public UpdateSkinButton()
            {
                AutoSizeAxes = Axes.X;
                Height = 22f;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Content.Anchor = Anchor.Centre;
                Content.Origin = Anchor.Centre;
                Content.Shear = OsuGame.SHEAR;
                Content.CornerRadius = 4;
                Content.CornerExponent = 2;

                Content.AddRange(new Drawable[]
                {
                    progressFill = new Box
                    {
                        Colour = Color4.White,
                        Alpha = 0.2f,
                        Blending = BlendingParameters.Additive,
                        RelativeSizeAxes = Axes.Both,
                        Width = 0,
                    },
                    new FillFlowContainer
                    {
                        Padding = new MarginPadding { Horizontal = 5, Vertical = 3 },
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(4),
                        Shear = -OsuGame.SHEAR,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Size = new Vector2(icon_size),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Child = icon = new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.SyncAlt,
                                    Size = new Vector2(icon_size),
                                },
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                                Text = WebCommonStrings.ButtonsUpdate,
                            }
                        }
                    },
                });

                Action = updateSkin;
            }

            public void SetTarget(APIOnlineSkin online, Live<SkinInfo> existing)
            {
                onlineSkin = online;
                existingSkin = existing;
                attachExistingDownload();
            }

            public void ClearTarget()
            {
                onlineSkin = null;
                existingSkin = null;
                Enabled.Value = false;
                progressFill.ResizeWidthTo(0, 100, Easing.OutQuint);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                skinDownloader.DownloadBegan += onDownloadBegan;
                skinDownloader.DownloadCompleted += onDownloadFinished;
                skinDownloader.DownloadFailed += onDownloadFinished;
                attachExistingDownload();
                icon.Spin(4000, RotationDirection.Clockwise);
            }

            protected override void Dispose(bool isDisposing)
            {
                if (skinDownloader != null)
                {
                    skinDownloader.DownloadBegan -= onDownloadBegan;
                    skinDownloader.DownloadCompleted -= onDownloadFinished;
                    skinDownloader.DownloadFailed -= onDownloadFinished;
                }

                base.Dispose(isDisposing);
            }

            protected override bool OnHover(HoverEvent e)
            {
                icon.Spin(400, RotationDirection.Clockwise, icon.Rotation);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                icon.Spin(4000, RotationDirection.Clockwise, icon.Rotation);
                base.OnHoverLost(e);
            }

            private void onDownloadBegan(APIOnlineSkin skin, FileWebRequest _) => Schedule(attachExistingDownload);

            private void onDownloadFinished(APIOnlineSkin _) => Schedule(attachExistingDownload);

            private void attachExistingDownload()
            {
                if (onlineSkin == null || existingSkin == null)
                {
                    Enabled.Value = false;
                    progressFill.ResizeWidthTo(0, 100, Easing.OutQuint);
                    return;
                }

                var download = skinDownloader.GetActiveRequest(onlineSkin);

                if (download != null)
                {
                    Enabled.Value = false;
                    download.DownloadProgress += (current, total) =>
                    {
                        float progress = total > 0 ? (float)current / total : 0;
                        Schedule(() => progressFill.ResizeWidthTo(progress, 100, Easing.OutQuint));
                    };
                }
                else
                {
                    Enabled.Value = !skins.CurrentSkin.Disabled;
                    progressFill.ResizeWidthTo(0, 100, Easing.OutQuint);
                }
            }

            private void updateSkin()
            {
                if (onlineSkin == null || existingSkin == null)
                    return;

                skinDownloader.DownloadAndUpdate(onlineSkin, existingSkin);
                attachExistingDownload();
            }
        }

        public partial class ExportSkinButton : ShearedButton
        {
            [Resolved]
            private SkinManager skins { get; set; }

            private Bindable<Skin> currentSkin;

            [BackgroundDependencyLoader]
            private void load()
            {
                Text = CommonStrings.Export;
                TextSize = 14;
                Action = export;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                currentSkin = skins.CurrentSkin.GetBoundCopy();
                currentSkin.BindValueChanged(_ => updateState());
                currentSkin.BindDisabledChanged(_ => updateState(), true);
            }

            private void updateState() => Enabled.Value = !currentSkin.Disabled && currentSkin.Value.SkinInfo.PerformRead(s => !s.Protected);

            private void export()
            {
                try
                {
                    skins.ExportCurrentSkin();
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not export current skin: {e.Message}", level: LogLevel.Error);
                }
            }
        }

        public partial class ViewOnSkinListingButton : ShearedButton
        {
            [Resolved]
            private SkinManager skins { get; set; }

            [Resolved(CanBeNull = true)]
            private SkinSetOverlay skinSetOverlay { get; set; }

            private Bindable<Skin> currentSkin;

            [BackgroundDependencyLoader]
            private void load()
            {
                Text = SkinSettingsStrings.ViewOnSkinListing;
                TextSize = 14;
                Action = viewOnline;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                currentSkin = skins.CurrentSkin.GetBoundCopy();
                currentSkin.BindValueChanged(_ => updateState());
                currentSkin.BindDisabledChanged(_ => updateState(), true);
            }

            private void updateState()
            {
                Enabled.Value = !currentSkin.Disabled
                                && SkinIniVersionHelper.TryGetOnlineSkinId(currentSkin.Value, out _);
            }

            private void viewOnline()
            {
                if (!SkinIniVersionHelper.TryGetOnlineSkinId(currentSkin.Value, out int onlineSkinId))
                    return;

                skinSetOverlay?.FetchAndShowSkin(onlineSkinId);
            }
        }

        public partial class DeleteSkinButton : ShearedButton
        {
            [Resolved]
            private SkinManager skins { get; set; }

            [Resolved(CanBeNull = true)]
            private IDialogOverlay dialogOverlay { get; set; }

            private Bindable<Skin> currentSkin;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Text = WebCommonStrings.ButtonsDelete;
                TextSize = 14;
                DarkerColour = colours.DangerousButtonColour;
                LighterColour = colours.Pink1;
                TextColour = Colour4.White;
                Action = delete;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                currentSkin = skins.CurrentSkin.GetBoundCopy();
                currentSkin.BindValueChanged(_ => updateState());
                currentSkin.BindDisabledChanged(_ => updateState(), true);
            }

            private void updateState() => Enabled.Value = !currentSkin.Disabled && currentSkin.Value.SkinInfo.PerformRead(s => !s.Protected);

            private void delete()
            {
                dialogOverlay?.Push(new SkinDeleteDialog(currentSkin.Value));
            }
        }

        public partial class SkinDeleteDialog : DeletionDialog
        {
            private readonly Skin skin;

            public SkinDeleteDialog(Skin skin)
            {
                this.skin = skin;
                BodyText = skin.SkinInfo.Value.Name;
            }

            [BackgroundDependencyLoader]
            private void load(SkinManager manager)
            {
                DangerousAction = () =>
                {
                    manager.Delete(skin.SkinInfo.Value);
                    manager.CurrentSkinInfo.SetDefault();
                };
            }
        }
    }
}
