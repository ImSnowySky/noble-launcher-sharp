using NobleLauncher.Globals;
using NobleLauncher.Interfaces;
using NobleLauncher.Models;
using NobleLauncher.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NobleLauncher.Components
{
    /// <summary>
    /// Логика взаимодействия для Updater.xaml
    /// </summary>
    public partial class Updater : UserControl
    {
        public Updater()
        {
            InitializeComponent();
            EventDispatcher.CreateSubscription(EventDispatcherEvent.CompletePreload, FastCheckUpdateNeeded);
            EventDispatcher.CreateSubscription(EventDispatcherEvent.StartUpdate, Update);
        }

        private List<IUpdateable> GetPatches()
        {
            List<IUpdateable> necessaryPatches = Static.Patches.List.ToList<IUpdateable>();
            List<IUpdateable> selectedCustomPatches = Static.CustomPatches.List.FindAll(patch => patch.Selected).ToList<IUpdateable>();

            List<IUpdateable> patches = new List<IUpdateable>();

            necessaryPatches.ForEach(patch => patches.Add(patch));
            selectedCustomPatches.ForEach(patch => patches.Add(patch));
            return patches;
        }

        private async void FastCheckUpdateNeeded()
        {
            var patches = GetPatches();
            await Task.Run(async () => {
                foreach (var patch in patches)
                {
                    DateTime lastModified = await patch.GetRemoteLastModified();
                    if (lastModified > patch.GetLastModified())
                    {
                        Static.InUIThread(() => {
                            ActionTextView.Text = "Некоторые патчи устарели, пришла пора обновиться.";
                        });
                        return;
                    }
                }
                Static.InUIThread(() => {
                    ActionTextView.Text = "Все патчи актуальны! Приятной игры!";
                });
            });
        }

        // https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
        protected virtual bool CanWriteToFile(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }
            FileInfo file = new FileInfo(path);
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return false;
            }
            return true;
        }

        private async Task UpdateLastModifiedTime(List<IUpdateable> patches)
        {
            foreach (IUpdateable patch in patches)
            {
                if (CanWriteToFile(patch.LocalPath))
                {
                    DateTime lastModified = await patch.GetRemoteLastModified();
                    File.SetLastWriteTime(patch.LocalPath, lastModified);
                }
            }
        }

        public async void Update()
        {
            var patches = GetPatches();
            await CalcHashes(patches);
            await UpdateLastModifiedTime(patches);
            List<IUpdateable> patchesToUpdate = patches.FindAll(patch => patch.LocalHash != patch.RemoteHash);

            await DownloadFiles(patchesToUpdate, await GetSummaryDownloadSize(patchesToUpdate));
            CompleteUpdate();
        }

        private long GetSummaryHashFileSize(List<IUpdateable> patches)
        {
            long summaryFileSize = 0;
            for (int i = 0; i < patches.Count; i++)
            {
                summaryFileSize += patches[i].GetPathByteSize();
            };

            return summaryFileSize;
        }
        private void CalcHash(IUpdateable patch)
        {
            patch.LocalHash = patch.CalcCRC32Hash((blockSize) => {});
        }

        private void UpdateProgressBar(int done, int total)
        {
            double progress = (double)(done) / (double)(total);
            int progressPercentage = (int)(progress * 100);
            Static.InUIThread(() => {
                ActionTextView.Text = "Считаем хэш-суммы... [" + done + "/" + total + "].";
            });
            SetProgress(progressPercentage);
        }

        private int hashesCounted = 0;

        private Task CalcHashes(List<IUpdateable> patches)
        {
            hashesCounted = 0;
            if (patches.Count == 0) return Task.Run(() => { });
            long summarySize = GetSummaryHashFileSize(patches);
            UpdateProgressBar(0, patches.Count);
            Task[] tasks = new Task[patches.Count];
            for (int i = 0; i < patches.Count; i++)
            {
                var patch = patches[i];
                tasks[i] = Task.Run(() => {
                    CalcHash(patch);
                    int newCount = Interlocked.Increment(ref hashesCounted);
                    UpdateProgressBar(newCount, patches.Count);
                });
            }

            return Task.Run(() => { Task.WhenAll(tasks).Wait(); });
        }

        private async Task<long> GetSummaryDownloadSize(List<IUpdateable> patches)
        {
            long summarySize = 0;
            if (patches.Count == 0)
            {
                return 0;
            }

            Static.InUIThread(() => {
                ActionTextView.Text = "Считаем размер обновления";
                SetProgress(0);
            });

            await Task.Run(async () => {
                for (int i = 0; i < patches.Count; i++)
                {
                    var patch = patches[i];
                    Static.InUIThread(() => {
                        ActionTextView.Text = "Считаем размер обновления: " + patch.LocalPath;
                    });

                    var size = await patch.GetRemoteSize();
                    summarySize += size;
                    double fileProgress = (double)(i + 1) / (double)(patches.Count);
                    int progress = (int)(fileProgress * 100);
                    SetProgress(progress);
                }
            });

            return summarySize;
        }

        private Task DownloadFiles(List<IUpdateable> patches, long summarySize)
        {
            if (patches.Count == 0)
                return Task.Run(() => { });

            long currentLoaded = 0;

            Static.InUIThread(() => {
                ActionTextView.Text = "Загружаем файлы";
                SetProgress(0);
            });

            return Task.Run(async () => {
                for (int i = 0; i < patches.Count; i++)
                {
                    var patch = patches[i];
                    Static.InUIThread(() => {
                        ActionTextView.Text = "Загружаем файл: " + patch.LocalPath + "(" + (i + 1) + "/" + patches.Count + ")";
                    });

                    await patch.LoadUpdated((loadedChunkSize, percentOfFile) => {
                        currentLoaded += loadedChunkSize;
                        double downloadProgress = (double)(currentLoaded) / (double)(summarySize);
                        int progress = (int)(downloadProgress * 100);
                        SetProgress(progress);
                    });

                    if (!File.Exists(patch.PathToTMP))
                        return;

                    if (File.Exists(patch.FullPath))
                    {
                        File.Delete(patch.FullPath);
                    }

                    File.Move(patch.PathToTMP, patch.FullPath);
                }
            });
        }

        private void CompleteUpdate()
        {
            Static.InUIThread(() => {
                ActionTextView.Text = "Обновлено!";
                ProgressTextView.Text = "";
                ProgressView.Value = 100;
            });

            EventDispatcher.Dispatch(EventDispatcherEvent.CompleteUpdate);
        }

        private void SetProgress(int progress)
        {
            Static.InUIThread(() => {
                if (progress != ProgressView.Value)
                {
                    ProgressView.Value = progress;
                    ProgressTextView.Text = progress.ToString() + "%";
                }
            });
        }
    }
}
