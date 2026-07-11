using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Submission;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinListing.Submission
{
    public partial class SkinSubmissionOverlay : WizardOverlay
    {
        [Resolved]
        private SkinSubmissionSettings settings { get; set; } = null!;

        public SkinSubmissionSettings Settings => settings;

        [Resolved]
        private Storage storage { get; set; } = null!;

        public bool IsUpdate => settings.IsUpdate.Value;

        public SkinSubmissionOverlay()
            : base(OverlayColourScheme.Aquamarine)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (!settings.IsUpdate.Value)
            {
                AddStep<ScreenContentPermissions>();
                AddStep<ScreenSkinFrequentlyAskedQuestions>();
            }

            AddStep<ScreenSkinSubmissionSettings>();
            RefreshHeaderCopy();
        }

        public void RefreshHeaderCopy()
        {
            if (IsUpdate)
            {
                Header.Title = SkinSubmissionStrings.SkinUpdateTitle;
                Header.Description = SkinSubmissionStrings.SkinUpdateDescription;
            }
            else
            {
                Header.Title = SkinSubmissionStrings.SkinSubmissionTitle;
                Header.Description = SkinSubmissionStrings.SkinSubmissionDescription;
            }
        }

        public async Task ExportSkinToTempAsync(Skin skin)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"skin-upload-{System.Guid.NewGuid()}.osk");

            using (var stream = File.Create(tempPath))
            {
                var exporter = new LegacySkinExporter(storage);
                await exporter.ExportToStreamAsync(skin.SkinInfo, stream).ConfigureAwait(false);
            }

            settings.SkinFilePath = tempPath;
        }
    }
}
