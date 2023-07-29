using NobleLauncher.Globals;
using NobleLauncher.Structures;
using System;
using System.Threading.Tasks;

namespace NobleLauncher.Models
{
    public class SiteAPIModel : APIModel
    {
        private static SiteAPIModel instance;
        private SiteAPIModel(string BaseURL) : base(BaseURL) { }

        public static SiteAPIModel Instance() {
            if (instance == null) {
                instance = new SiteAPIModel(Settings.NobleDomain);
            }

            return instance;
        }

        public async Task<NobleResponse> GetUpdateServerAddress() {
            var response = await MakeAsyncRequest("/site/patches-ip");
            if (!response.IsOK) {
                Static.ShutdownWithError("Не удалось получить адрес сервера обновлений");
                return new NobleResponse();
            }
            return response;
        }

        public async Task<NobleResponse> GetOnlineCount() {
            var response = await MakeAsyncRequest($"/armory/online");
            if (!response.IsOK) {
                Static.ShutdownWithError("Не удалось получить текущий онлайн");
                return new NobleResponse();
            }
            return response;
        }

        public async Task<NobleResponse> GetLastNews() {
            var response = await MakeAsyncRequest($"/site/articles");
            if (!response.IsOK) {
                Static.ShutdownWithError("Не удалось получить новости");
                return new NobleResponse();
            }
            return response;
        }

        public async Task<NobleResponse> GetActualDiscordLink() {
            var response = await MakeAsyncRequest($"/site/discord-link");
            if (!response.IsOK) {
                Static.ShutdownWithError("Не удалось получить ссылку на Discord");
                return new NobleResponse();
            }
            return response;
        }

        public async Task<NobleResponse> GetActualVKLink() {
            var response = await MakeAsyncRequest($"/site/vk-link");
            if (!response.IsOK) {
                Static.ShutdownWithError("Не удалось получить ссылку на VK");
                return new NobleResponse();
            }
            return response;
        }
    }
}
