// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public partial class ShearedToggleButton : ShearedButton
    {
        private Sample? sampleOff;
        private Sample? sampleOn;

        /// <summary>
        /// Sheared toggle buttons by default play two samples when toggled: a click and a toggle (on/off).
        /// Sometimes this might be too much. Setting this to <c>false</c> will silence the toggle sound.
        /// </summary>
        protected virtual bool PlayToggleSamples => true;

        /// <summary>
        /// Whether this button is currently toggled to an active state.
        /// </summary>
        public BindableBool Active { get; } = new BindableBool();

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleOn = audio.Samples.Get(@"UI/check-on");
            sampleOff = audio.Samples.Get(@"UI/check-off");
        }

        protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverSounds(sampleSet);

        protected override void LoadComplete()
        {
            Active.BindDisabledChanged(disabled => Action = disabled ? null : Active.Toggle, true);
            Active.BindValueChanged(_ =>
            {
                UpdateActiveState();
                playSample();
            });

            UpdateActiveState();
            base.LoadComplete();
        }

        protected virtual void UpdateActiveState()
        {
            DarkerColour = Active.Value ? ColourProvider?.Highlight1 ?? Colour4.Gray : ColourProvider?.Background3 ?? Colour4.DimGray;
            LighterColour = Active.Value ? ColourProvider?.Colour0 ?? Colour4.AliceBlue : ColourProvider?.Background1 ?? Colour4.LightGray;
            TextColour = Active.Value ? ColourProvider?.Background6 ?? Colour4.Black : ColourProvider?.Content1 ?? Colour4.DarkGray;
        }

        private void playSample()
        {
            if (PlayToggleSamples)
            {
                if (Active.Value)
                    sampleOn?.Play();
                else
                    sampleOff?.Play();
            }
        }
    }
}
