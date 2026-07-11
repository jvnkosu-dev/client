// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Game.Beatmaps.Formats;

namespace osu.Game.Skinning
{
    public class LegacySkinDecoder : LegacyDecoder<SkinConfiguration>
    {
        public LegacySkinDecoder()
            : base(1)
        {
        }

        protected override void ParseLine(SkinConfiguration skin, Section section, string line, bool isPrimaryStream)
        {
            if (section != Section.Colours)
            {
                var pair = SplitKeyVal(line);

                switch (section)
                {
                    case Section.General:
                        switch (pair.Key)
                        {
                            case @"Name":
                                skin.SkinInfo.Name = pair.Value;
                                return;

                            case @"Author":
                                skin.SkinInfo.Creator = pair.Value;
                                return;

                            case @"SkinVersion":
                                skin.SkinVersion = pair.Value;
                                return;

                            case @"SkinType":
                                skin.SkinType = pair.Value;
                                return;

                            case @"ModifiedModes":
                                skin.ModifiedModes = pair.Value;
                                return;

                            case @"OnlineSkinID":
                                if (int.TryParse(pair.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int onlineSkinId) && onlineSkinId > 0)
                                    skin.OnlineSkinId = onlineSkinId;

                                return;

                            case @"Description":
                                skin.Description = pair.Value;
                                return;

                            case @"Tags":
                                skin.Tags = pair.Value;
                                return;

                            case @"ServerLastUpdated":
                                skin.ServerLastUpdated = pair.Value;
                                return;

                            case @"ServerContentLength":
                                if (long.TryParse(pair.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long contentLength) && contentLength > 0)
                                    skin.ServerContentLength = contentLength;

                                return;

                            case @"Version":
                                if (pair.Value == "latest")
                                    skin.LegacyVersion = SkinConfiguration.LATEST_VERSION;
                                else if (decimal.TryParse(pair.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal version))
                                    skin.LegacyVersion = version;

                                return;
                        }

                        break;

                    // osu!catch section only has colour settings
                    // so no harm in handling the entire section
                    case Section.CatchTheBeat:
                        HandleColours(skin, line, true);
                        return;
                }

                if (!string.IsNullOrEmpty(pair.Key))
                    skin.ConfigDictionary[pair.Key] = pair.Value;
            }

            base.ParseLine(skin, section, line, isPrimaryStream);
        }

        protected override SkinConfiguration CreateTemplateObject()
        {
            var config = base.CreateTemplateObject();
            config.LegacyVersion = 1.0m;
            return config;
        }
    }
}
