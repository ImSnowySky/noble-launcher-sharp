using System.Collections.Generic;

namespace NobleLauncher.Structures
{
    public struct UserControlledSettings
    {
        public List<string> SelectedCustomPatches;
        public bool EnableTLS;
        public bool EnableDebugMode;
        public UserControlledSettings(List<string> SelectedCustomPatches, bool EnableTLS, bool EnableDebugMode) {
            this.SelectedCustomPatches = SelectedCustomPatches;
            this.EnableTLS = EnableTLS;
            this.EnableDebugMode = EnableDebugMode;
        }
    }
}
