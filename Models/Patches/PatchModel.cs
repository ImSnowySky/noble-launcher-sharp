using NobleLauncher.Globals;
using NobleLauncher.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NobleLauncher.Models
{
    public class PatchModel: IUpdateable
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string LocalPath { get; set; }
        public string RemotePath { get; set; }
        public string Description { get; set; }
        public string RemoteHash { get; set; }
        public string LocalHash { get; set; }
        public bool Selected { get; set; }
        public string FullPath {
            get => Settings.WORKING_DIR + "/" + LocalPath;
        }

        public string PathToTMP {
            get => FullPath + ".tmp";
        }

        public PatchModel() {
            Selected = false;
        }

        public PatchModel(string LocalPath, string RemotePath, string RemoteHash, string Description) {
            this.LocalPath = LocalPath;
            this.RemotePath = RemotePath;
            this.RemoteHash = RemoteHash;
            this.Description = Description;
            Selected = false;

            CalcNameFromPath();
        }

        public void CalcNameFromPath() {
            if (LocalPath == null)
                return;

            var NameParts = LocalPath.Split('/');
            Name = NameParts[NameParts.Length - 1];
        }

        public virtual void ChangeSelectionTo(bool To) {
            return;
        }

        public virtual NecessaryPatchModel ToNecessaryPatch() {
            var patch = new NecessaryPatchModel(LocalPath, RemotePath, RemoteHash, Description) {
                Index = Index
            };
            return patch;
        }

        public virtual CustomPatchModel ToCustomPatchModel() {
            var patch = new CustomPatchModel(LocalPath, RemotePath, RemoteHash, Description) {
                Index = Index
            };
            return patch;
        }

        public long GetPathByteSize() {
            if (!File.Exists(FullPath))
                return 0;

            return new FileInfo(FullPath).Length;
        }

        public DateTime GetLastModified()
        {
            if (!File.Exists(FullPath)) 
                return DateTime.MinValue;
            return new FileInfo(FullPath).LastWriteTime;
        }
        public string CalcCRC32Hash(Action<long> OnBlockRead) {
            return HashCalculator.CalcCRC32Hash(this, OnBlockRead);
        }

        public Task<long> GetRemoteSize() {
            return FileDownloader.GetFileSize(this);
        }

        public Task<DateTime> GetRemoteLastModified()
        {
            return FileDownloader.GetLastModified(this);
        }

        public Task LoadUpdated(Action<long, int> OnChunkLoaded) {
            return FileDownloader.DownloadPatch(this, OnChunkLoaded);
        }
    }
}
