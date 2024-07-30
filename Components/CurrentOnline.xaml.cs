using System;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using NobleLauncher.Models;
using NobleLauncher.Globals;

namespace NobleLauncher.Components
{
    /// <summary>
    /// Логика взаимодействия для CurrentOnline.xaml
    /// </summary>
    public partial class CurrentOnline : UserControl
    {
        private readonly SiteAPIModel SiteAPI = SiteAPIModel.Instance();
        public CurrentOnline() {
            InitializeComponent();
            Task.Run(() => DrawCurrentOnline());
        }
        private void OpenCurrentOnlineLink(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Static.OpenLinkFromTag(sender, e);
        }

        private async Task DrawCurrentOnline() {
            while (true)
            {
                var onlineResponse = await SiteAPI.GetOnlineCount();
                var wordForms = new string[] { "игрок", "игрока", "игроков" };
                string text = "Не найдено";
                if (onlineResponse.IsOK)
                {
                    try
                    {
                        int playersCount = Int32.Parse(onlineResponse.FormattedData);
                        string wordForm = Static.GetRightWordForm(playersCount, wordForms);
                        text = $"{playersCount} {wordForm}";
                    }
                    catch
                    {
                        text = onlineResponse.FormattedData;
                    }
                }

                Static.InUIThread(() =>
                {
                    CurrentOnlineText.Text = text;
                });

                await Task.Delay(60000/*milliseconds*/);
            }
        }
    }
}
