// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Online.API.Requests;

namespace osu.Game.Skinning.Preview
{
    /// <summary>
    /// Downloads online skins into memory for temporary preview usage without importing them.
    /// </summary>
    public partial class OnlineSkinPreviewProvider : Component
    {
        private const int max_cached_previews = 10;

        private readonly Dictionary<int, SkinPreviewHandle> cache = new Dictionary<int, SkinPreviewHandle>();
        private readonly List<int> cacheOrder = new List<int>();
        private readonly HashSet<int> activeDownloads = new HashSet<int>();
        private readonly HashSet<int> loadedThumbnails = new HashSet<int>();
        private readonly HashSet<int> activePreviewSessions = new HashSet<int>();
        private readonly Dictionary<int, List<Action<SkinPreviewHandle>>> pendingSuccessCallbacks = new Dictionary<int, List<Action<SkinPreviewHandle>>>();
        private readonly Dictionary<int, List<Action>> pendingFailureCallbacks = new Dictionary<int, List<Action>>();

        [Resolved]
        private SkinManager skinManager { get; set; } = null!;

        /// <summary>
        /// Retrieve a preview handle for an online skin, downloading it if required.
        /// </summary>
        public void GetPreview(APIOnlineSkin onlineSkin, Action<SkinPreviewHandle> onSuccess, Action? onFailure = null)
        {
            if (cache.TryGetValue(onlineSkin.OnlineID, out var cached))
            {
                cacheOrder.Remove(onlineSkin.OnlineID);
                cacheOrder.Add(onlineSkin.OnlineID);
                onSuccess(cached);
                return;
            }

            if (activeDownloads.Contains(onlineSkin.OnlineID))
            {
                if (!pendingSuccessCallbacks.TryGetValue(onlineSkin.OnlineID, out var callbacks))
                {
                    callbacks = new List<Action<SkinPreviewHandle>>();
                    pendingSuccessCallbacks[onlineSkin.OnlineID] = callbacks;
                }

                callbacks.Add(onSuccess);

                if (onFailure != null)
                {
                    if (!pendingFailureCallbacks.TryGetValue(onlineSkin.OnlineID, out var failureCallbacks))
                    {
                        failureCallbacks = new List<Action>();
                        pendingFailureCallbacks[onlineSkin.OnlineID] = failureCallbacks;
                    }

                    failureCallbacks.Add(onFailure);
                }

                return;
            }

            string? previewUrl = onlineSkin.GetPreviewRequestUrl();
            string? downloadUrl = string.IsNullOrWhiteSpace(onlineSkin.DownloadUrl) ? null : onlineSkin.DownloadUrl;

            if (string.IsNullOrWhiteSpace(previewUrl) && string.IsNullOrWhiteSpace(downloadUrl))
            {
                onFailure?.Invoke();
                return;
            }

            pendingSuccessCallbacks[onlineSkin.OnlineID] = new List<Action<SkinPreviewHandle>> { onSuccess };

            if (onFailure != null)
                pendingFailureCallbacks[onlineSkin.OnlineID] = new List<Action> { onFailure };

            beginDownload(onlineSkin, previewUrl, downloadUrl);
        }

        public void Invalidate(int onlineSkinId)
        {
            if (!cache.Remove(onlineSkinId, out var handle))
                return;

            handle.Dispose();
            cacheOrder.Remove(onlineSkinId);
        }

        /// <summary>
        /// A skin thumbnail has been loaded and is currently retained on-screen.
        /// </summary>
        public void NotifyThumbnailLoaded(int onlineSkinId)
        {
            if (onlineSkinId > 0)
                loadedThumbnails.Add(onlineSkinId);
        }

        /// <summary>
        /// A skin thumbnail has been unloaded after leaving the screen.
        /// </summary>
        public void NotifyThumbnailUnloaded(int onlineSkinId)
        {
            if (onlineSkinId <= 0)
                return;

            loadedThumbnails.Remove(onlineSkinId);
            tryInvalidate(onlineSkinId);
        }

        /// <summary>
        /// A temporary in-overlay skin preview session has started for an online skin archive.
        /// </summary>
        public void NotifyPreviewSessionStarted(int onlineSkinId)
        {
            if (onlineSkinId > 0)
                activePreviewSessions.Add(onlineSkinId);
        }

        /// <summary>
        /// A temporary in-overlay skin preview session has ended.
        /// </summary>
        public void NotifyPreviewSessionEnded(int onlineSkinId)
        {
            if (onlineSkinId <= 0)
                return;

            activePreviewSessions.Remove(onlineSkinId);
            tryInvalidate(onlineSkinId);
        }

        private void tryInvalidate(int onlineSkinId)
        {
            if (loadedThumbnails.Contains(onlineSkinId) || activePreviewSessions.Contains(onlineSkinId))
                return;

            Invalidate(onlineSkinId);
        }

        private void beginDownload(APIOnlineSkin onlineSkin, string? previewUrl, string? downloadUrl)
        {
            Task.Run(async () =>
            {
                Schedule(() => activeDownloads.Add(onlineSkin.OnlineID));

                try
                {
                    byte[]? data = await SkinPreviewDownloader.DownloadAsync(previewUrl, downloadUrl).ConfigureAwait(false);

                    Schedule(() =>
                    {
                        activeDownloads.Remove(onlineSkin.OnlineID);
                        completeRequest(onlineSkin, data);
                    });
                }
                catch (Exception ex)
                {
                    Logger.Log($"Skin preview download failed for skin {onlineSkin.OnlineID}: {ex.Message}", level: LogLevel.Debug);

                    Schedule(() =>
                    {
                        activeDownloads.Remove(onlineSkin.OnlineID);
                        failRequest(onlineSkin.OnlineID);
                    });
                }
            });
        }

        private void completeRequest(APIOnlineSkin onlineSkin, byte[]? data)
        {
            activeDownloads.Remove(onlineSkin.OnlineID);

            if (data == null)
            {
                failRequest(onlineSkin.OnlineID);
                return;
            }

            try
            {
                var memoryStream = new MemoryStream(data);
                var archive = new ZipArchiveReader(memoryStream, onlineSkin.Name);

                var skinInfo = new SkinInfo(onlineSkin.Name, onlineSkin.Creator)
                {
                    InstantiationInfo = resolveInstantiationInfo(archive, onlineSkin),
                };

                var skin = createPreviewSkin(skinInfo, skinManager, archive);
                var handle = new SkinPreviewHandle(skin, memoryStream, archive);

                cache[onlineSkin.OnlineID] = handle;
                cacheOrder.Add(onlineSkin.OnlineID);
                trimCache();

                invokeSuccessCallbacks(onlineSkin.OnlineID, handle);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create online skin preview");
                failRequest(onlineSkin.OnlineID);
            }
        }

        private void trimCache()
        {
            while (cacheOrder.Count > max_cached_previews)
            {
                int oldestId = cacheOrder[0];
                cacheOrder.RemoveAt(0);

                if (cache.Remove(oldestId, out var handle))
                    handle.Dispose();
            }
        }

        private void failRequest(int onlineSkinId)
        {
            activeDownloads.Remove(onlineSkinId);
            invokeFailureCallbacks(onlineSkinId);
        }

        private void invokeSuccessCallbacks(int onlineSkinId, SkinPreviewHandle handle)
        {
            if (!pendingSuccessCallbacks.Remove(onlineSkinId, out var callbacks))
                return;

            pendingFailureCallbacks.Remove(onlineSkinId);

            foreach (var callback in callbacks)
                callback(handle);
        }

        private void invokeFailureCallbacks(int onlineSkinId)
        {
            pendingSuccessCallbacks.Remove(onlineSkinId);

            if (!pendingFailureCallbacks.Remove(onlineSkinId, out var callbacks))
                return;

            foreach (var callback in callbacks)
                callback();
        }

        private static Skin createPreviewSkin(SkinInfo skinInfo, IStorageResourceProvider resources, IResourceStore<byte[]> archive)
        {
            switch (SkinEngineTypeHelper.FromInstantiationInfo(skinInfo.InstantiationInfo))
            {
                case SkinEngineType.Triangles:
                    return new TrianglesSkin(skinInfo, resources, archive, useRealmStorage: false);

                case SkinEngineType.Argon:
                    return new ArgonSkin(skinInfo, resources, archive, useRealmStorage: false);

                case SkinEngineType.ArgonPro:
                    return new ArgonProSkin(skinInfo, resources, archive, useRealmStorage: false);

                default:
                    return new ArchiveBackedLegacySkin(skinInfo, resources, archive);
            }
        }

        private static string resolveInstantiationInfo(ZipArchiveReader archive, APIOnlineSkin onlineSkin)
        {
            byte[]? skinInfoBytes = archive.Get("skininfo.json");

            if (skinInfoBytes != null)
            {
                try
                {
                    var deserialised = JsonConvert.DeserializeObject<SkinInfo>(Encoding.UTF8.GetString(skinInfoBytes));

                    if (!string.IsNullOrWhiteSpace(deserialised?.InstantiationInfo))
                        return deserialised.InstantiationInfo;
                }
                catch
                {
                    // ignored and fallback below.
                }
            }

            return SkinEngineTypeHelper.GetInstantiationInfo(SkinEngineTypeHelper.GetEngineType(onlineSkin));
        }
    }
}
