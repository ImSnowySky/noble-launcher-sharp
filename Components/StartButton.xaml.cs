using NobleLauncher.Globals;
using NobleLauncher.Models;
using NobleLauncher.Structures;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace NobleLauncher.Components
{
    /// <summary>
    /// Логика взаимодействия для StartButton.xaml
    /// </summary>
    public partial class StartButton : UserControl
    {
        public StartButton() {
            InitializeComponent();
            EventDispatcher.CreateSubscription(EventDispatcherEvent.StartUpdate, () => ToggleButtonState(false));
            EventDispatcher.CreateSubscription(EventDispatcherEvent.CompleteUpdate, () => ToggleButtonState(true));

            if (!IsEXEPresented()) {
                ToggleButtonState(false);
            }
        }

        private bool IsEXEPresented() {
            string pathToEXE = Settings.WorkingDir + "/Wow.exe";
            return File.Exists(pathToEXE);
        }

        private void ToggleButtonState(bool enabled) {
            var buttonBrush = new SolidColorBrush {
                Color = enabled ? Color.FromRgb(80, 190, 60) : Color.FromRgb(170, 170, 170)
            };
            StartButtonContainerView.IsHitTestVisible = enabled;
            StartButtonBgView.Fill = buttonBrush;
        }

        private void Start(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (!IsEXEPresented())
                return;

            if (Settings.WorkingDir == ".") {
                Process.Start("Wow.exe");
            } else {
                Process.Start(Settings.WorkingDir + "/Wow.exe");
            }
        }
    }
}
