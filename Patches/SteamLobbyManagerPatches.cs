using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace HoloCheck.Patches
{
    [HarmonyPatch(typeof(SteamLobbyManager))]
    public class SteamLobbyManagerPatches
    {
        private static SteamLobbyManager lobbyManager;

        [HarmonyPatch("OnEnable")]
        [HarmonyPostfix]
        private static void OnEnablePostfix(SteamLobbyManager __instance)
        {
            HoloCheck.Logger.LogInfo("OnEnable postfix patch run!");
            lobbyManager = __instance;
        }

        public static void CallRefreshServerListButton()
        {
            if (lobbyManager == null)
            {
                HoloCheck.Logger.LogError("There is no stored reference to the game's SteamLobbyManager!");
            }
            else
            {
                HoloCheck.Logger.LogInfo("Forcing a server list refresh!");
                lobbyManager.RefreshServerListButton();
            }
        }

        [HarmonyPatch("LoadServerList")]
        [HarmonyPostfix]
        private static void LoadServerListPostfix()
        {
            if (HoloCheck.passkey == "")
            {
                HoloCheck.Logger.LogWarning("Accessing the server list with no passkey set!");
                UIPatches.DisplayWarningMessage("> No Passkey Set!", 10.0f);
            }
        }
    }
}
