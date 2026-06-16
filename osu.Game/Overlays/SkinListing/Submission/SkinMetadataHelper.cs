using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinListing.Submission
{
    public static class SkinMetadataHelper
    {
        public static string ExtractBaseName(string skinName) => SkinIniVersionHelper.ExtractBaseName(skinName);

        public static void PopulateSettingsFromSkin(SkinSubmissionSettings settings, Skin skin)
        {
            var skinInfo = skin.SkinInfo.Value;

            settings.Name.Value = ExtractBaseName(skinInfo.Name);
            settings.Version.Value = SkinIniVersionHelper.GetSkinVersion(skin, useDefaultIfMissing: true);
            settings.Description.Value = string.Empty;

            settings.Author.Value = !string.IsNullOrWhiteSpace(skinInfo.Creator) && skinInfo.Creator != @"Unknown"
                ? skinInfo.Creator
                : string.Empty;
        }
    }
}
