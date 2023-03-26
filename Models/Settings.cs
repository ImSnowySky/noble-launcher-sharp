using System.Collections.Generic;
using System.IO;
using NobleLauncher.Models;
using NobleLauncher.Structures;

namespace NobleLauncher.Globals
{
    public class Settings
    {
        private static Settings instance;

        private readonly string AppVersionPrivate = "2.0.0";
        private readonly string WorkingDirPrivate = @"C:\SSD-Games";
        private readonly string NobleDomainPrivate = "https://noblegarden.net";
        private readonly UserSettingsFileModel UserSettingsFile;

        private bool EnableTLSPrivate = false;
        private bool EnableDebugModePrivate = false;
        private List<string> SelectedCustomPatchesPrivate = new List<string>();

        public static string AppVersion {
            get => GetInstance().AppVersionPrivate;
        }

        public static string WorkingDir {
            get => GetInstance().WorkingDirPrivate;
        }

        public static string NobleDomain {
            get => GetInstance().NobleDomainPrivate;
        }

        public static bool EnableTLS {
            get => GetInstance().EnableTLSPrivate;
        }

        public static bool EnableDebugMode {
            get => GetInstance().EnableDebugModePrivate;
        }

        public static List<string> SelectedCustomPatches {
            get => GetInstance().SelectedCustomPatchesPrivate;
        }

        private Settings() {
            var PathToSettingsFile = Path.Combine(WorkingDirPrivate, "launcher-config.ini");
            UserSettingsFile = new UserSettingsFileModel(PathToSettingsFile);
        }
        public void Parse() {
            var UserControledSettings = UserSettingsFile.GetCurrentSettings();
            EnableTLSPrivate = UserControledSettings.EnableTLS;
            EnableDebugModePrivate = UserControledSettings.EnableDebugMode;
            SelectedCustomPatchesPrivate = UserControledSettings.SelectedCustomPatches;
        }

        public static Settings GetInstance() {
            if (instance == null) {
                instance = new Settings();
            }

            return instance;
        }
        public void ToggleCustomPatchSelection(string PatchName) {
            if (SelectedCustomPatches.Contains(PatchName)) {
                SelectedCustomPatches.Remove(PatchName);
            }
            else {
                SelectedCustomPatches.Add(PatchName);
            }

            UserSettingsFile.UpdateSettings(new UserControlledSettings(
                SelectedCustomPatches,
                EnableTLS,
                EnableDebugMode
            ));
        }
    }
}
