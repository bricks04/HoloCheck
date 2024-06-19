using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using UnityEngine.Device;
using System.Text.RegularExpressions;


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

        public static GameObject passkeyField;
        public static GameObject passkeyMode;
        public static GameObject pendingChangesAlert;
        public static GameObject revealPasskeyButton;
        public static GameObject changePasskeyButton;

        private static RectTransform settingsClosedPasskeyPosition;
        private static RectTransform settingsOpenPasskeyPosition;

        public static GameObject injectorButton;
        public static GameObject injectorText;

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
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void AwakePostfix()
        {
            try
            {
                // Begin Construction
                //holoCheckUI = The object data loaded from the assetpack. Treat this like your instantiate template
                //instantiatedUI = The actual objects in game. Use this to listen for user input and stuff.
                holoCheckUI = HoloCheckUIAssets.LoadAsset<GameObject>("HoloCheckUI");
                instantiatedUI = GameObject.Instantiate(holoCheckUI);

                enableHoloCheckSettingsButton = instantiatedUI.transform.Find("Canvas").Find("ActivateButton").gameObject;
                //HoloCheck.Logger.LogInfo(enableHoloCheckSettingsButton);
                disableHoloCheckSettingsButton = instantiatedUI.transform.Find("Canvas").Find("HoloCheckPanel").Find("Back Button").gameObject;
                //HoloCheck.Logger.LogInfo(disableHoloCheckSettingsButton);
                settingsPanel = instantiatedUI.transform.Find("Canvas").Find("HoloCheckPanel").gameObject;
                passkeyMode = instantiatedUI.transform.Find("Canvas").Find("Passkey Mode").gameObject;
                passkeyField = instantiatedUI.transform.Find("Canvas").Find("Passkey Mode").Find("Passkey Field").gameObject;
                pendingChangesAlert = instantiatedUI.transform.Find("Canvas").Find("Passkey Mode").Find("Unsaved Changes Label").gameObject;
                revealPasskeyButton = instantiatedUI.transform.Find("Canvas").Find("Passkey Mode").Find("Reveal Button").gameObject;
                changePasskeyButton = instantiatedUI.transform.Find("Canvas").Find("Passkey Mode").Find("Change Button").gameObject;

                settingsClosedPasskeyPosition = instantiatedUI.transform.Find("Canvas").Find("Menu Closed Passkey").gameObject.GetComponent<RectTransform>();
                settingsOpenPasskeyPosition = instantiatedUI.transform.Find("Canvas").Find("Menu Opened Passkey").gameObject.GetComponent<RectTransform>();

                injectorButton = instantiatedUI.transform.Find("Canvas").Find("HoloCheckPanel").Find("Injector Mode").Find("InjectorButton").gameObject;
                injectorText = instantiatedUI.transform.Find("Canvas").Find("HoloCheckPanel").Find("Injector Mode").Find("Text (TMP)").gameObject;

                passkeyField.GetComponent<TMP_InputField>().text = "****";

                //HoloCheck.Logger.LogInfo(enableHoloCheckSettingsButton.GetComponent<Button>());

                enableHoloCheckSettingsButton.GetComponent<Button>().onClick.AddListener(EnableHoloCheckSettingsPanel);
                disableHoloCheckSettingsButton.GetComponent<Button>().onClick.AddListener(DisableHoloCheckSettingsPanel);
                revealPasskeyButton.GetComponent<Button>().onClick.AddListener(RevealPasskeyButton);
                changePasskeyButton.GetComponent<Button>().onClick.AddListener(ChangePasskeyButton);
                injectorButton.GetComponent<Button>().onClick.AddListener(InjectorButtonPressed);


                if (HoloCheck.payloadInjection)
                {
                    injectorButton.GetComponent<Image>().color = Color.green;
                    injectorText.GetComponent<TextMeshProUGUI>().text = "Payload Injection enabled";
                }
                else
                {
                    injectorButton.GetComponent<Image>().color = Color.white;
                    injectorText.GetComponent<TextMeshProUGUI>().text = "Payload Injection disabled";
                }
                

                // Debugging stuff
                //EnableHoloCheckSettingsPanel();

                PopulateUserList();
                RewriteRuleset();
            }
            catch (Exception e)
            {
                HoloCheck.Logger.LogError(e);
            }
        }

        private static void InjectorButtonPressed()
        {
            HoloCheck.Logger.LogInfo("Injector Button pressed!");
            HoloCheck.payloadInjection = !HoloCheck.payloadInjection;
            if (HoloCheck.payloadInjection)
            {
                injectorButton.GetComponent<Image>().color = Color.green;
                //Invoke VersionPatches here with no passkey to reset the version number.
                VersionPatches.ResetVersionNumber();
                injectorText.GetComponent<TextMeshProUGUI>().text = "Payload Injection enabled";
            }
            else
            {
                injectorButton.GetComponent<Image>().color = Color.white;
                //Forcefully invoke VersionPatches here to ensure that user enters with a modified version number.
                VersionPatches.ChangePasskey(HoloCheck.passkey);
                injectorText.GetComponent<TextMeshProUGUI>().text = "Payload Injection disabled";

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

        private static void ChangePasskeyButton()
        {
            HoloCheck.Logger.LogInfo("Change Passkey button pressed! Attempting to change passkey to " + passkeyField.GetComponent<TMP_InputField>().text);
            string checkResult = CheckStringPasskeyValidity(passkeyField.GetComponent<TMP_InputField>().text);
            if (checkResult == "" | passkeyField.GetComponent<TMP_InputField>().text.Length == 0)
            {
                HoloCheck.passkey = passkeyField.GetComponent<TMP_InputField>().text;
                VersionPatches.ChangePasskey(HoloCheck.passkey);
                HoloCheck.SaveConfig();
                pendingChangesAlert.GetComponent<TextMeshProUGUI>().text = "> Change successful.";
                pendingChangesAlert.SetActive(true);
                RewriteRuleset();
            }
            else
            {
                HoloCheck.Logger.LogError(checkResult.Split("|")[0]);
                pendingChangesAlert.GetComponent<TextMeshProUGUI>().text = checkResult.Split("|")[1];
                pendingChangesAlert.SetActive(true);
            }
            
        }

        //Check for passkey validity. Return an integer that represents if the passkey is OK, or not. 
        public static string CheckStringPasskeyValidity(string passkeyToCheck)
        {
            if (Regex.IsMatch(passkeyToCheck, @"^\d*$"))
            {
                if (Regex.IsMatch(passkeyToCheck, @"^\d{1,4}$"))
                {
                    return "";
                }
                else
                {
                    return "Entered passkey value is too long!|> Entered value too long! (4 max)";
                }
            }
            else
            {
                return "Entered passkey value is not a number!|> Entered value is not a number!";
            }
            return "Internal error!|> Internal Error!";
        }

        private static void RevealPasskeyButton()
        {
            if (passkeyField.GetComponent<TMP_InputField>().text == "****")
            {
                passkeyField.GetComponent<TMP_InputField>().text = HoloCheck.passkey;
            }
            else
            {
                passkeyField.GetComponent<TMP_InputField>().text = "****";
            }
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
                //SteamId steamIdObject = new SteamId();
                //HoloCheck.Logger.LogInfo(Steamworks.SteamFriends.RequestUserInformation(steamIdObject));
            }
            // End the list
            GameObject.Instantiate(endOfList, parent.transform);
        }

        private static void RewriteRuleset()
        {
            GameObject rulesetObject = instantiatedUI.transform.Find("Canvas").Find("HoloCheckPanel").Find("CurrentRulesetBorder").Find("Text (TMP)").gameObject;
            string result = "Active Ruleset: \n\n";
            if (HoloCheck.passkey != "")
            {
                result = result + "> Users must install HoloCheck\n\n> Users must enter the correct passkey\n\n";
            }
            if (HoloCheck.allowedSteamIDs.Length > 0)
            {
                result = result + "> The user's SteamID must be present on the whitelist.\nWhitelist Length : " + HoloCheck.allowedSteamIDs.Length.ToString() + "\n\n";
            }
            rulesetObject.GetComponent<TextMeshProUGUI>().text = result;
        }
    }
}
