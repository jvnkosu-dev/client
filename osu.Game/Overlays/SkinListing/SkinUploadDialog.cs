// Copyright (c) jvnkosu! team, MIT license
// See the LICENCE file in the repository root for full license text

using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

// to be revamped and used later
namespace osu.Game.Overlays.SkinListing
{
    public partial class SkinUploadDialog : FocusedOverlayContainer
    {
        private OsuTextBox descriptionBox = null!;
        private OsuSpriteText fileLabel = null!;
        private string? selectedFilePath;

        [Resolved]
        private SkinUploader uploader { get; set; } = null!;

        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        public SkinUploadDialog()
        {
            Size = new Vector2(500, 300);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray1,
                    Alpha = 0.95f,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(20),
                    Spacing = new Vector2(0, 15),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "Uploading skin...",
                            Font = OsuFont.GetFont(size: 24, weight: FontWeight.Bold),
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 5),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "Description:",
                                    Font = OsuFont.GetFont(size: 14)
                                },
                                descriptionBox = new OsuTextBox
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 40,
                                    PlaceholderText = "Put your description here! (Optional)"
                                }
                            }
                        },
                        fileLabel = new OsuSpriteText
                        {
                            Text = "no file specified!",
                            Font = OsuFont.GetFont(size: 14),
                            Colour = colours.Red,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(10, 0),
                            Children = new Drawable[]
                            {
                                new RoundedButton
                                {
                                    Width = 100,
                                    Height = 40,
                                    Text = "Cancel",
                                    Action = Hide,
                                    BackgroundColour = colours.Gray3
                                },
                                new RoundedButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Width = 0.7f,
                                    Height = 40,
                                    Text = "Upload!",
                                    Action = upload,
                                    BackgroundColour = colours.Blue
                                }
                            }
                        }
                    }
                }
            };
        }

        public void SelectFile(string path)
        {
            selectedFilePath = path;
            fileLabel.Text = $"Выбран файл: {Path.GetFileName(path)}";
            fileLabel.Colour = Color4.White;
        }

        private void upload()
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                notifications.Post(new SimpleNotification { Text = "Сначала выберите файл .osk!" });
                return;
            }

            string path = selectedFilePath;
            string description = descriptionBox.Text;

            Hide();

            var notification = new ProgressNotification { Text = "Загрузка скина на сервер..." };
            notifications.Post(notification);

            Task.Run(async () =>
            {
                bool success = await uploader.UploadSkinAsync(path, description).ConfigureAwait(false);
                Schedule(() =>
                {
                    if (success)
                    {
                        notification.State = ProgressNotificationState.Completed;
                        notification.Text = "Скин успешно загружен!";
                    }
                    else
                    {
                        notification.State = ProgressNotificationState.Cancelled;
                        notification.Text = "Ошибка при загрузке скина.";
                    }
                });
            });
        }

        protected override void PopIn() => this.FadeIn(200);
        protected override void PopOut() => this.FadeOut(200);
    }
}
