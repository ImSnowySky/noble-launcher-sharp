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
        private static int startTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        private static int spentTime = startTime;
        private long downloadInSec = 0;

        public Action<long, long, long, long> onDownload;

        public void Notify(long downloadedBytes, long receivedBytes, long totalBytes) {
            downloadInSec += downloadedBytes;
            spentTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            if (spentTime - startTime >= 1) {
                long estimation = (totalBytes - receivedBytes) / downloadInSec;
                onDownload?.Invoke(downloadInSec, receivedBytes, totalBytes, estimation);
                downloadInSec = 0;
                startTime = spentTime;
            }
        }
    }
    static class FileDownloader
    {
        private static WebClient CurrentWebClient;
        private static DownloadService downloader;
        public static async Task<long> GetFileSize(IUpdateable patch) {
            long size = 0;
            var request = HttpWebRequest.CreateHttp(patch.RemotePath);
            request.Method = "HEAD";
            using (var response = await request.GetResponseAsync()) {
                size = response.ContentLength;
            }

            return size;
        }

        public static void CreateFolderForDownload(string fileDestination) {
            var dirPath = fileDestination;
            var symb = dirPath[dirPath.Length - 1];
            while (symb != '/') {
                dirPath = dirPath.Substring(0, dirPath.Length - 1);
                symb = dirPath[dirPath.Length - 1];
            }

            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }
        }

        public static Task DownloadFile(string from, string to, Action<long, int> onChunkLoaded) {
            if (downloader != null) {
                throw new AccessViolationException("Downloader уже существует");
            }

            CreateFolderForDownload(to);

            if (File.Exists(to)) {
                File.Delete(to);
            }

            var opts = new DownloadConfiguration() {
                ChunkCount = 4,
                ParallelDownload = true,
                ClearPackageOnCompletionWithFailure = true,
                ReserveStorageSpaceBeforeStartingDownload = true,
            };

            SpeedWatcher sw = new SpeedWatcher();
            sw.onDownload += (downloadedBytesInSecond, receivedBytesSize, totalBytesSize, estimation) => {
                Debug.WriteLine("Download: " + from + ", speed: " + downloadedBytesInSecond / 1024 + "kb/s. Downloaded: " + (receivedBytesSize / 1024) + " out of " + (totalBytesSize / 1024) + ". Estimation: " + estimation);
            };

            long previousDownloadedSize = 0;

            void DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e) {
                var diff = e.ReceivedBytesSize - previousDownloadedSize;
                if (diff > 1024 * 4) {
                    sw.Notify(diff, e.ReceivedBytesSize, e.TotalBytesToReceive);
                    previousDownloadedSize = e.ReceivedBytesSize;
                    onChunkLoaded(diff, (int)(e.ProgressPercentage * 100));
                }
            }

            /*
            void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
                Debug.WriteLine("Completed");
                downloader.Dispose();
                downloader = null;
            }
            */

            downloader = new DownloadService(opts);
            downloader.DownloadProgressChanged += DownloadProgressChanged;
            //downloader.DownloadFileCompleted += DownloadFileCompleted;
            return downloader.DownloadFileTaskAsync(from, to);
        }

        public static Task DownloadFileNet(string from, string to, Action<long, int> onChunkLoaded) {
            if (CurrentWebClient != null) {
                throw new AccessViolationException("Web client уже существует");
            }

            CreateFolderForDownload(to);

            if (File.Exists(to)) {
                File.Delete(to);
            }

            SpeedWatcher sw = new SpeedWatcher();
            sw.onDownload += (downloadedBytesInSecond, receivedBytesSize, totalBytesSize, estimation) => {
                Debug.WriteLine("Download: " + from + ", speed: " + downloadedBytesInSecond / 1024 + "kb/s. Downloaded: " + (receivedBytesSize / 1024) + " out of " + (totalBytesSize / 1024) + ". Estimation: " + estimation);
            };

            long previousDownloadedSize = 0;

            CurrentWebClient = new WebClient();

            void onProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e) {
                var diff = e.BytesReceived - previousDownloadedSize;
                sw.Notify(diff, e.BytesReceived, e.TotalBytesToReceive);
                previousDownloadedSize = e.BytesReceived;
                onChunkLoaded(diff, e.ProgressPercentage);
            }

            void onComplete(object sender, AsyncCompletedEventArgs e) {
                CurrentWebClient.Dispose();
                CurrentWebClient = null;
            }

            CurrentWebClient.Proxy = null;
            CurrentWebClient.DownloadProgressChanged += onProgressChanged;
            CurrentWebClient.DownloadFileCompleted += onComplete;
            return CurrentWebClient.DownloadFileTaskAsync(new Uri(from), to);
        }

        public static Task DownloadPatch(IUpdateable patch, Action<long, int> onChunkLoaded) {
            return DownloadFile(patch.RemotePath, patch.PathToTMP, onChunkLoaded);
        }

        public static void AbortAnyLoad() {
            if (CurrentWebClient == null)
                return;

            CurrentWebClient.CancelAsync();
        }

    }
}
