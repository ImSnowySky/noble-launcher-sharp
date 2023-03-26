using NobleLauncher.Processors;
using NobleLauncher.Structures;
using System.Collections.Generic;

namespace NobleLauncher.Models
{
    class UserSettingsFileModel : DictionaryFileModel
    {
        private Dictionary<string, string> SettingsRepresentation;
        public UserSettingsFileModel(string PathToFile) : base(PathToFile, new Dictionary<string, string> {
            { "custom_patches", "" },
            { "enable_tls", "false" },
            { "debug_mode", "false" }
        }) {
            SettingsRepresentation = Read();
        }

        public UserControlledSettings GetCurrentSettings() {
            SettingsRepresentation = Read();

            return new UserControlledSettings(
                ParseSelectedCustomPatches(SettingsRepresentation),
                ParseEnableTLS(SettingsRepresentation),
                ParseEnableDebugMode(SettingsRepresentation)
            );
        }
        public void UpdateSettings(UserControlledSettings NewSettings) {
            SettingsRepresentation["custom_patches"] = "";
            SettingsRepresentation["enable_tls"] = NewSettings.EnableTLS ? "true" : "false";
            SettingsRepresentation["debug_mode"] = NewSettings.EnableDebugMode ? "true" : "false";

            for (var i = 0; i < NewSettings.SelectedCustomPatches.Count; i++) {
                if (SettingsRepresentation["custom_patches"].Length == 0) {
                    SettingsRepresentation["custom_patches"] = NewSettings.SelectedCustomPatches[i];
                }
                else {
                    SettingsRepresentation["custom_patches"] += "," + NewSettings.SelectedCustomPatches[i];
                }
            }

            Rewrite(SettingsRepresentation);
        }
        private static bool ParseEnableTLS(Dictionary<string, string> Config) {
            return StringValueConverter.IsStrictlyTrue(Config, "enable_tls");
        }

        private static bool ParseEnableDebugMode(Dictionary<string, string> Config) {
            return StringValueConverter.IsStrictlyTrue(Config, "debug_mode");
        }
        private static List<string> ParseSelectedCustomPatches(Dictionary<string, string> Config) {
            return StringValueConverter.ToList(Config, "custom_patches", ',');
        }
    }
}
