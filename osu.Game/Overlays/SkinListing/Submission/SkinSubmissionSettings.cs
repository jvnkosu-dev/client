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

        public Bindable<string> Tags { get; } = new Bindable<string>();

        public Bindable<FileInfo?> PreviewFile { get; } = new Bindable<FileInfo?>();

        public BindableList<string> ModifiedModes { get; } = new BindableList<string>();

        public Bindable<string> EngineType { get; } = new Bindable<string>();

        public Bindable<bool> IsUpdate { get; } = new Bindable<bool>();

        public BindableInt OnlineSkinId { get; } = new BindableInt();

        public string? SkinFilePath { get; set; }
    }
}
