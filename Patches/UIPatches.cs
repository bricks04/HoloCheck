using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace HoloCheck.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    public class UIPatches
    {
        public static AssetBundle HoloCheckUIAssets;
        public static GameObject holoCheckUI;

        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        private static void AwakePrefix()
        {
            // Load external asset pack that contains basic UI stuff, and instantiate them in the same way MoreCompanyAssets does
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            AssetBundle proposedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "holocheckassetbundle"));
            if (proposedAssetBundle == null)
            {
                HoloCheck.Logger.LogError("Failed to load custom assets. You can safely ignore this error. "); // ManualLogSource for your plugin
                return;
            }
            else
            {
                HoloCheckUIAssets = proposedAssetBundle;
                HoloCheck.Logger.LogInfo("Custom assets loaded!");
                
            }
        }

        //Patch disabled as not ready yet
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void AwakePostfix()
        {
            try
            {
                // Begin Construction
                holoCheckUI = HoloCheckUIAssets.LoadAsset<GameObject>("HoloCheckUI");
                GameObject.Instantiate(holoCheckUI);

                GameObject enableHoloCheckSettingsButton = holoCheckUI.transform.Find("Canvas").Find("ActivateButton").gameObject;
                //This line might not work - how to safely reference a disabled object?
                GameObject disableHoloCheckSettingsButton = holoCheckUI.transform.Find("Canvas").Find("HoloCheckPanel").Find("Back Button").gameObject;
            }
            catch (Exception e)
            {
                HoloCheck.Logger.LogError(e);
            }
        }
    }
}
