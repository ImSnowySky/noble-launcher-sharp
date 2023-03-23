using NobleLauncher.Globals;
using System.Threading.Tasks;

namespace NobleLauncher.Models
{
    public class CustomPatchModel : PatchModel
    {
        public CustomPatchModel(string LocalPath, string RemotePath, string Hash, string Description) : base(LocalPath, RemotePath, Hash, Description) {
            ChangeSelectionTo(false);
            GetSelectionFromSettings();
        }

        public override void ChangeSelectionTo(bool To) {
            Selected = To;
        }

        public void GetSelectionFromSettings() {
            var selectCustomPatchesLocalPaths = Settings.SelectedCustomPatches;
            Parallel.For(0, selectCustomPatchesLocalPaths.Count, (i) => {
                if (selectCustomPatchesLocalPaths[i] == LocalPath) {
                    ChangeSelectionTo(true);
                }
            });
        }

        public override NecessaryPatchModel ToNecessaryPatch() {
            return new NecessaryPatchModel(LocalPath, RemotePath, RemoteHash, Description);
        }

        public override CustomPatchModel ToCustomPatchModel() {
            return this;
        }
    }
}