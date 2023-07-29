using Downloader;
using NobleLauncher.Interfaces;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace NobleLauncher.Models
{
    class SpeedWatcher
    {
        private static int StartTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        private static int SpentTime = StartTime;
        private long DownloadInSec = 0;

        public Action<long, long, long, long> onDownload;

        public void Notify(long DownloadedBytes, long ReceivedBytes, long TotalBytes) {
            DownloadInSec += DownloadedBytes;
            SpentTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            if (SpentTime - StartTime >= 1) {
                long estimation = (TotalBytes - ReceivedBytes) / DownloadInSec;
                onDownload?.Invoke(DownloadInSec, ReceivedBytes, TotalBytes, estimation);
                DownloadInSec = 0;
                StartTime = SpentTime;
            }
        }
    }
    static class FileDownloader
    {
        private static WebClient CurrentWebClient;
        private static DownloadService Downloader;
        public static async Task<long> GetFileSize(IUpdateable Patch) {
            long size = 0;
            var request = HttpWebRequest.CreateHttp(Patch.RemotePath);
            request.Method = "HEAD";
            using (var response = await request.GetResponseAsync()) {
                size = response.ContentLength;
            }

            return size;
        }

        public static void CreateFolderForDownload(string FileDestination) {
            var dirPath = FileDestination;
            var symb = dirPath[dirPath.Length - 1];
            while (symb != '/') {
                dirPath = dirPath.Substring(0, dirPath.Length - 1);
                symb = dirPath[dirPath.Length - 1];
            }

            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }
        }

        public static Task DownloadFileLib(string From, string To, Action<long, int> OnChunkLoaded) {
            if (Downloader != null) {
                throw new AccessViolationException("Downloader уже существует");
            }

            CreateFolderForDownload(To);

            if (File.Exists(To)) {
                File.Delete(To);
            }

            var opts = new DownloadConfiguration() {
                ChunkCount = 4,
                ParallelDownload = true,
                ClearPackageOnCompletionWithFailure = true,
                ReserveStorageSpaceBeforeStartingDownload = true,
            };

            SpeedWatcher sw = new SpeedWatcher();
            sw.onDownload += (downloadedBytesInSecond, receivedBytesSize, totalBytesSize, estimation) => {
                Debug.WriteLine("Download: " + From + ", speed: " + downloadedBytesInSecond / 1024 + "kb/s. Downloaded: " + (receivedBytesSize / 1024) + " out of " + (totalBytesSize / 1024) + ". Estimation: " + estimation);
            };

            long previousDownloadedSize = 0;

            void DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e) {
                var diff = e.ReceivedBytesSize - previousDownloadedSize;
                if (diff > 1024 * 4) {
                    sw.Notify(diff, e.ReceivedBytesSize, e.TotalBytesToReceive);
                    previousDownloadedSize = e.ReceivedBytesSize;
                    OnChunkLoaded(diff, (int)(e.ProgressPercentage * 100));
                }
            }

            /*
            void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
                Debug.WriteLine("Completed");
                downloader.Dispose();
                downloader = null;
            }
            */

            Downloader = new DownloadService(opts);
            Downloader.DownloadProgressChanged += DownloadProgressChanged;
            //downloader.DownloadFileCompleted += DownloadFileCompleted;
            return Downloader.DownloadFileTaskAsync(From, To);
        }

        public static Task DownloadFile(string From, string To, Action<long, int> OnChunkLoaded) {
            if (CurrentWebClient != null) {
                throw new AccessViolationException("Web client уже существует");
            }

            CreateFolderForDownload(To);

            if (File.Exists(To)) {
                File.Delete(To);
            }

            SpeedWatcher sw = new SpeedWatcher();
            sw.onDownload += (downloadedBytesInSecond, receivedBytesSize, totalBytesSize, estimation) => {
                Debug.WriteLine("Download: " + From + ", speed: " + downloadedBytesInSecond / 1024 + "kb/s. Downloaded: " + (receivedBytesSize / 1024) + " out of " + (totalBytesSize / 1024) + ". Estimation: " + estimation);
            };

            long previousDownloadedSize = 0;

            CurrentWebClient = new WebClient();

            void onProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e) {
                var diff = e.BytesReceived - previousDownloadedSize;
                sw.Notify(diff, e.BytesReceived, e.TotalBytesToReceive);
                OnChunkLoaded(diff, e.ProgressPercentage);
            }

            void onComplete(object sender, AsyncCompletedEventArgs e) {
                CurrentWebClient.Dispose();
                CurrentWebClient = null;
            }

            CurrentWebClient.Proxy = null;
            CurrentWebClient.DownloadProgressChanged += onProgressChanged;
            CurrentWebClient.DownloadFileCompleted += onComplete;
            return CurrentWebClient.DownloadFileTaskAsync(new Uri(From), To);
        }

        public static Task DownloadPatch(IUpdateable Patch, Action<long, int> onChunkLoaded) {
            return DownloadFile(Patch.RemotePath, Patch.PathToTMP, onChunkLoaded);
        }

        public static void AbortAnyLoad() {
            if (CurrentWebClient == null)
                return;

            CurrentWebClient.CancelAsync();
        }

    }
}
