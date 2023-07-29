using System.Windows;
using NobleLauncher.Globals;
using NobleLauncher.Models;
using NobleLauncher.Structures;
using System.Net;
using System.Net.Security;

namespace NobleLauncher {
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Settings.GetInstance().Parse();
            ToggleTLS();
            InitializeComponent();
        }

        private void ToggleTLS() {
            if (Settings.EnableTLS)
                return;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, cert, chain, policy) => true);
        }

        private void OnWindowLoad(object sender, RoutedEventArgs e) {
            EventDispatcher.Dispatch(EventDispatcherEvent.StartPreload);
        }
    }
}
