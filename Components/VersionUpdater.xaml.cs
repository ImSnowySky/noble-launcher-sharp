using NobleLauncher.Globals;
using NobleLauncher.Models;
using NobleLauncher.Structures;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

namespace NobleLauncher.Components
{
    /// <summary>
    /// Interaction logic for VersionUpdater.xaml
    /// </summary>
    public partial class VersionUpdater : UserControl
    {
        private UpdateServerAPIModel UpdateServerAPI;
        private string launcherLink;
        public VersionUpdater()
        {
            InitializeComponent();
            UpdateServerAPI = UpdateServerAPIModel.Instance();
            MarkDeprecated(false);
            EventDispatcher.CreateSubscription(EventDispatcherEvent.StartPreload, CheckLauncherVersion);
        }

        public void MarkDeprecated(bool deprecated)
        {
            Static.InUIThread(() => {
                if (deprecated)
                {
                    VersionTextBlock.Text = VersionTextBlock.Text + " - версия не совпадает с версией на сервере.";
                }
                ManualDownloadTextBlock.Visibility = deprecated ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                OrTextBlock.Visibility = deprecated ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                AutoUpdateTextBlock.Visibility = deprecated ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            });
        }

        public async void CheckLauncherVersion()
        {
            if (UpdateServerAPI == null)
                return;
            var launcherVersionResponse = await UpdateServerAPI.GetActualLauncherVersion();
            string actualLauncherVersion = (string)launcherVersionResponse.FormattedData.version;
            if (actualLauncherVersion == "")
            {
                Static.ShutdownWithError("Сервер не вернул актуальной версии лаунчера");
            }
            if (actualLauncherVersion != Settings.LAUNCHER_VERSION)
            {
                launcherLink = (string)launcherVersionResponse.FormattedData.link;
                MarkDeprecated(true);
            }
        }

        private void DownloadLauncher(object sender, MouseButtonEventArgs e)
        {
            ManualDownloadTextBlock.Tag = launcherLink;
            Static.OpenLinkFromTag(sender, e);
        }

        private async void AutoUpdateLauncher(object sender, MouseButtonEventArgs e)
        {

            await FileDownloader.DownloadFile(
                launcherLink,
                Settings.WORKING_DIR + "/NobleLauncher.exe.tmp",
                (chunkSize, percentage) => {}
            );

            using (var sw = File.CreateText("focus.vbs"))
            {
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
    }
}
