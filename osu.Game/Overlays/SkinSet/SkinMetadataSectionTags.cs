using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinMetadataSectionTags : SkinMetadataSection
    {
        public SkinMetadataSectionTags()
            : base("Skin Tags")
        {
        }

        protected override void AddMetadata(string metadata, LinkFlowContainer loaded)
        {
            string[] tags = metadata.Split(' ');

            for (int i = 0; i <= tags.Length - 1; i++)
            {
                string tag = tags[i];

                if (string.IsNullOrWhiteSpace(tag))
                    continue;

                loaded.AddLink(tag, LinkAction.SearchSkin, tag);

                if (i != tags.Length - 1)
                    loaded.AddText(" ");
            }
        }
    }
}
