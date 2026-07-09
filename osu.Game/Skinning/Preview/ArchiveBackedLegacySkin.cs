// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Stores;
using osu.Game.IO;

namespace osu.Game.Skinning.Preview
{
    /// <summary>
    /// A <see cref="LegacySkin"/> backed by an in-memory archive for temporary preview usage.
    /// </summary>
    public partial class ArchiveBackedLegacySkin : LegacySkin
    {
        public ArchiveBackedLegacySkin(SkinInfo skin, IStorageResourceProvider resources, IResourceStore<byte[]> archive)
            : base(skin, resources, archive, useRealmStorage: false)
        {
        }
    }
}
