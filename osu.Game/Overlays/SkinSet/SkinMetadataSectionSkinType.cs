using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinMetadataSectionSkinType : SkinMetadataSection
    {
        public SkinMetadataSectionSkinType()
            : base("Skin Type")
        {
        }

        protected override void AddMetadata(string metadata, LinkFlowContainer loaded)
        {
            loaded.AddText(metadata);
        }
    }
}
