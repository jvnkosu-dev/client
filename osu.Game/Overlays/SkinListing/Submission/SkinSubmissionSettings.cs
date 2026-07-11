// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Skinning;

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

        public Bindable<SkinSubmissionTarget> Target { get; } = new Bindable<SkinSubmissionTarget>(SkinSubmissionTarget.WIP);

        public string? SkinFilePath { get; set; }

        /// <summary>
        /// The local skin being uploaded; used for review preview and thumbnail extraction.
        /// </summary>
        public Skin? SourceSkin { get; set; }
    }

    public enum SkinSubmissionTarget
    {
        [LocalisableDescription(typeof(SkinSubmissionStrings), nameof(SkinSubmissionStrings.SkinSubmissionTargetWIP))]
        WIP,

        [LocalisableDescription(typeof(SkinSubmissionStrings), nameof(SkinSubmissionStrings.SkinSubmissionTargetPending))]
        Pending,
    }
}
