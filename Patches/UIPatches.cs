using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;


namespace HoloCheck.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    public class UIPatches
    {
        public static AssetBundle HoloCheckUIAssets;
        public static GameObject holoCheckUI;
        public static GameObject instantiatedUI;

        public static GameObject enableHoloCheckSettingsButton;
        public static GameObject disableHoloCheckSettingsButton;
        public static GameObject settingsPanel;

        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        private static void AwakePrefix()
        {
            // Load external asset pack that contains basic UI stuff, and instantiate them in the same way MoreCompanyAssets does
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            AssetBundle proposedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "holocheckassetbundle"));
            if (proposedAssetBundle == null)
            {
                HoloCheck.Logger.LogError("Failed to load custom assets. If custom assets were previously loaded, you can safely ignore this error. "); // ManualLogSource for your plugin
                return;
            }
            else
            {
                HoloCheckUIAssets = proposedAssetBundle;
                HoloCheck.Logger.LogInfo("Custom assets loaded!");
                
            }
        }

        //Patch disabled as not ready yet
        //[HarmonyPatch("Awake")]
        //[HarmonyPostfix]
        private static void AwakePostfix()
        {
            try
            {
                // Begin Construction
                holoCheckUI = HoloCheckUIAssets.LoadAsset<GameObject>("HoloCheckUI");
                instantiatedUI = GameObject.Instantiate(holoCheckUI);

                enableHoloCheckSettingsButton = holoCheckUI.transform.Find("Canvas").Find("ActivateButton").gameObject;
                HoloCheck.Logger.LogInfo(enableHoloCheckSettingsButton);
                disableHoloCheckSettingsButton = holoCheckUI.transform.Find("Canvas").Find("HoloCheckPanel").Find("Back Button").gameObject;
                HoloCheck.Logger.LogInfo(disableHoloCheckSettingsButton);
                settingsPanel = holoCheckUI.transform.Find("Canvas").Find("HoloCheckPanel").gameObject;

                HoloCheck.Logger.LogInfo(enableHoloCheckSettingsButton.GetComponent<Button>());
                //How do I assign listeners? This doesnt work
                enableHoloCheckSettingsButton.GetComponent<Button>().onClick.AddListener(EnableHoloCheckSettingsPanel);
                // Debugging stuff
                EnableHoloCheckSettingsPanel();

                PopulateUserList();
            }
            catch (Exception e)
            {
                HoloCheck.Logger.LogError(e);
            }
        }

        //Potentially stupid way of doing this
        private static void EnableHoloCheckSettingsPanel()
        {
            HoloCheck.Logger.LogInfo("Enable HoloCheck Settings button pressed!");
            HoloCheck.displaySettings = true;
            settingsPanel.SetActive(true);
            enableHoloCheckSettingsButton.SetActive(false);
        }

        private static void DisableHoloCheckSettingsPanel()
        {
            HoloCheck.Logger.LogInfo("Disable HoloCheck settings button pressed!");
            HoloCheck.displaySettings = false;
            settingsPanel.SetActive(false);
            enableHoloCheckSettingsButton.SetActive(true);
        }

        private static void PopulateUserList()
        {
            GameObject originalEntry = holoCheckUI.transform.Find("Canvas").Find("HoloCheckPanel").Find("UserScrollArea").Find("Content").Find("UserPanel").gameObject;
            GameObject parent = instantiatedUI.transform.Find("Canvas").Find("HoloCheckPanel").Find("UserScrollArea").Find("Content").gameObject;
            GameObject endOfList = holoCheckUI.transform.Find("Canvas").Find("HoloCheckPanel").Find("UserScrollArea").Find("Content").Find("EndPanel").gameObject;

            // Clear the existing list
            GameObject.Destroy(instantiatedUI.transform.Find("Canvas").Find("HoloCheckPanel").Find("UserScrollArea").Find("Content").Find("UserPanel").gameObject);
            GameObject.Destroy(instantiatedUI.transform.Find("Canvas").Find("HoloCheckPanel").Find("UserScrollArea").Find("Content").Find("EndPanel").gameObject);
            foreach (var steamId in HoloCheck.allowedSteamIDs)
            {
                GameObject entry = GameObject.Instantiate(originalEntry, parent.transform);
                entry.transform.GetChild(0).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = steamId;
                entry.transform.GetChild(1).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "This is my username!";
                //SteamId steamIdObject = new SteamId();
                //HoloCheck.Logger.LogInfo(Steamworks.SteamFriends.RequestUserInformation(steamIdObject));
            }
            // End the list
            GameObject.Instantiate(endOfList, parent.transform);
        }
    }
}
