using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Screens;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinListing.Submission
{
    public partial class SkinSubmissionScreen : OsuScreen
    {
        private readonly Skin skin;

        private SkinSubmissionOverlay overlay = null!;

        [Cached]
        private readonly SkinSubmissionSettings settings = new SkinSubmissionSettings();

        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        [Resolved]
        private SkinEditorOverlay? skinEditorOverlay { get; set; }

        protected override bool InitialBackButtonVisibility => false;

        public SkinSubmissionScreen(Skin skin)
        {
            this.skin = skin;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(overlay = new SkinSubmissionOverlay());

            overlay.State.BindValueChanged(state =>
            {
                if (state.NewValue == Visibility.Hidden)
                {
                    allowExit();
                    skinEditorOverlay?.Show();
                    this.Exit();
                }
            });
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            overlay.PopulateMetadataFromSkin(skin);
            overlay.Show();

            Task.Run(async () =>
            {
                try
                {
                    await overlay.ExportSkinToTempAsync(skin).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Schedule(() =>
                    {
                        notifications.Post(new SimpleNotification
                        {
                            Text = $"Не удалось подготовить скин: {ex.Message}",
                        });
                        overlay.Hide();
                    });
                }
            });
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (!BackButtonVisibility.Value)
                return true;

            return base.OnExiting(e);
        }

        private void allowExit() => BackButtonVisibility.Value = true;
    }
}
