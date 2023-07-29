using NobleLauncher.Globals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace NobleLauncher.Models
{
    public class DictionaryFileModel : FileModel
    {
        public Dictionary<string, string> DefaultContent;
        public DictionaryFileModel(string PathToFile, Dictionary<string, string> DefaultContent) : base(PathToFile) {
            this.DefaultContent = DefaultContent;
            CreateIfNotExist();
            FillWithDefaults();
        }

        protected void FillWithDefaults() {
            if (DefaultContent.Count == 0 || !Exists() || HasAnyContent())
                return;

            try {
                using (Stream file = new FileStream(PathToFile, FileMode.Open, FileAccess.Write, FileShare.Read)) {
                    using (StreamWriter writer = new StreamWriter(file)) {
                        foreach (KeyValuePair<string, string> entry in DefaultContent) {
                            writer.WriteLine($"{entry.Key}={entry.Value}");
                        }
                    }
                }
            }
            catch (Exception e) {
                if (Settings.EnableDebugMode) {
                    MessageBox.Show(e.Message);
                }
                throw new Exception($"Не удалось заполнить файл {PathToFile} стандартными значениями");
            }
        }

        public Dictionary<string, string> Read() {
            var content = new Dictionary<string, string>();
            if (!Exists() || !HasAnyContent())
                return content;

            try {
                using (Stream file = new FileStream(PathToFile, FileMode.Open, FileAccess.Read, FileShare.None)) {
                    using (StreamReader reader = new StreamReader(file, false)) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            var lineParts = line.Split('=');
                            if (lineParts.Length < 2)
                                throw new Exception();

                            content.Add(lineParts[0], lineParts[1]);
                        }
                    }
                }
            }
            catch(Exception E) {
                if (Settings.EnableDebugMode) {
                    MessageBox.Show(E.Message);
                }
                throw new Exception($"Не удалось прочитать файл {PathToFile} как экземпляр Dictionary");
            }

            return content;
        }

        public void Rewrite(Dictionary<string, string> Dict) {
            if (!Exists()) return;

            File.WriteAllText(PathToFile, "");

            try {
                using (Stream file = new FileStream(PathToFile, FileMode.Open, FileAccess.Write, FileShare.None)) {
                    using (StreamWriter writer = new StreamWriter(file)) {
                        foreach (KeyValuePair<string, string> pair in Dict) {
                            Debug.WriteLine(pair);
                            writer.WriteLine($"{pair.Key}={pair.Value}");
                        }
                    }
                }
            }
            catch (Exception E) {
                if (Settings.EnableDebugMode) {
                    MessageBox.Show(E.Message);
                }
                throw new Exception($"Не удалось записать содержимое в файл {PathToFile}");
            }
        }
    }
}
