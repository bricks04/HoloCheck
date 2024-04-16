using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using BepInEx.Logging;

namespace HoloCheck.Patches
{
    internal class TestNetworkingSignals
    {

        private void Awake()
        {
            HoloCheck.Logger.LogInfo("Successfully loaded TestNetworkingSignals");

        }
    }
}
