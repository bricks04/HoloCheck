using HarmonyLib;
using System.Linq;
using System;
using System.Text;
using Unity.Netcode;
using Steamworks;


namespace HoloCheck.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    public class GameNetworkManagerPatches
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void AwakePostFix()
        {
            HoloCheck.Logger.LogInfo("GameNetworkManager awoken!");
        }

        [HarmonyPatch("ConnectionApproval")]
        [HarmonyPostfix]
        private static void ConnectionApprovalPostFix(GameNetworkManager __instance, NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            // Debug, highly inefficient as we decode the payload later anyway.
            //HoloCheck.Logger.LogInfo("Decoded Payload of joiner : " + Encoding.ASCII.GetString(request.Payload));
            if (HoloCheck.allowedSteamIDs.Length > 0) 
            {
                CheckForSteamID(__instance, request, response);
            }
            CheckForPassPIN(__instance, request, response);
        }

        [HarmonyPatch("SetConnectionDataBeforeConnecting")]
        [HarmonyPostfix]
        // Issue - MoreCompany LAN overrides the payload to store the intended lobby size. Either read the payload and transplant the lobby size to the new payload, or disable this check in LAN.
        [HarmonyAfter(["me.swipez.melonloader.morecompany"])]
        private static void SetConnectionDataPostFix(GameNetworkManager __instance)
        {
            HoloCheck.Logger.LogInfo("Checking Connection data = " + Encoding.ASCII.GetString(NetworkManager.Singleton.NetworkConfig.ConnectionData));
            // Check if Payload injection method is active, if so, inject passcode into payload.
            if (HoloCheck.payloadInjection)
            {
                __instance.localClientWaitingForApproval = true;
                HoloCheck.Logger.LogInfo("Pass to inject: " + HoloCheck.passkey.ToString());
                // gameVersionNum,SteamId,Passkey
                if (__instance.disableSteam)
                {
                    NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(__instance.gameVersionNum.ToString() + "," + 32 + "," + HoloCheck.passkey.ToString());
                }
                else
                {
                    NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(__instance.gameVersionNum + "," + (ulong)SteamClient.SteamId + "," + HoloCheck.passkey.ToString());
                }
                HoloCheck.Logger.LogInfo("Post-injection Connection data = " + Encoding.ASCII.GetString(NetworkManager.Singleton.NetworkConfig.ConnectionData));
            }
            
        }

        private static void CheckForSteamID(GameNetworkManager __instance, NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            HoloCheck.Logger.LogInfo("A connection approval check happened! Checking for steamID");
            HoloCheck.Logger.LogInfo(__instance.disallowConnection);
            HoloCheck.Logger.LogInfo(__instance.disableSteam);
            HoloCheck.Logger.LogInfo(response.Approved);

            bool flag = !__instance.disallowConnection;
            //Check if the instance is disallowing connections
            //Or if response was already denied
            //Or if Steam is disabled eg. LAN mode.
            //(Check that both are true)
            if (__instance.disableSteam)
            {
                HoloCheck.Logger.LogWarning("You are in LAN mode, or you somehow have Steam disabled! User ID checks will not be conducted. ");
            }
            if (flag && response.Approved && !__instance.disableSteam)
            {
                //Can't extract variables in a function, so we regenerate request payload variables
                string @string = Encoding.ASCII.GetString(request.Payload);
                string[] array = @string.Split(",");
                //DEBUG - Show whats in the payload
                foreach (String item in array)
                {
                    HoloCheck.Logger.LogInfo(item);
                }

                //Array[1] is the Steam Id. Use this to compare list
                HoloCheck.Logger.LogInfo("Steam ID = " + array[1]);
                HoloCheck.Logger.LogInfo(HoloCheck.allowedSteamIDs);
                if (HoloCheck.allowedSteamIDs.Contains(array[1]))
                {
                    HoloCheck.Logger.LogWarning("User " + array[1] + " was approved to join the server! ");
                    //If array[1] in approvedUsersList, flag = true
                    flag = true;
                }
                else
                {
                    //else flag = false, response.reason = "Your account has not been approved to join this server."
                    HoloCheck.Logger.LogWarning("User " + array[1] + " attempted to join the server, but has been denied because they are not on the list. ");
                    response.Reason = "Your account has not been approved to join this server.";
                    flag = false;
                }

                //Set the response.approved to what the flag is
                response.Approved = flag;
                //No need to return anything, just have the response variables done and dusted
                //POSSIBLE ISSUE - Player entities still seem to remain if they are removed with this method. Check if this is ok?
            }
        }

        private static void CheckForPassPIN(GameNetworkManager __instance, NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            HoloCheck.Logger.LogInfo("A connection approval check happened! Checking for pass");

            bool flag = response.Approved;
            if (!__instance.disallowConnection)
            {
                //Can't extract variables in a function, so we regenerate request payload variables
                string @string = Encoding.ASCII.GetString(request.Payload);
                string[] array = @string.Split(",");

                //Array[0] is the Version ID. In a correct PIN scenario, the correct ID is {PIN HERE}{ORIGINAL VERSION HERE} eg. pin 1234, version 50 = 123450
                //Array[2] is the Passkey IF payload injection has been enabled by the connecting client. MAKE SURE you check if the payload has this value available?
                //Check for payload injection first, use Version checking as a fallback. In theory, most versions of HoloCheck will attempt to use Version Checking
                //If versions do not use version checking, they should be using payload injection. 
                
                HoloCheck.Logger.LogInfo("Length of Payload = " + array.Length.ToString());
                switch(array.Length)
                {
                    case 3:
                        HoloCheck.Logger.LogInfo("Attempted Passkey = " + array[2]);
                        HoloCheck.Logger.LogInfo("Passkey to check = " + HoloCheck.passkey.ToString());
                        if (array[2].ToString() == HoloCheck.passkey.ToString())
                        {
                            HoloCheck.Logger.LogWarning("User's password matches.");
                        }
                        else
                        {
                            HoloCheck.Logger.LogWarning("User attempted to join the server, but has been denied because their passkey does not match. Overriding any previous connection approval.");
                            response.Reason = "Your account has not been approved to join this server.";
                            flag = false;
                        }
                        break;
                    case 1 or 2:
                        HoloCheck.Logger.LogInfo("Attempted Version = " + array[0]);
                        if (array[0] == HoloCheck.targetVersion.ToString())
                        {
                            HoloCheck.Logger.LogWarning("User's password matches.");
                        }
                        else
                        {
                            HoloCheck.Logger.LogWarning("User attempted to join the server, but has been denied because their passkey does not match. Overriding any previous connection approval.");
                            response.Reason = "Your account has not been approved to join this server.";
                            flag = false;
                        }
                        break;
                    default:
                        HoloCheck.Logger.LogError("No case found for payload handling! Immediately rejecting user by default.");
                        response.Reason = "Your account cannot join the server for safety reasons.";
                        flag = false;
                        break;
                }


                //Set the response.approved to what the flag is
                response.Approved = flag;
            }
        }
    }
}
