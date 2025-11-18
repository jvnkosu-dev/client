// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.Play.HUD
{
    public partial class KeyCounterBindingTrigger<T> : InputTrigger, IKeyBindingHandler<T>
        where T : struct
    {
        public IKeyBinding Key { get; }
        public T Action { get; }

        [Resolved]
        private ReadableKeyCombinationProvider keyCombinationProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(ReadableKeyCombinationProvider keys)
        {
            keyCombinationProvider = keys;
        }

        private string getName(IKeyBinding key, T action)
        {
            return keyCombinationProvider?.GetReadableString(key.KeyCombination) ?? $"B{(int)(object)action + 1}";
        }
        public KeyCounterBindingTrigger(IKeyBinding key, T action)
            : base("")
        {
            Key = key;
            Action = action;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Name = getName(Key, Action);
        }

        public bool OnPressed(KeyBindingPressEvent<T> e)
        {
            if (!EqualityComparer<T>.Default.Equals(e.Action, Action))
                return false;

            Activate(Clock.Rate >= 0);
            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<T> e)
        {
            if (!EqualityComparer<T>.Default.Equals(e.Action, Action))
                return;

            Deactivate(Clock.Rate >= 0);
        }
    }
}
