using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Linq;

namespace HoloCheck
{
    class HoloCheckConfig
    {
        public readonly ConfigEntry<string> configAllowedSteamIDs;
        public readonly ConfigEntry<string> configPasskey;

        public HoloCheckConfig(ConfigFile cfg)
        {
            cfg.Bind("General",
                        "READ ME",
                        "PLEASE READ ME",
                        "Before playing, please obtain the steam IDs of everyone you wish to allow into the server. A steam ID check will be performed regardless of if you use a private or public server, please disable the mod if you want to allow users freely. This mod will not work if you use LAN mode.");
            cfg.Bind("General",
                        "Permission Statement and Notice",
                        "",
                        "Where it is required, I, the creator of the HoloCheck mod, grant express permission to those that wish to use this mod to create Lethal Company content, or to assist with the creation of said content. The code is provided AS IS, and no express warranty or guarantee of performance or functionality is provided. By installing and running this mod, you agree that you understand the risks of utilising mods, including the risk of save file/game damage. "
                        );
            cfg.Bind("General",
                        "Contact",
                        "andrew3199 or Xitter@bricks041lol, email bricks041@gmail.com",
                        "Please contact me if you find any issues with the mod, or if you have any questions. (Please do not spam any of these contact channels as I will block people if needed.)");

            configAllowedSteamIDs = cfg.Bind("General",
                                                "Allowed Steam IDs",
                                                "",
                                                "A comma-separated string, containing a list of Steam IDs that you wish to allow entry into your servers. Ensure that there is no whitespace in the string. Example - '123456789,987654321,011131017'. YOUR STEAM ID IS NOT THE SAME THING AS THE STEAM FRIEND CODE! Obtain steam IDs by going to your profile, and taking the numbers at the end of the URL. ");

            //configPasskey = cfg.Bind("General",
            //                                    "Passkey Checking",
            //                                    "",
            //                                    "A collection of numbers that must match people joining your lobby before they will be allowed to enter. Leave blank to disable.");
        }
    }
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class HoloCheck : BaseUnityPlugin
    {
        internal static HoloCheckConfig BoundConfig { get; private set; } = null!;
        public static HoloCheck Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        internal new static string[] allowedSteamIDs;
        internal static int originalVersion = 0;
        internal static int targetVersion = 999950;

        public static string passkey = "";

        public static bool displaySettings = false;

        public static void SaveConfig()
        {
            //BoundConfig.configPasskey.BoxedValue = passkey;
            string saveResult = "";
            foreach (var item in allowedSteamIDs)
            {
                saveResult = saveResult + "," + item;
            }
            Logger.LogInfo(saveResult);
            BoundConfig.configAllowedSteamIDs.BoxedValue = saveResult;
        }

        private void Awake()
        {
            BoundConfig = new HoloCheckConfig(base.Config);
            string @string = (string)BoundConfig.configAllowedSteamIDs.BoxedValue;
            allowedSteamIDs = @string.Split(",");
            if (allowedSteamIDs[0] == "")
            {
                allowedSteamIDs = [];
            }

            passkey = "";

            Logger = base.Logger;
            Instance = this;

            Patch();
            Logger.LogInfo($"Passkey = {passkey}, Steam IDs = {allowedSteamIDs.Length}");
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogInfo("Patching...");

            Harmony.PatchAll();

            Logger.LogInfo("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }
    }
}
