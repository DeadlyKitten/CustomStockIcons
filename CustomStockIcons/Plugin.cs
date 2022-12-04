using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CustomStockIcons.Managers;
using HarmonyLib;
//using MenuCore;
//using System.Collections.Generic;

namespace CustomStockIcons
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance;
        internal static ConfigEntry<bool> useVanillaIcons;
        internal static ConfigEntry<bool> useSeriesEmblems;

        void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(this);
                return;
            }
            Instance = this;

            useVanillaIcons = Config.Bind<bool>("Options", "Use Vanilla Icons", false, "Use the default heart icons instead of customized defaults.\nThis only applies if an icon is not found for a character.");
            useSeriesEmblems = Config.Bind<bool>("Options", "Use Series Emblems", true, "Replace the gradient behind the damage percentage with a custom series emblem.");

            IconManager.Init();
            BackgroundManager.Init();

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            //CustomOptionsMenuHandler.InitiateMenu(CreateMenu);
        }

        //private void CreateMenu()
        //{
        //    var menu = new CustomOptionsMenu("Custom Stock Icons");
        //    menu.CreateSelector("defaultEnabled", "Use Vanilla Icons", new List<string> { "No", "Yes" }, useVanillaIcons.Value ? 1 : 0, SelectorType.RightSide, OnValueChanged);
        //    menu.CreateText("text", "Use the default hearts");
        //    menu.CreateText("text", "instead of included icons?");

        //    CustomOptionsMenuHandler.CreateMenuTab(menu);
        //}

        //private void OnValueChanged(string _, int value)
        //{
        //    useVanillaIcons.Value = value > 0;
        //    Config.Save();
        //    IconManager.Refresh();
        //}

        #region logging
        internal static void LogDebug(string message) => Instance.Log(message, LogLevel.Debug);
        internal static void LogInfo(string message) => Instance.Log(message, LogLevel.Info);
        internal static void LogWarning(string message) => Instance.Log(message, LogLevel.Warning);
        internal static void LogError(string message) => Instance.Log(message, LogLevel.Error);
        private void Log(string message, LogLevel logLevel) => Logger.Log(logLevel, message);
        #endregion
    }
}
