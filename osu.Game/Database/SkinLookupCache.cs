// Copyright (c) jvnkosu! team, MIT license
// See the LICENCE file in the repository root for full license text.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Database
{
    public partial class SkinLookupCache : MemoryCachingComponent<int, APIOnlineSkin>
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        protected override bool CacheNullValues => false;

        /// <summary>
        /// Perform an API lookup on the specified skin, populating an <see cref="APIOnlineSkin"/> model.
        /// </summary>
        public Task<APIOnlineSkin?> GetSkinAsync(int skinId, CancellationToken token = default) => GetAsync(skinId, token);

        /// <summary>
        /// Store skins retrieved from listing searches so subsequent lookups can be served from cache.
        /// </summary>
        public void StoreSkins(IEnumerable<APIOnlineSkin> skins)
        {
            foreach (var skin in skins)
            {
                if (skin.OnlineID > 0)
                    CacheValue(skin.OnlineID, skin);
            }
        }

        public bool TryGetCached(int skinId, [MaybeNullWhen(false)] out APIOnlineSkin skin) => CheckExists(skinId, out skin);

        protected override async Task<APIOnlineSkin?> ComputeValueAsync(int lookup, CancellationToken token = default)
        {
            var request = new GetSkinRequest(lookup);
            await api.PerformAsync(request).ConfigureAwait(false);
            return request.Response;
        }
    }
}
