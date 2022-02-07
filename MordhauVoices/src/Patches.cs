using Beam;
using Bolt;
using BoltInternal;
using HarmonyLib;
using MordhauVoices.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MordhauVoices
{
    // Patch to make sure VoiceLineEvent_Meta is registered when other Events get registered.

    [HarmonyPatch(typeof(BoltNetworkInternal_User), nameof(BoltNetworkInternal_User.EnvironmentSetup))]
    public class BoltNetworkInternal_User_EnvironmentSetup
    {
        static void Postfix()
        {
            Factory.Register(VoiceLineEvent_Meta.Instance);
            Plugin.Log.LogMessage($"Registered VoiceLineEvent_Meta");
        }
    }

    // Patch to register RPCManager component

    [HarmonyPatch(typeof(BoltCore), nameof(BoltCore.CreateBoltBehaviourObject))]
    public class BoltCore_CreateBoltBehaviourObject
    {
        static void Postfix()
        {
            BoltCore._globalBehaviours.Add(new(new(), typeof(RPCManager)));
            Plugin.Log.LogMessage($"Registered RPCManager in _globalBehaviours");
        }
    }

    // Patch to make Toolbelt hotkey not trigger when we use voice lines

    [HarmonyPatch(typeof(HotkeyController), nameof(HotkeyController.DoHotkey))]
    public class HotkeyController_BlockHotkeyInput
    {
        static bool Prefix()
        {
            return !Menu.BlockToolbeltHotkeys;
        }
    }
}
