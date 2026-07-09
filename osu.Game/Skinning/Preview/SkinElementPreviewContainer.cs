// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;

namespace osu.Game.Skinning.Preview
{
    /// <summary>
    /// Wraps a preview drawable in a <see cref="SkinProvidingContainer"/> with the ruleset skin transformer applied.
    /// </summary>
    public partial class SkinElementPreviewContainer : SkinProvidingContainer
    {
        private readonly ISkin skin;
        private readonly Ruleset ruleset;

        public SkinElementPreviewContainer(ISkin skin, Ruleset ruleset, Drawable previewContent)
            : base()
        {
            this.skin = skin;
            this.ruleset = ruleset;
            RelativeSizeAxes = Axes.Both;
            Child = previewContent;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var skinManager = parent.Get<SkinManager>();
            var sources = new List<ISkin> { getTransformedSkin(skin) };

            if (skin is LegacySkin && skin != skinManager.DefaultClassicSkin)
                sources.Add(getTransformedSkin(skinManager.DefaultClassicSkin));

            SetSources(sources);

            return base.CreateChildDependencies(parent);
        }

        private ISkin getTransformedSkin(ISkin source)
            => ruleset.CreateSkinTransformer(source, ruleset.CreatePreviewBeatmap()) ?? source;

        protected override bool AllowFallingBackToParent => false;
    }
}
