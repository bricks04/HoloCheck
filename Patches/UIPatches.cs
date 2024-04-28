using HarmonyLib;


namespace HoloCheck.Patches
{
    //[HarmonyPatch(typeof(GameNetworkManager))]
    public class UIPatches
    {

        //[HarmonyPatch("ConnectionApproval")]
        //[HarmonyPrefix]
        private static void AwakePostFix(GameNetworkManager __instance)
        {
            HoloCheck.Logger.LogInfo("VersionPatches awoken!");
            //MORECOMPANY case - Check if MoreCompany is in effect
            HoloCheck.Logger.LogInfo("Detected Version = " + __instance.gameVersionNum.ToString());
            if (__instance.gameVersionNum > 9950 && __instance.gameVersionNum != HoloCheck.targetVersion)
            {
                HoloCheck.Logger.LogWarning("Detected manipulation of Version Number from MoreCompany, applying quick-fix");
                //Offset should be 9950 + originalVersion - numberofAdditionalMods.
                //To prevent version level modlist checking, add by 1 to hide this mod.
                //__instance.gameVersionNum += 1;
                HoloCheck.Logger.LogInfo("The new version is = " + __instance.gameVersionNum.ToString());
                HoloCheck.targetVersion = __instance.gameVersionNum;
            }
        }
    }
}
