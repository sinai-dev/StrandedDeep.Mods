using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MordhauVoices.RPC;
using MordhauVoices.Voices;
using UnityEngine;

namespace MordhauVoices
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "com.sinai.stranded-deep.mordhau-voices";
        const string NAME = "Mordhau Voices";
        const string VERSION = "1.0.0";

        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }

        public static ConfigEntry<float> Pitch;
        public static ConfigEntry<VoiceTypes> VoiceType;
        public static ConfigEntry<KeyCode> VoiceMenuKey;
        public static ConfigEntry<float> VoiceVolume;
        public static ConfigEntry<float> MaxDistance;

        internal void Awake()
        {
            Instance = this;
            Log = this.Logger;

            VoiceType = Config.Bind("Settings", "Voice Type", VoiceTypes.Plain, "Voice type to use");
            Pitch = Config.Bind("Settings", "Pitch", 1f, new ConfigDescription("Pitch of the voice", new AcceptableValueRange<float>(0.65f, 1.35f)));
            VoiceMenuKey = Config.Bind("Settings", "Voice key bind", KeyCode.X, "Key to press to open the voice line menu");
            VoiceVolume = Config.Bind("Settings", "Voice Volume", 0.8f, new ConfigDescription("Volume of voice lines", new AcceptableValueRange<float>(0.0f, 1.0f)));
            MaxDistance = Config.Bind("Settings", "Max Distance", 125f, "Max player distance for voice lines");

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            VoiceLibrary.Init();
            VoicePlayer.Init();

            Log.LogMessage($"Enabled {NAME}, patched methods: {harmony.GetPatchedMethods().Count()}");
        }

        internal void Update()
        {
            Menu.Update();
        }

        internal void OnGUI()
        {
            Menu.OnGUI();
        }
    }
}
