using NobleLauncher.Models;
using NobleLauncher.Structures;
using System.Collections.Generic;
using System.Diagnostics;

namespace NoblegardenLauncherSharp.Processors
{
    static class UserControlledSettingsFileProcessor
    {
        private static readonly DictionaryFileModel SettingsFile = new DictionaryFileModel(
            "launcher-config.ini",
            new Dictionary<string, string> {
                { "custom_patches", "" },
                { "enable_tls", "false" },
                { "debug_mode", "false" }
            }
        );

        private static Dictionary<string, string> SettingsRepresentation = new Dictionary<string, string>();

        public static UserControlledSettings Parse() {
            SettingsRepresentation = SettingsFile.Read();
            return new UserControlledSettings(
                ParseSelectedCustomPatches(SettingsRepresentation),
                ParseEnableTLS(SettingsRepresentation),
                ParseEnableDebugMode(SettingsRepresentation)
            );
        }
        public static void UpdateSettings(UserControlledSettings NewSettings) {
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
            SettingsFile.Rewrite(SettingsRepresentation);
        }

        private static List<string> ParseSelectedCustomPatches(Dictionary<string, string> config) {
            if (!config.ContainsKey("custom_patches")) {
                config.Add("custom_patches", "");
                SettingsFile.Rewrite(config);
                return new List<string>();
            }

            var patches = config["custom_patches"];

            if (patches == null || patches.Length == 0) {
                return new List<string>();
            }

            var splittedPatches = patches.Split(',');
            return new List<string>(splittedPatches);
        }
        private static bool ParseEnableTLS(Dictionary<string, string> config) {
            return ParseAndReturnBoolVal(config, "enable_tls");
        }

        private static bool ParseEnableDebugMode(Dictionary<string, string> config) {
            return ParseAndReturnBoolVal(config, "debug_mode");
        }

        private static bool ParseAndReturnBoolVal(Dictionary<string, string> config, string key) {
            if (!config.ContainsKey(key)) {
                config.Add(key, "false");
                SettingsFile.Rewrite(config);
                return false;
            }

            return config[key] == "true";
        }
    }
}
