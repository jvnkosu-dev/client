using System.Globalization;
using System.Text;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.SkinSet
{
    public partial class SkinMetadataSectionDescription : SkinMetadataSection
    {
        /// <summary>
        /// Words longer than this are split into per-character parts so the text flow can wrap mid-word.
        /// </summary>
        private const int max_unbroken_length = 16;

        public SkinMetadataSectionDescription()
            : base("Description")
        {
        }

        protected override void AddMetadata(string metadata, LinkFlowContainer loaded)
        {
            addWrappableText(loaded, metadata);
        }

        /// <summary>
        /// Adds text with forced wrap opportunities inside unbroken runs.
        /// Text flow only wraps between separate text parts.
        /// </summary>
        private static void addWrappableText(LinkFlowContainer flow, string text)
        {
            var current = new StringBuilder();

            void flush()
            {
                if (current.Length == 0)
                    return;

                string value = current.ToString();
                current.Clear();

                if (value.Length <= max_unbroken_length)
                {
                    flow.AddText(value);
                    return;
                }

                foreach (char c in value)
                    flow.AddText(c.ToString());
            }

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\n')
                {
                    flush();
                    flow.NewParagraph();
                    continue;
                }

                current.Append(c);

                // Match TextChunk.SplitWords break opportunities (separator stays with the preceding word).
                if (i < text.Length - 1
                    && (char.IsSeparator(c)
                        || char.IsControl(c)
                        || char.GetUnicodeCategory(c) == UnicodeCategory.DashPunctuation
                        || c is '/' or '\\'
                        || (isCjkCharacter(c) && !char.IsPunctuation(text[i + 1]))))
                {
                    flush();
                }
            }

            flush();
        }

        private static bool isCjkCharacter(char c) => c >= '\x2E80' && c <= '\x9FFF';
    }
}
