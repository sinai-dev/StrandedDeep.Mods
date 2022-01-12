using Beam;
using Beam.Terrain;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ModPack.InGameMap
{
    // Inspired by Hantacore's map https://www.nexusmods.com/strandeddeep/mods/164

    public class InGameMap : SubPlugin
    {
        public override string Name => "Ingame Map";

        public static InGameMap Instance { get; private set; }

        // consts
        internal const float INGAME_DIMENSIONS = 3000f;
        internal const float MAP_IMAGE_DIMENSIONS = 650f;
        internal const float REFERENCE_SCREEN_WIDTH = 1024f;
        internal const float REFERENCE_SCREEN_HEIGHT = 768f;

        // Configurable
        public static ConfigEntry<KeyCode> toggleMapKey;
        public static ConfigEntry<bool> revealMissions;
        public static ConfigEntry<bool> autoDiscover;

        // State
        bool mapVisible;
        StrandedWorld lastWorldRef;

        // UI object references
        internal GameObject canvasRoot;

        // Object cache
        readonly List<CachedPlayer> cachedPlayers = new();
        readonly List<CachedMapZone> cachedMapZones = new();

        // ~~~~~~~~ Initialization ~~~~~~~~

        public override void Initialize()
        {
            Instance = this;

            toggleMapKey = Bind("Toggle Map Keybind", KeyCode.F1, "The hotkey for toggling the map");
            revealMissions = Bind("Reveal Missions", false, "Shows a special icon for missions before you have discovered them", SettingChanged);
            autoDiscover = Bind("Auto-Discover", false, "(Cheat mode) Automatically discover all islands and missions", SettingChanged);

            InitMap();
            MarkerBase.InitTemplates();

            base.Initialize();
        }

        private void SettingChanged(SettingChangedEventArgs args)
        {
            RebuildMap();
        }

        public static bool IsGameReady() => Game.State == GameState.LOAD_GAME
            && Singleton<StrandedWorld>.Instance
            && Singleton<StrandedWorld>.Instance.Zones != null
            && Singleton<StrandedWorld>.Instance.Zones.Length >= 48;

        private void InitMap()
        {
            canvasRoot = new("Canvas");
            canvasRoot.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(canvasRoot);

            var canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 12;

            var canvasScaler = canvasRoot.AddComponent<CanvasScaler>();
            canvasScaler.referenceResolution = new(REFERENCE_SCREEN_WIDTH, REFERENCE_SCREEN_HEIGHT);
            canvasScaler.referencePixelsPerUnit = 100f;
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            var background = MarkerBase.CreateTemplate(canvasRoot, "Background", MAP_IMAGE_DIMENSIONS, 2000, "ingamemap.background.png");
            background.rectTransform.anchoredPosition = Vector2.zero;

            canvasRoot.SetActive(false);

            Log("Initialized map");
        }

        public override void Update()
        {
            try
            {
                if (!IsGameReady())
                {
                    mapVisible = false;
                    lastWorldRef = null;
                    if (canvasRoot)
                        canvasRoot.gameObject.SetActive(false);
                    return;
                }

                // If world instance changed, rebuild map.
                if (lastWorldRef != Singleton<StrandedWorld>.Instance)
                {
                    RebuildMap();
                    lastWorldRef = Singleton<StrandedWorld>.Instance;
                }

                // Check recache players
                if (cachedPlayers.Count != PlayerRegistry.AllPlayers.Count 
                    || cachedPlayers.Any(it => !(it.refPlayer as Component)))
                    RebuildPlayers();

                // Check toggle key and map visibility
                if (Input.GetKeyDown(toggleMapKey.Value))
                    mapVisible = !mapVisible;

                if (Input.GetKeyDown(KeyCode.Escape))
                    mapVisible = false;

                if (canvasRoot.activeSelf != mapVisible)
                    canvasRoot.SetActive(mapVisible);

                // Update map state
                if (mapVisible)
                {
                    RefreshMapZones();
                    RefreshPlayers();
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Exception in OnUpdate: {ex}");
            }
        }

        private void ClearObjectPools()
        {
            foreach (var player in cachedPlayers)
                Pool.Return(player);
            cachedPlayers.Clear();

            foreach (var mapZone in cachedMapZones)
                Pool.Return(mapZone);
            cachedMapZones.Clear();
        }

        private void RebuildMap()
        {
            if (!IsGameReady())
                return;

            Log("Rebuilding map...");

            ClearObjectPools();

            for (int i = 0; i < World.MapList.Length; i++)
            {
                var map = World.MapList[i];
                var zone = Singleton<StrandedWorld>.Instance.Zones[i];

                var mapZone = Pool.Borrow<CachedMapZone>(map, zone, World.GenerationZonePositons[i]);
                mapZone.SetHeightmap(map);
                mapZone.SetPosition();
                cachedMapZones.Add(mapZone);
            }
        }

        private void RebuildPlayers()
        {
            Log("Rebuilding player cache...");

            foreach (var player in cachedPlayers)
                Pool.Return(player);
            cachedPlayers.Clear();

            foreach (var player in PlayerRegistry.AllPlayers)
            {
                var cached = Pool.Borrow<CachedPlayer>(player);
                cachedPlayers.Add(cached);
            }
        }

        private void RefreshMapZones()
        {
            for (int i = 0; i < cachedMapZones.Count; i++)
            {
                var mapZone = cachedMapZones[i];
                bool discovered = mapZone.RefZone.HasVisited || mapZone.RefZone.IsStartingIsland || autoDiscover.Value;
                mapZone.UpdateState(discovered);
            }
        }

        private void RefreshPlayers()
        {
            // Update players
            foreach (var player in cachedPlayers)
                player.SetPosition();
        }
    }
}
