using NobleLauncher.Models;
using NobleLauncher.Structures;
using System.Windows.Controls;
using System.Windows.Input;

namespace NobleLauncher.Components
{
    /// <summary>
    /// Логика взаимодействия для UpdateButton.xaml
    /// </summary>
    public partial class UpdateButton : UserControl
    {
        public UpdateButton() {
            InitializeComponent();
            EventDispatcher.CreateSubscription(EventDispatcherEvent.CompleteUpdate, UpdateCompleted);
        }

        public void StartUpdate(object sender, MouseButtonEventArgs e) {
            EventDispatcher.Dispatch(EventDispatcherEvent.StartUpdate);
            UpdateButtonTextView.Visibility = System.Windows.Visibility.Hidden;
            UpdateButtonPreloaderView.Visibility = System.Windows.Visibility.Visible;
            UpdateButtonContainerView.IsHitTestVisible = false;
        }

        public void UpdateCompleted() {
            UpdateButtonTextView.Visibility = System.Windows.Visibility.Visible;
            UpdateButtonPreloaderView.Visibility = System.Windows.Visibility.Hidden;
            UpdateButtonContainerView.IsHitTestVisible = true;
        }
    }
}
