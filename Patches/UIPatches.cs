using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace HoloCheck.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    public class UIPatches
    {
        public static AssetBundle HoloCheckUIAssets;

        //Patch disabled as not ready yet
        //[HarmonyPatch("Awake")]
        //[HarmonyPostfix]
        private static void Awake()
        {
            // Load external asset pack that contains basic UI stuff, and instantiate them in the same way MoreCompanyAssets does
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            HoloCheckUIAssets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "holocheckuipack"));
            if (HoloCheckUIAssets == null)
            {
                HoloCheck.Logger.LogError("Failed to load custom assets. You can safely ignore this error. "); // ManualLogSource for your plugin
                return;
            }
            else
            {
                HoloCheck.Logger.LogInfo("Custom assets loaded!");
            }
        }
    }
}
