using System;
using System.Collections.Generic;
using NobleLauncher.Models;

namespace NobleLauncher.Globals
{
    public static class Settings {
        public static readonly string WORKING_DIR = @".";
        public static readonly string NOBLE_DOMAIN = "https://noblegarden.net";
        public static readonly string LAUNCHER_VERSION = "2.0.3";
        public static bool ENABLE_TLS = false;
        public static bool ENABLE_DEBUG_MODE = false;

        private static List<string> SELECTED_CUSTOM_PATCHES = new List<string>();

        private static readonly FileWithDefaultDictionaryContentModel SettingsFile = new FileWithDefaultDictionaryContentModel(
            "launcher-config.ini",
            new Dictionary<string, string> {
                { "custom_patches", "" },
                { "enable_tls", "false" },
                { "debug_mode", "false" }
            }
        );

        private static Dictionary<string, string> SettingsRepresentation = new Dictionary<string, string>();

        public static void Parse() {
            var config = SettingsFile.ReadAsDictionary();
            SettingsRepresentation = config;

            ParseSelectedCustomPatches(SettingsRepresentation);
            ParseEnableTLS(SettingsRepresentation);
            ParseEnableDebugMode(SettingsRepresentation);
        }

        public static List<string> GetSelectedCustomPatches() {
            return SELECTED_CUSTOM_PATCHES;
        }

        public static void ToggleCustomPatchSelection(string name) {
            if (SELECTED_CUSTOM_PATCHES.Contains(name)) {
                SELECTED_CUSTOM_PATCHES.Remove(name);
            }
            else {
                SELECTED_CUSTOM_PATCHES.Add(name);
            }

            SettingsRepresentation["custom_patches"] = "";

            for (var i = 0; i < SELECTED_CUSTOM_PATCHES.Count; i++) {
                if (SettingsRepresentation["custom_patches"].Length == 0) {
                    SettingsRepresentation["custom_patches"] = SELECTED_CUSTOM_PATCHES[i];
                }
                else {
                    SettingsRepresentation["custom_patches"] += "," + SELECTED_CUSTOM_PATCHES[i];
                }
            }

            SettingsFile.WriteDictionary(SettingsRepresentation);
        }
        private static void ParseSelectedCustomPatches(Dictionary<string, string> config) {
            if (!config.ContainsKey("custom_patches")) {
                config.Add("custom_patches", "");
                SettingsFile.WriteDictionary(config);
                return;
            }

            var patches = config["custom_patches"];

            if (patches == null || patches.Length == 0) {
                SELECTED_CUSTOM_PATCHES = new List<string>();
                return;
            }

            var splittedPatches = patches.Split(',');
            SELECTED_CUSTOM_PATCHES = new List<string>(splittedPatches);
        }

        private static void ParseEnableTLS(Dictionary<string, string> config) {
            ENABLE_TLS = ParseAndReturnBoolVal(config, "enable_tls");
            return;
        }

        private static void ParseEnableDebugMode(Dictionary<string, string> config) {
            ENABLE_DEBUG_MODE = ParseAndReturnBoolVal(config, "debug_mode");
            return;
        }

        private static bool ParseAndReturnBoolVal(Dictionary<string, string> config, string key) {
            if (!config.ContainsKey(key)) {
                config.Add(key, "false");
                SettingsFile.WriteDictionary(config);
                return false;
            }

            return config[key] == "true";
        }
    }
}
