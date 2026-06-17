using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinMetadataSectionVersion : SkinMetadataSection
    {
        public SkinMetadataSectionVersion()
            : base("Version")
        {
        }

        protected override void AddMetadata(string metadata, LinkFlowContainer loaded)
        {
            loaded.AddText(metadata);
        }
    }
}
