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
            await GetBasePatches();
            PlaySuccessLoadAnimation();
            EventDispatcher.Dispatch(EventDispatcherEvent.CompletePreload);
        }

        public void Migration() {
            CurrentLoadingStepView.Text = "Мигрируем со старой версии";
            if (Directory.Exists(Settings.WORKING_DIR + "/Launcher")) {
                Directory.Delete(Settings.WORKING_DIR + "/Launcher", true);
            }

            if (File.Exists(Settings.WORKING_DIR + "/Noblegarden Launcher.exe")) {
                File.Delete(Settings.WORKING_DIR + "/Noblegarden Launcher.exe");
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
