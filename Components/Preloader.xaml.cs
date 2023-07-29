using NobleLauncher.Globals;
using NobleLauncher.Models;
using NobleLauncher.Structures;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace NobleLauncher.Components
{
    /// <summary>
    /// Логика взаимодействия для Preloader.xaml
    /// </summary>
    public partial class Preloader : UserControl
    {
        private UpdateServerAPIModel UpdateServerAPI;

        public Preloader() {
            InitializeComponent();
            EventDispatcher.CreateSubscription(EventDispatcherEvent.StartPreload, StartPreload);
        }

        private async void StartPreload() {
            UpdateServerAPI = UpdateServerAPIModel.Instance();
            ModalBackgroundView.Opacity = 1;

            Migration();
            await CheckLauncherVersion();
            await GetBasePatches();
            PlaySuccessLoadAnimation();
        }

        public void Migration() {
            CurrentLoadingStepView.Text = "Мигрируем со старой версии";
            if (Directory.Exists(Settings.WorkingDir + "/Launcher")) {
                Directory.Delete(Settings.WorkingDir + "/Launcher", true);
            }

            if (File.Exists(Settings.WorkingDir + "/Noblegarden Launcher.exe")) {
                File.Delete(Settings.WorkingDir + "/Noblegarden Launcher.exe");
            }
        }

        private async Task UpdateLauncher(string linkToLauncher) {
            CurrentLoadingStepView.Text = "Скачиваем актуальную версию лаунчера";
            CurrentLoadingProgressView.Opacity = 1;
            await FileDownloader.DownloadFile(
                linkToLauncher,
                Settings.WorkingDir + "/NobleLauncher.exe.tmp",
                (chunkSize, percentage) => {
                    CurrentLoadingProgressView.Value = percentage;
                }
            );

            using (var sw = File.CreateText("focus.vbs")) {
                sw.WriteLine("Dim WshShell");
                sw.WriteLine("Set WshShell = WScript.CreateObject(\"WScript.Shell\")");
                sw.WriteLine("Dim ARGS");
                sw.WriteLine("set ARGS = WScript.Arguments");
                sw.WriteLine("WshShell.AppActivate(ARGS.Item(0))");
                sw.WriteLine("WshShell.SendKeys(\"~\")");
                sw.WriteLine("WScript.Quit(0)");
            }

            var process = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            var cmds = new string[]{
                "/C timeout 2",
                "del NobleLauncher.exe",
                "rename NobleLauncher.exe.tmp NobleLauncher.exe",
                "start NobleLauncher.exe",
                "timeout 1",
                "focus.vbs \"Лаунчер Noblegarden\"",
                "del focus.vbs"
            };
            startInfo.Arguments = String.Join("&", cmds);
            process.StartInfo = startInfo;
            process.Start();

            Static.Shutdown();
        }

        public async Task CheckLauncherVersion() {
            if (UpdateServerAPI == null)
                return;
            CurrentLoadingStepView.Text = "Сверяем версии лаунчеров";
            var launcherVersionResponse = await UpdateServerAPI.GetActualLauncherVersion();
            string actualLauncherVersion = (string)launcherVersionResponse.FormattedData.version;
            if (actualLauncherVersion == "") {
                Static.ShutdownWithError("Сервер не вернул актуальной версии лаунчера");
            }
            if (actualLauncherVersion != Settings.AppVersion) {
                await UpdateLauncher((string)launcherVersionResponse.FormattedData.link);
            }
        }
        private async Task GetBasePatches() {
            CurrentLoadingStepView.Text = "Получаем список патчей";
            var defaultPatchesResponse = await UpdateServerAPI.GetBasePatches();
            var patchesInfo = defaultPatchesResponse.FormattedData;
            var defaultPatches = JObjectConverter.ConvertToNecessaryPatchesList(patchesInfo);
            Static.Patches = new NoblePatchGroupModel<NecessaryPatchModel>(defaultPatches);
        }
        private void PlaySuccessLoadAnimation() {
            CurrentLoadingStepView.Text = "";
            var fadeOutAnim = (Storyboard)FindResource("FadeOutModalBG");
            if (fadeOutAnim != null) {
                fadeOutAnim.Begin();
            }
        }
    }
}
