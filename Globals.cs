﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using NoblegardenLauncherSharp.Controllers;
using NoblegardenLauncherSharp.Models;
using NoblegardenLauncherSharp.Structures;

namespace NoblegardenLauncherSharp
{
    public static class Globals
    {
        public static readonly string WORKING_DIR = @"D:\";
        public static readonly string NOBLE_DOMAIN = "https://noblegarden.net";
        public static readonly string LAUNCHER_VERSION = "1.3.2";

        public static NoblePatchGroupModel<NecessaryPatchModel> Patches;
        public static NoblePatchGroupModel<CustomPatchModel> CustomPatches;

        public static readonly List<SliderElement> SliderElements = new List<SliderElement> {
            new SliderElement("Персонажи", "https://noblegarden.net/charlist", "Images/square-character.jpg"),
            new SliderElement("Гильдии", "https://noblegarden.net/guild", "Images/square-guild.jpg"),
            new SliderElement("Сюжеты", "https://noblegarden.net/storyline", "Images/square-plots.jpg")
        };

        public enum LOADING_STEPS {
            GET_SERVER_ADDRESS = 0,
            CHECK_LAUNCHER_VERSION = 1,
            GET_LAST_UPDATES = 2,
            GET_PATCHES_INFO = 3,
        }

        public static readonly string[] LOADING_TEXTS = {
            "Получаем адрес сервера обновлений",
            "Сверяем версии лаунчеров",
            "Получаем последние новости",
            "Получаем информацию о патчах",
        };

        public static string GetRightWordForm(int count, string[] words) {
            if (count <= 0)
                return words[2];

            if (count < 20) {
                if (count == 1)
                    return words[0];
                if (count < 5)
                    return words[1];
                return words[2];
            }

            if (count < 100) {
                int lastNumberInCount = count % 10;
                return GetRightWordForm(lastNumberInCount, words);
            }

            int lastTenInCount = count % 100;
            return GetRightWordForm(lastTenInCount, words);
        }

        public static void OpenLinkFromTag(object sender, RoutedEventArgs e) {
            var target = (FrameworkElement)sender;
            string link = target.Tag.ToString();
            Process.Start(link);
        }
    }
}
