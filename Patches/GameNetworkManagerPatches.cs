using HarmonyLib;
using Steamworks.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using GameNetcodeStuff;
using Netcode.Transports.Facepunch;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Buffers;

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
            HoloCheck.Logger.LogInfo("A connection approval check happened!");
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
                //Array[1] is the Steam Id. Use this to compare list
                HoloCheck.Logger.LogInfo("Steam ID = " + array[1]);
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
            }
        }
    }
}
