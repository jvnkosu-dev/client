using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinMetadataSectionDescription : SkinMetadataSection
    {
        public SkinMetadataSectionDescription()
            : base("Description")
        {
        }

        protected override void AddMetadata(string metadata, LinkFlowContainer loaded)
        {
            loaded.AddText(metadata);
        }
    }
}
