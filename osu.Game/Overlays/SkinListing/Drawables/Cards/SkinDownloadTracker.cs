using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.IO.Network;
using osu.Game.Database;
using osu.Game.Online;
using osu.Game.Online.API.Requests;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinListing.Drawables.Cards
{
    public partial class SkinDownloadTracker : DownloadTracker<APIOnlineSkin>
    {
        [Resolved]
        private SkinDownloader skinDownloader { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private FileWebRequest? attachedRequest;
        private IDisposable? realmSubscription;

        public SkinDownloadTracker(APIOnlineSkin skin)
            : base(skin)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            skinDownloader.DownloadBegan += onDownloadBegan;
            skinDownloader.DownloadCompleted += onDownloadCompleted;
            skinDownloader.DownloadFailed += onDownloadFailed;

            realmSubscription = realm.RegisterForNotifications(
                r => r.All<SkinInfo>().Where(s => !s.DeletePending),
                (items, _) =>
                {
                    bool installed = items.Any(s => SkinDownloader.matchesOnlineSkin(s, TrackedItem));

                    Schedule(() =>
                    {
                        if (attachedRequest != null)
                            return;

                        UpdateState(installed ? DownloadState.LocallyAvailable : DownloadState.NotDownloaded);
                        attachDownload(skinDownloader.GetActiveRequest(TrackedItem));
                    });
                });

            attachDownload(skinDownloader.GetActiveRequest(TrackedItem));
        }

        private void onDownloadBegan(APIOnlineSkin skin, FileWebRequest request)
        {
            if (skin.OnlineID != TrackedItem.OnlineID)
                return;

            Schedule(() => attachDownload(request));
        }

        private void onDownloadCompleted(APIOnlineSkin skin)
        {
            if (skin.OnlineID != TrackedItem.OnlineID)
                return;

            Schedule(() => attachDownload(null));
        }

        private void onDownloadFailed(APIOnlineSkin skin)
        {
            if (skin.OnlineID != TrackedItem.OnlineID)
                return;

            Schedule(() => attachDownload(null));
        }

        private void attachDownload(FileWebRequest? request)
        {
            detachRequest();

            attachedRequest = request;

            if (attachedRequest == null)
            {
                UpdateState(skinDownloader.IsInstalled(TrackedItem) ? DownloadState.LocallyAvailable : DownloadState.NotDownloaded);
                return;
            }

            attachedRequest.Failed += onRequestFailure;
            attachedRequest.DownloadProgress += onRequestProgress;
            attachedRequest.Finished += onRequestFinished;

            UpdateState(DownloadState.Downloading);
            UpdateProgress(0);
        }

        private void detachRequest()
        {
            if (attachedRequest == null)
                return;

            attachedRequest.Failed -= onRequestFailure;
            attachedRequest.DownloadProgress -= onRequestProgress;
            attachedRequest.Finished -= onRequestFinished;
            attachedRequest = null;
        }

        private void onRequestProgress(long current, long total)
        {
            if (total > 0)
                Schedule(() => UpdateProgress((double)current / total));
        }

        private void onRequestFinished() => Schedule(() => UpdateState(DownloadState.Importing));

        private void onRequestFailure(Exception _) => Schedule(() => attachDownload(null));

        protected override void Dispose(bool isDisposing)
        {
            detachRequest();

            realmSubscription?.Dispose();

            skinDownloader.DownloadBegan -= onDownloadBegan;
            skinDownloader.DownloadCompleted -= onDownloadCompleted;
            skinDownloader.DownloadFailed -= onDownloadFailed;

            base.Dispose(isDisposing);
        }
    }
}
