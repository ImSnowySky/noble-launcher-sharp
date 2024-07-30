using NobleLauncher.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace NobleLauncher.Models
{
    static class FileDownloader
    {
        private static WebClient CurrentWebClient;

        public static async Task<long> GetFileSize(IUpdateable patch) {
            long size = 0;
            var request = HttpWebRequest.CreateHttp(patch.RemotePath);
            request.Method = "HEAD";
            using (var response = await request.GetResponseAsync()) {
                size = response.ContentLength;
            }
            return size;
        }
        
        public static async Task<DateTime> GetLastModified(IUpdateable patch)
        {
            DateTime lastModified;
            var request = HttpWebRequest.CreateHttp(patch.RemotePath);
            request.Method = "HEAD";
            using (var response = await request.GetResponseAsync())
            {
                string lastModifiedString = response.Headers[HttpResponseHeader.LastModified];
                lastModified = DateTime.Parse(lastModifiedString);
            }

            return lastModified;
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
            if (CurrentWebClient != null) {
                throw new AccessViolationException("Web client уже существует");
            }

            CreateFolderForDownload(to);

            if (File.Exists(to)) {
                File.Delete(to);
            }

            long previousDownloadedSize = 0;
            CurrentWebClient = new WebClient();

            void onProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
                var diff = e.BytesReceived - previousDownloadedSize;
                previousDownloadedSize = e.BytesReceived;
                onChunkLoaded(diff, e.ProgressPercentage);
            }

            void onComplete(object sender, AsyncCompletedEventArgs e) {
                CurrentWebClient = null;
            }

            CurrentWebClient.Proxy = WebRequest.DefaultWebProxy;
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
