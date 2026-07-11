// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps.Formats;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// An empty skin configuration.
    /// </summary>
    public class SkinConfiguration : IHasComboColours, IHasCustomColours
    {
        public readonly SkinInfo SkinInfo = new SkinInfo();

        public const decimal LATEST_VERSION = 2.7m;

        /// <summary>
        /// Whether to allow <see cref="DefaultComboColours"/> as a fallback list for when no combo colours are provided.
        /// </summary>
        internal bool AllowDefaultComboColoursFallback = true;

        /// <summary>
        /// Legacy version of this skin.
        /// </summary>
        public decimal? LegacyVersion { get; internal set; }

        /// <summary>
        /// User-facing skin release version (jvnko extension), stored as <c>SkinVersion</c> in skin.ini.
        /// </summary>
        public string SkinVersion { get; internal set; } = string.Empty;

        /// <summary>
        /// Skin engine type (jvnko extension), stored as <c>SkinType</c> in skin.ini.
        /// </summary>
        public string SkinType { get; internal set; } = string.Empty;

        /// <summary>
        /// Comma-separated modified mode short names (jvnko extension), stored as <c>ModifiedModes</c> in skin.ini.
        /// </summary>
        public string ModifiedModes { get; internal set; } = string.Empty;

        /// <summary>
        /// Online listing ID (jvnko extension), stored as <c>OnlineSkinID</c> in skin.ini.
        /// </summary>
        public int OnlineSkinId { get; internal set; }

        /// <summary>
        /// Skin description (jvnko extension), stored as <c>Description</c> in skin.ini.
        /// </summary>
        public string Description { get; internal set; } = string.Empty;

        /// <summary>
        /// Space-separated listing tags (jvnko extension), stored as <c>Tags</c> in skin.ini.
        /// </summary>
        public string Tags { get; internal set; } = string.Empty;

        public enum LegacySetting
        {
            Version,
            ComboPrefix,
            ComboOverlap,
            ScorePrefix,
            ScoreOverlap,
            HitCirclePrefix,
            HitCircleOverlap,
            AnimationFramerate,
            LayeredHitSounds,
            AllowSliderBallTint,
            InputOverlayText,
        }

        public static List<Color4> DefaultComboColours { get; } = new List<Color4>
        {
            new Color4(255, 192, 0, 255),
            new Color4(0, 202, 0, 255),
            new Color4(18, 124, 255, 255),
            new Color4(242, 24, 57, 255),
        };

        public List<Color4> CustomComboColours { get; set; } = new List<Color4>();

        public IReadOnlyList<Color4>? ComboColours
        {
            get
            {
                if (CustomComboColours.Count > 0)
                    return CustomComboColours;

                if (AllowDefaultComboColoursFallback)
                    return DefaultComboColours;

                return null;
            }
        }

        public Dictionary<string, Color4> CustomColours { get; } = new Dictionary<string, Color4>();

        public readonly Dictionary<string, string> ConfigDictionary = new Dictionary<string, string>();
    }
}
