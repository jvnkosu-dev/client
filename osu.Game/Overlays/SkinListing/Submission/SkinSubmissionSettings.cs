using System.IO;
using osu.Framework.Bindables;

namespace osu.Game.Overlays.SkinListing.Submission
{
    public class SkinSubmissionSettings
    {
        public Bindable<string> Name { get; } = new Bindable<string>();

        public Bindable<string> Author { get; } = new Bindable<string>();

        public Bindable<string> Version { get; } = new Bindable<string>();

        public Bindable<string> Description { get; } = new Bindable<string>();

        public Bindable<FileInfo?> PreviewFile { get; } = new Bindable<FileInfo?>();

        public string? SkinFilePath { get; set; }
    }
}
