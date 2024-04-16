using HarmonyLib;
using System.Diagnostics;

namespace HoloCheck.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class TestNetworkingSignals
    {

        //[HarmonyPatch("Awake")]
        //[HarmonyPrefix]
        private static void AwakePrefix()
        {
            HoloCheck.Logger.LogInfo("Successfully loaded TestNetworkingSignals");
        }

        //[HarmonyPatch("PlayerLoadedServerRpc")]
        //[HarmonyPostfix]
        private static void PlayerLoadedServerRpcPostfix(ulong clientId)
        {
            HoloCheck.Logger.LogInfo("PlayerLoadedServerRpc called!");
            HoloCheck.Logger.LogInfo(clientId);
        }

        //[HarmonyPatch("PlayerLoadedClientRpc")]
        //[HarmonyPostfix]
        private static void PlayerLoadedClientRpcPostFix(ulong clientId)
        {
            HoloCheck.Logger.LogInfo("PlayerLoadedClientRpc called!");
            HoloCheck.Logger.LogInfo(clientId);
        }

        //[HarmonyPatch("OnClientConnect")]
        //[HarmonyPostfix]
        private static void OnClientConnect(ulong clientId)
        {
            HoloCheck.Logger.LogInfo("OnClientConnect called!");
            HoloCheck.Logger.LogInfo(clientId);
        }
    }
}
