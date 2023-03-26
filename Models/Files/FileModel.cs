using System;
using System.IO;
using System.Windows;
using NobleLauncher.Globals;

namespace NobleLauncher.Models
{
    public class FileModel
    {
        protected string PathToFile;

        public FileModel(string PathToFile) {
            this.PathToFile = Path.GetFullPath(PathToFile);
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
