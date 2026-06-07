// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Database;
using osu.Game.Scoring;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Online;
using osu.Game.Online.Multiplayer;
using osuTK;
using System.Linq.Expressions;

namespace osu.Game.Screens.Play
{
    public partial class SaveScoreOnQuitButton : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        private readonly Bindable<DownloadState> state = new Bindable<DownloadState>();

        private DownloadButton button = null!;

        private bool saveScore = false;
        private bool canImport = true;
        public bool SaveScore => saveScore;
        public Action<bool>? OnAct;

        public SaveScoreOnQuitButton(bool canImport = true)
        {
            Size = new Vector2(50, 30);
            this.canImport = canImport;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame? game, Player? player)
        {
            InternalChild = button = new DownloadButton
            {
                RelativeSizeAxes = Axes.Both,
                State = { BindTarget = state },
                Action = () =>
                {
                    switch (state.Value)
                    {
                        case DownloadState.LocallyAvailable:
                        {
                            saveScore = false;
                            state.Value = DownloadState.NotDownloaded;
                            button.State.Value = DownloadState.NotDownloaded;
                            break;
                        }

                        default:
                        {
                            saveScore = true;
                            state.Value = DownloadState.LocallyAvailable;
                            button.State.Value = DownloadState.LocallyAvailable;
                            break;
                        }
                    }
                    OnAct?.Invoke(saveScore);
                }
            };


            state.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case DownloadState.LocallyAvailable:
                        button.TooltipText = @"replay will be saved upon quitting";
                        button.Enabled.Value = true;
                        break;

                    default:
                        if (this.canImport)
                        {
                            button.TooltipText = @"save score on quit";
                            button.Enabled.Value = true;
                        }
                        else
                        {
                            button.TooltipText = @"replay cannot be saved at this time";
                            button.Enabled.Value = false;
                        }

                        break;
                }
            }, true);
        }

        #region Export via hotkey logic (also in ReplayDownloadButton)

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.SaveReplay:
                    button.TriggerClick();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        #endregion
    }
}
