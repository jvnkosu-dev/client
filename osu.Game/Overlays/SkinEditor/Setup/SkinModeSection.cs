// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.SkinEditor.Setup
{
    public partial class SkinModeSection : SkinSetupSection
    {
        public override LocalisableString Title => "Mode";

        private readonly BindableList<string> modifiedModes = new BindableList<string>();
        private readonly Dictionary<string, BindableBool> toggles = new Dictionary<string, BindableBool>();

        private Bindable<Skin> currentSkin = null!;
        private bool reloading;
        private bool suppressPersist;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            var rulesetFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 5),
            };

            foreach (var ruleset in SkinModifiedModesHelper.GetLegacyRulesetsInOrder(rulesets))
            {
                var toggle = new BindableBool();
                toggles[ruleset.ShortName] = toggle;

                toggle.BindValueChanged(state =>
                {
                    updateModifiedModes(ruleset.ShortName, state.NewValue);

                    if (!reloading && !suppressPersist)
                        persist();
                });

                rulesetFlow.Add(new FormCheckBox
                {
                    RelativeSizeAxes = Axes.X,
                    Caption = ruleset.Name,
                    HintText = $"Skin contains changes for {ruleset.Name}",
                    Current = { BindTarget = toggle },
                });
            }

            Children = new Drawable[]
            {
                rulesetFlow,
            };

            modifiedModes.BindCollectionChanged((_, _) => syncTogglesFromList());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentSkin = Skins.CurrentSkin.GetBoundCopy();
            currentSkin.BindValueChanged(_ => reloadFromSkin(), true);
        }

        private void reloadFromSkin()
        {
            reloading = true;

            modifiedModes.Clear();

            foreach (string mode in SkinIniVersionHelper.ParseModifiedModes(currentSkin.Value.Configuration.ModifiedModes))
                modifiedModes.Add(mode);

            reloading = false;
        }

        private void syncTogglesFromList()
        {
            suppressPersist = true;

            foreach (var (shortName, toggle) in toggles)
            {
                bool shouldBeOn = modifiedModes.Contains(shortName, StringComparer.OrdinalIgnoreCase);

                if (toggle.Value != shouldBeOn)
                    toggle.Value = shouldBeOn;
            }

            suppressPersist = false;
        }

        private void updateModifiedModes(string shortName, bool enabled)
        {
            if (enabled)
            {
                if (!modifiedModes.Contains(shortName, StringComparer.OrdinalIgnoreCase))
                    modifiedModes.Add(shortName);
            }
            else
            {
                modifiedModes.RemoveAll(m => string.Equals(m, shortName, StringComparison.OrdinalIgnoreCase));
            }
        }

        private void persist()
        {
            if (reloading)
                return;

            if (Skins.EnsureMutableSkin())
                return;

            var skin = currentSkin.Value;
            var skinInfo = skin.SkinInfo.Value;

            Skins.PersistSetupMetadata(
                skin,
                SkinIniVersionHelper.SanitizeUploadName(skinInfo.Name),
                skinInfo.Creator == @"Unknown" ? string.Empty : skinInfo.Creator,
                SkinIniVersionHelper.GetSkinVersion(skin, useDefaultIfMissing: true),
                skin.Configuration.Description ?? string.Empty,
                skin.Configuration.Tags ?? string.Empty,
                SkinModifiedModesHelper.FormatForUpload(modifiedModes));
        }
    }
}
