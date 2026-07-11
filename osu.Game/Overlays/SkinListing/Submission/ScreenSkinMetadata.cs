// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.SkinListing.Submission
{
    /// <summary>
    /// Review/confirm step before upload. Metadata is edited in the skin editor Setup tab.
    /// </summary>
    public partial class ScreenSkinMetadata : WizardScreen
    {
        [Resolved]
        private SkinSubmissionSettings settings { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private IRenderer renderer { get; set; } = null!;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        private OsuSpriteText nameValue = null!;
        private OsuSpriteText authorValue = null!;
        private OsuSpriteText versionValue = null!;
        private OsuSpriteText descriptionValue = null!;
        private OsuSpriteText tagsValue = null!;
        private OsuSpriteText modesValue = null!;
        private OsuSpriteText typeValue = null!;
        private Container previewContent = null!;

        public override LocalisableString? NextStepText
        {
            get
            {
                if (settings == null)
                    return null;

                return settings.IsUpdate.Value ? SkinMetadataHelper.UpdateUploadActionText : SkinMetadataHelper.UploadActionText;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Content.Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(12),
                Children = new Drawable[]
                {
                    new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 14))
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Text = "Review skin details from the editor Setup tab, then upload. Edit metadata or background in the skin editor if needed.",
                    },
                    previewContent = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 140,
                        Masking = true,
                        CornerRadius = 5,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background4,
                            },
                        }
                    },
                    createRow("Name", out nameValue),
                    createRow("Author", out authorValue),
                    createRow("Version", out versionValue),
                    createRow("Description", out descriptionValue),
                    createRow("Tags", out tagsValue),
                    createRow("Modified rulesets", out modesValue),
                    createRow("Skin type", out typeValue),
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            refreshDisplay();
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            refreshDisplay();
        }

        private void refreshDisplay()
        {
            nameValue.Text = displayOrDash(settings.Name.Value);
            authorValue.Text = displayOrDash(settings.Author.Value);
            versionValue.Text = displayOrDash(settings.Version.Value);
            descriptionValue.Text = displayOrDash(settings.Description.Value);
            tagsValue.Text = displayOrDash(settings.Tags.Value);
            typeValue.Text = displayOrDash(settings.EngineType.Value);

            string modes = SkinModifiedModesHelper.FormatForDisplay(settings.ModifiedModes.ToArray(), rulesets);
            modesValue.Text = string.IsNullOrWhiteSpace(modes) ? "-" : modes;

            updatePreview();
        }

        private void updatePreview()
        {
            previewContent.RemoveAll(d => d is not Box, true);

            var skin = settings.SourceSkin ?? skins.CurrentSkin.Value;
            var texture = SkinBackgroundHelper.GetTexture(skin, renderer, skins);

            if (texture == null)
            {
                previewContent.Add(new OsuSpriteText
                {
                    Text = "No background (bg.*) in skin",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Default.With(size: 16),
                });
                return;
            }

            previewContent.Add(new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fill,
                Texture = texture,
            });
        }

        private static Drawable createRow(LocalisableString label, out OsuSpriteText valueText)
        {
            OsuSpriteText value;

            var row = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 160),
                    new Dimension(),
                },
                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = label,
                            Font = OsuFont.Default.With(size: 14, weight: FontWeight.SemiBold),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        value = new TruncatingSpriteText
                        {
                            Font = OsuFont.Default.With(size: 14),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                        },
                    }
                }
            };

            valueText = value;
            return row;
        }

        private static string displayOrDash(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }
}
