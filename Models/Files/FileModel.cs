using System;
using System.IO;
using System.Windows;
using NobleLauncher.Globals;

namespace NobleLauncher.Models
{
    public class FileModel
    {
        protected static readonly string WORKING_DIR = Settings.WorkingDir;
        protected string PathToFile;

        public FileModel(string RelativePath) {
            PathToFile = Path.GetFullPath(
                Path.Combine(
                    WORKING_DIR,
                    RelativePath
                )
            );
        }

        protected bool Exists() {
            return File.Exists(PathToFile);
        }

        protected bool HasAnyContent() {
            return Exists() && new FileInfo(PathToFile).Length > 0;
        }
        protected void Create() {
            try {
                File.Create(PathToFile).Close();
            }
            catch (Exception e) {
                if (Settings.EnableDebugMode) {
                    MessageBox.Show(e.Message);
                }
                throw new Exception($"Не удалось создать файл {PathToFile}");
            }
        }

        protected void CreateIfNotExist() {
            if (Exists())
                return;

            Create();
        }
    }
}
