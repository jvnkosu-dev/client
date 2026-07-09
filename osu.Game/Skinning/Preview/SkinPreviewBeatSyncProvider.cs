// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Skinning.Preview
{
    /// <summary>
    /// A minimal beat sync source for skin preview drawables that depend on <see cref="IBeatSyncProvider"/>.
    /// </summary>
    public class SkinPreviewBeatSyncProvider : IBeatSyncProvider
    {
        private readonly ControlPointInfo controlPoints = new ControlPointInfo();

        public SkinPreviewBeatSyncProvider(IClock clock)
        {
            Clock = clock;

            // Keep kiai flashes disabled and beats infrequent for static previews.
            controlPoints.Add(0, new TimingControlPoint { BeatLength = 60000 });
            controlPoints.Add(0, new EffectControlPoint { KiaiMode = false });
        }

        public ControlPointInfo? ControlPoints => controlPoints;

        public IClock Clock { get; }

        public ChannelAmplitudes CurrentAmplitudes => new ChannelAmplitudes();
    }
}
