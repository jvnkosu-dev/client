using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Extensions;
using osu.Game.Online.API.Requests;

namespace osu.Game.Skinning
{
    public static class SkinEngineTypeHelper
    {
        public static string GetDisplayName(SkinEngineType type) => type switch
        {
            SkinEngineType.Triangles => "Triangles",
            SkinEngineType.Argon => "Argon",
            SkinEngineType.ArgonPro => "ArgonPro",
            _ => "Legacy",
        };

        public static string ToStorageString(SkinEngineType type) => GetDisplayName(type);

        public static string GetInstantiationInfo(SkinEngineType type) => type switch
        {
            SkinEngineType.Triangles => typeof(TrianglesSkin).GetInvariantInstantiationInfo(),
            SkinEngineType.Argon => typeof(ArgonSkin).GetInvariantInstantiationInfo(),
            SkinEngineType.ArgonPro => typeof(ArgonProSkin).GetInvariantInstantiationInfo(),
            _ => typeof(LegacySkin).GetInvariantInstantiationInfo(),
        };

        public static SkinEngineType FromInstantiationInfo(string? instantiationInfo)
        {
            if (string.IsNullOrWhiteSpace(instantiationInfo))
                return SkinEngineType.Legacy;

            // ArgonPro must be checked before Argon.
            if (instantiationInfo.Contains(nameof(ArgonProSkin), StringComparison.Ordinal))
                return SkinEngineType.ArgonPro;

            if (instantiationInfo.Contains(nameof(ArgonSkin), StringComparison.Ordinal))
                return SkinEngineType.Argon;

            if (instantiationInfo.Contains(nameof(TrianglesSkin), StringComparison.Ordinal))
                return SkinEngineType.Triangles;

            return SkinEngineType.Legacy;
        }

        public static SkinEngineType FromSkinInfo(SkinInfo skinInfo) => FromInstantiationInfo(skinInfo.InstantiationInfo);

        public static bool TryParse(string? value, out SkinEngineType type)
        {
            type = SkinEngineType.Legacy;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            foreach (SkinEngineType candidate in Enum.GetValues<SkinEngineType>())
            {
                if (string.Equals(value, GetDisplayName(candidate), StringComparison.OrdinalIgnoreCase)
                    || string.Equals(value, candidate.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    type = candidate;
                    return true;
                }
            }

            return false;
        }

        public static SkinEngineType GetEngineType(APIOnlineSkin skin) =>
            TryParse(skin.EngineType, out var type) ? type : SkinEngineType.Legacy;

        public static string GetDisplayName(APIOnlineSkin skin) => GetDisplayName(GetEngineType(skin));

        public static IEnumerable<APIOnlineSkin> Filter(IEnumerable<APIOnlineSkin> skins, SkinEngineType? selectedType)
        {
            if (selectedType == null)
                return skins;

            return skins.Where(s => GetEngineType(s) == selectedType);
        }
    }
}
