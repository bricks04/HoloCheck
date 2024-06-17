using HarmonyLib;
using System;


namespace HoloCheck.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    public class VersionPatches
    {
        private static GameNetworkManager instance;
        // Modify the version number directly so that it now contains a combination of the Password and the Version.
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        private static void StartPostFix(GameNetworkManager __instance)
        {
            instance = __instance;
            if (!HoloCheck.payloadInjection)
            {
                ChangeVersionWithPasskey(HoloCheck.passkey, __instance);
            }
        }

        [HarmonyPatch("JoinLobby")]
        [HarmonyPrefix]
        private static void JoinLobbyPrefix(GameNetworkManager __instance)
        {
            if (!HoloCheck.payloadInjection)
            {
                ChangeVersionWithPasskey(HoloCheck.passkey, __instance);
            }
        }

        public static void ChangePasskey(string passkey)
        {
            if (!HoloCheck.payloadInjection)
            {
                ChangeVersionWithPasskey(passkey, instance);
            }
        }
        private static void ChangeVersionWithPasskey(string passkey, GameNetworkManager __instance)
        {
            HoloCheck.Logger.LogInfo("VersionPatches awoken!");
            if (HoloCheck.originalVersion == 0)
            {
                HoloCheck.originalVersion = __instance.gameVersionNum;
            }
            HoloCheck.Logger.LogInfo("Original Version = " + HoloCheck.originalVersion.ToString());
            //If passkey exists, add a 1 to indicate active HoloCheck
            //Otherwise, leave the target version alone.
            if (HoloCheck.passkey != "")
            {
                HoloCheck.targetVersion = Int32.Parse(("1" + passkey + HoloCheck.originalVersion.ToString()));
            }
            else
            {
                HoloCheck.targetVersion = HoloCheck.originalVersion;
            }

            HoloCheck.Logger.LogInfo("Detected Version = " + __instance.gameVersionNum.ToString());
            HoloCheck.Logger.LogInfo("Target Version = " + HoloCheck.targetVersion.ToString());
            if (__instance.gameVersionNum != HoloCheck.targetVersion)
            {
                __instance.gameVersionNum = HoloCheck.targetVersion;
                HoloCheck.Logger.LogInfo("Detected version number does not match the intended version number, applying fix");
                HoloCheck.Logger.LogInfo("The new version is = " + __instance.gameVersionNum.ToString());
            }
            else
            {
                HoloCheck.Logger.LogWarning("Detected version matches the target version! Your lobbies will not have Passkey protection enabled! ");
            }
        }
    }
}
