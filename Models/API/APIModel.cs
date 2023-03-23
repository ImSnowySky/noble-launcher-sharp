using NobleLauncher.Globals;
using NobleLauncher.Structures;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace NobleLauncher.Models
{
    public class APIModel
    {
        public string BaseURL;

        public APIModel(string BaseURL) {
            this.BaseURL = BaseURL;
        }

        protected async Task<NobleResponse> MakeAsyncRequest(string TargetURL) {
            if (BaseURL == null) {
                return new NobleResponse(false, "Не определён адрес удаленного сервера");
            }

            string errMsg = "Неизвестная ошибка";
            Debug.WriteLine($"Request to: {BaseURL}{TargetURL}");

            string responseText = await Task.Run(() => {
                try {
                    WebRequest request = WebRequest.Create($"{BaseURL}{TargetURL}");
                    WebResponse response = request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    return new StreamReader(responseStream).ReadToEnd();
                }
                catch (Exception e) {
                    if (Settings.EnableDebugMode) {
                        MessageBox.Show(e.Message);
                    }
                    errMsg = $"Error: {e.Message}";
                }

                return null;
            });

            if (responseText != null) {
                return new NobleResponse(responseText);
            }

            return new NobleResponse(false, $"Не удалось соединиться с сервером. ({errMsg})");
        }
    }
}
