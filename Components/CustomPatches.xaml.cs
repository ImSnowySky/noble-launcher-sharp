using NobleLauncher.Globals;
using NobleLauncher.Models;
using NobleLauncher.Structures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NobleLauncher.Components
{
    public partial class CustomPatches : UserControl
    {
        private readonly UpdateServerAPIModel UpdateServerAPI = UpdateServerAPIModel.Instance();
        public CustomPatches() {
            InitializeComponent();
            Task.Run(() => GetAndDrawCustomPatches());
            Task.Run(() => GetAndDrawBasePatches());
        }

        private void OpenPatchLink(object sender, MouseButtonEventArgs e) {
            Static.OpenLinkFromTag(sender, e);
        }

        private void OnCustomPatchSelectorClick(object sender, RoutedEventArgs e) {
            var target = (FrameworkElement)sender;
            var id = Int32.Parse(target.Tag.ToString());
            var patch = Static.CustomPatches.GetPatchByID(id);
            patch.ChangeSelectionTo(!patch.Selected);
            Settings.ToggleCustomPatchSelection(patch.LocalPath);
            CustomPatchesView.Items.Refresh();
            EventDispatcher.Dispatch(EventDispatcherEvent.SettingsRefresh);
        }

        private async Task GetAndDrawCustomPatches() {
            var customPatchesResponse = await UpdateServerAPI.GetCustomPatches();
            var patchesInfo = customPatchesResponse.FormattedData;
            List<CustomPatchModel> customPatches = JObjectConverter.ConvertToCustomPatchesList(patchesInfo);
            Static.CustomPatches = new NoblePatchGroupModel<CustomPatchModel>(customPatches);

            Static.InUIThread(() => {
                CustomPatchesView.ItemsSource = Static.CustomPatches.List;
            });
        }

        private async Task GetAndDrawBasePatches()
        {
            var basePatchesResponse = await UpdateServerAPI.GetBasePatches();
            var patchesInfo = basePatchesResponse.FormattedData;
            List<NecessaryPatchModel> basePatches = JObjectConverter.ConvertToNecessaryPatchesList(patchesInfo);
            Static.Patches = new NoblePatchGroupModel<NecessaryPatchModel>(basePatches);
            Static.InUIThread(() => {
                NecessaryPatchesView.ItemsSource = Static.Patches.List;
            });
        }
    }
}
