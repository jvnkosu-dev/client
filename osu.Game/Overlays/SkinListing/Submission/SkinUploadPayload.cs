using System.Collections.Generic;

namespace osu.Game.Overlays.SkinListing.Submission
{
    public class SkinUploadPayload
    {
        public string FilePath { get; init; } = string.Empty;

        public string? PreviewFilePath { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Version { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string Author { get; init; } = string.Empty;

        public string Tags { get; init; } = string.Empty;

        public IReadOnlyList<string> ModifiedModes { get; init; } = new List<string>();

        public string EngineType { get; init; } = string.Empty;

        /// <summary>
        /// When set, the payload is submitted as an update to an existing online skin entry.
        /// </summary>
        public int? OnlineSkinId { get; init; }
    }
}
