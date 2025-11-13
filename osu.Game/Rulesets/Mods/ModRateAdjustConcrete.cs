// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRateAdjustConcrete : ModRateAdjust
    {
        public override string Name => "Rate Adjust";
        public override LocalisableString Description => "[DEBUG BUILDS ONLY] Set any speed";
        public override string Acronym => "_R";
        private readonly RateAdjustModHelper rateAdjustHelper;

        [SettingSource("Speed decrease", "The actual decrease to apply", SettingControlType = typeof(MultiplierSettingsSlider))]
        public override BindableNumber<double> SpeedChange { get; } = new BindableDouble(0.75)
        {
            MinValue = 0.1, // BASS breaks at lower rates
            MaxValue = 10,
            Precision = 0.01
        };

        [SettingSource("Adjust pitch", "Should pitch be adjusted with speed")]
        public virtual BindableBool AdjustPitch { get; } = new BindableBool();


        protected ModRateAdjustConcrete()
        {
            rateAdjustHelper = new RateAdjustModHelper(SpeedChange);
            rateAdjustHelper.HandleAudioAdjustments(AdjustPitch);
        }

        public override double ScoreMultiplier => 1.0;
        public override void ApplyToTrack(IAdjustableAudioComponent track)
        {
            rateAdjustHelper.ApplyToTrack(track);
        }
        public override bool Ranked => false;
        public override ModType Type => ModType.Special;
    }
}
