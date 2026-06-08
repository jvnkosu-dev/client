// SkinListingFilterControl.cs
using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Overlays.SkinListing
{
    public partial class SkinListingFilterControl : Container
    {
        public Action<string>? SearchStarted;
        public Action? TypingStarted;
        public Action? UploadRequested;

        private OsuTextBox searchTextBox = null!;

        public SkinListingFilterControl()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0),
                        Children = new Drawable[]
                        {
                            searchTextBox = new OsuTextBox
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.8f,
                                Height = 40,
                                PlaceholderText = "Введите название скина или автора...",
                                SelectAllOnFocus = true,
                            },
                            new RoundedButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.2f,
                                Height = 40,
                                Text = "Загрузить скин",
                                Action = () => UploadRequested?.Invoke()
                            }
                        }
                    }
                }
            };

            searchTextBox.Current.ValueChanged += _ => TypingStarted?.Invoke();

            searchTextBox.OnCommit += (_, _) =>
            {
                SearchStarted?.Invoke(searchTextBox.Text);
            };
        }

        public void Search(string query)
        {
            searchTextBox.Text = query;
            SearchStarted?.Invoke(query);
        }
    }
}
