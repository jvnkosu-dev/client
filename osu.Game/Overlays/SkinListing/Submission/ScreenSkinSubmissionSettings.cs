// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Submission
{
    [LocalisableDescription(typeof(BeatmapSubmissionStrings), nameof(BeatmapSubmissionStrings.SubmissionSettings))]
    public partial class ScreenSkinSubmissionSettings : WizardScreen
    {
        private readonly BindableBool openSkinPageAfterSubmission = new BindableBool();

        [Resolved]
        private SkinSubmissionSettings settings { get; set; } = null!;

        public override LocalisableString? NextStepText
        {
            get
            {
                // Called from WizardOverlay.updateButtons before dependencies are injected.
                if (settings == null)
                    return null;

                return settings.IsUpdate.Value
                    ? SkinMetadataHelper.UpdateUploadActionText
                    : SkinMetadataHelper.UploadActionText;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager configManager, OsuColour colours, OverlayColourProvider colourProvider)
        {
            // Local bindable — binding settings.OpenSkinPageAfterSubmission here crashes when
            // revisiting this step (bindable already bound from a previous screen instance).
            configManager.BindWith(OsuSetting.SkinSubmissionOpenPageAfterSubmission, openSkinPageAfterSubmission);

            settings.Target.Disabled = true;

            Content.Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Masking = true,
                        CornerRadius = 5,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colour4.FromHex(@"#FFD966"),
                            },
                            new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE, weight: FontWeight.SemiBold))
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Colour = colourProvider.Background5,
                                Text = SkinSubmissionStrings.StatusSelectionUnavailable,
                                Padding = new MarginPadding(10),
                            },
                        },
                    },
                    new FormEnumDropdown<SkinSubmissionTarget>
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = SkinSubmissionStrings.SkinSubmissionTargetCaption,
                        Current = settings.Target,
                    },
                    new FormCheckBox
                    {
                        Caption = SkinSubmissionStrings.OpenSkinPageAfterSubmission,
                        Current = openSkinPageAfterSubmission,
                    },
                    new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE, weight: FontWeight.Bold))
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Colour = colours.Orange1,
                        Text = SkinSubmissionStrings.CustomElementsDisclaimer,
                        Padding = new MarginPadding { Top = 20 },
                    },
                    new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE, weight: FontWeight.Bold))
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Colour = colours.Orange1,
                        Text = SkinSubmissionStrings.LazerMetadataRegenDisclaimer,
                        Padding = new MarginPadding { Top = 10 },
                    },
                }
            });
        }
    }
}
