using Beam;
using Beam.Crafting;
using Beam.Events;
using Beam.UI;
using BepInEx.Configuration;
using Funlabs;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModPack.Cheats
{
    public class Cheats : SubPlugin
    {
        public override string Name => "Cheats";

        internal static Cheats Instance { get; private set; }

        private static ConfigEntry<bool> Invincible;
        private static ConfigEntry<bool> SuperSpeed;
        private static ConfigEntry<bool> UnlockAircraftPartRecipes;
        private static ConfigEntry<bool> UnbreakableTools;
        private static ConfigEntry<bool> RefundMaterials;
        private static ConfigEntry<bool> InfiniteAirTank;
        private static ConfigEntry<bool> InfiniteGasTank;
        private static ConfigEntry<bool> InfiniteFuelStill;
        private static ConfigEntry<bool> InfiniteWaterStill;
        private static ConfigEntry<bool> InfiniteCampfire;
        private static ConfigEntry<bool> InfiniteFireAndTorch;

        public override void Initialize()
        {
            Instance = this;

            Invincible = Bind("Invincible", false, string.Empty);
            SuperSpeed = Bind("Super Speed", false, string.Empty);
            UnlockAircraftPartRecipes = Bind("Unlock Aircraft Part Recipes", false, "Unlocks the recipes normally acquired from bosses.");
            UnbreakableTools = Bind("Unbreakable Tools", false, string.Empty);
            RefundMaterials = Bind("Refund All Materials", false, "When dismantling/destroying something, receive all materials back");
            InfiniteAirTank = Bind("Infinite Air Tank", false, string.Empty);
            InfiniteGasTank = Bind("Infinite Gas Tank", false, string.Empty);
            InfiniteFuelStill = Bind("Infinite Fuel Still mash", false, string.Empty);
            InfiniteWaterStill = Bind("Infinite Water Still fibre", false, string.Empty);
            InfiniteCampfire = Bind("Infinite Campfire", false, string.Empty);
            InfiniteFireAndTorch = Bind("Infinite Fire Torch", false, string.Empty);

            base.Initialize();
        }

        public override void OnEnabled()
        {
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            if (PlayerRegistry.LocalPlayer != null
                && PlayerRegistry.LocalPlayer.DeveloperMode
                && PlayerRegistry.LocalPlayer.Statistics)
            {
                PlayerRegistry.LocalPlayer.DeveloperMode._inFlyMode = false;
                PlayerRegistry.LocalPlayer.Statistics._invincible = false;
            }
        }

        #region (Update) INVINCIBLE, SUPER SPEED, UNLOCK BOSS RECIPES

        private float timeOfLastUpdate;

        public override void Update()
        {
            if (Time.realtimeSinceStartup - timeOfLastUpdate < 1f)
                return;
            timeOfLastUpdate = Time.realtimeSinceStartup;

            if (PlayerRegistry.LocalPlayer == null
                || !PlayerRegistry.LocalPlayer.DeveloperMode
                || !PlayerRegistry.LocalPlayer.Statistics)
                return;

            PlayerRegistry.LocalPlayer.DeveloperMode._inFlyMode = SuperSpeed.Value;
            PlayerRegistry.LocalPlayer.Statistics._invincible = Invincible.Value;

            if (UnlockAircraftPartRecipes.Value)
                CheckAircraftRecipeCheat();
        }

        private void CheckAircraftRecipeCheat()
        {
            var player = PlayerRegistry.LocalPlayer;
            if (!player.Crafter
                || player.Crafter.CraftingCombinations == null
                || player.Crafter.CraftingCombinations.Combinations == null)
                return;

            foreach (var recipe in player.Crafter.CraftingCombinations.Combinations)
            {
                if (recipe.Name.Contains("AIRCRAFT"))
                    recipe.Unlocked = true;
            }
        }

        #endregion

        #region UNBREAKABLE TOOLS

        [HarmonyPatch(typeof(InteractiveObject), nameof(InteractiveObject.DamageObject))]
        [HarmonyPrefix]
        static bool Prefix_InteractiveObject_DamageObject(InteractiveObject __instance, IBase obj)
        {
            if (!UnbreakableTools.Value)
                return true;

            if (__instance._canDamage && obj.IsDamageable)
                obj.Damage(__instance._damage, __instance);

            return false;
        }

        #endregion

        #region REFUND ALL MATERIALS

        [HarmonyPatch(typeof(MaterialRefunder), nameof(MaterialRefunder.RefundMaterials))]
        [HarmonyPrefix]
        static bool Prefix_MaterialRefunder_RefundMaterials(MaterialRefunder __instance, int playerID)
        {
            if (!RefundMaterials.Value)
                return true;

            OverrideMaterialRefunder(__instance, playerID);
            return false;
        }

        static void OverrideMaterialRefunder(MaterialRefunder __instance, int playerID)
        {
            __instance.SpawnGib();
            if (Game.Mode.IsClient())
                return;

            EventManager.RaiseEvent(new ExperienceEvent(playerID, PlayerSkills.Skills.PHYSICAL, PlayerSkills.ExpSource.DECONSTRUCTION_HIT));

            foreach (var uid in __instance._materials)
            {
                SaveablePrefab saveablePrefab = MultiplayerMng.Instantiate<SaveablePrefab>(uid, MiniGuid.New(), null);
                saveablePrefab.transform.position = __instance.transform.position + Vector3.up * 1f + UnityEngine.Random.insideUnitSphere;
            }
        }

        #endregion

        #region INFINITE AIR TANK

        [HarmonyPatch(typeof(InteractiveObject_AIRTANK), nameof(InteractiveObject_AIRTANK.ValidatePrimary))]
        [HarmonyPrefix]
        static void Prefix_InteractiveObject_AIRTANK_ValidatePrimary(InteractiveObject_AIRTANK __instance)
        {
            if (InfiniteAirTank.Value)
                __instance.DurabilityPoints = 3f;
        }

        [HarmonyPatch(typeof(InteractiveObject_AIRTANK), nameof(InteractiveObject_AIRTANK.Use))]
        [HarmonyPrefix]
        static void Prefix_InteractiveObject_AIRTANK_Use(InteractiveObject_AIRTANK __instance)
        {
            if (InfiniteAirTank.Value)
                __instance.DurabilityPoints += 1f;
        }

        [HarmonyPatch(typeof(InteractiveObject_AIRTANK), nameof(InteractiveObject_AIRTANK.Use))]
        [HarmonyPostfix]
        static void Postfix_InteractiveObject_AIRTANK_Use(InteractiveObject_AIRTANK __instance)
        {
            if (InfiniteAirTank.Value)
                __instance.DurabilityPoints = 3f;
        }

        #endregion

        #region INFINITE GAS TANK

        [HarmonyPatch(typeof(MotorVehicleMovement), nameof(MotorVehicleMovement.Awake))]
        [HarmonyPrefix]
        static void Prefix_MotorVehicleMovement_Awake(MotorVehicleMovement __instance)
        {
            if (InfiniteGasTank.Value)
            {
                __instance._fuel = __instance.FuelCapacity;
                __instance._ranOutOfFuel = false;
            }
        }

        [HarmonyPatch(typeof(MotorVehicleMovement), nameof(MotorVehicleMovement.Move))]
        [HarmonyPrefix]
        static void Prefix_MotorVehicleMovement_Move(MotorVehicleMovement __instance)
        {
            if (InfiniteGasTank.Value)
                __instance._fuel = __instance.FuelCapacity;
        }

        [HarmonyPatch(typeof(MotorVehicleMovement), nameof(MotorVehicleMovement.Move))]
        [HarmonyPostfix]
        static void Postfix_MotorVehicleMovement_Move(MotorVehicleMovement __instance)
        {
            if (InfiniteGasTank.Value)
                __instance._fuel = __instance.FuelCapacity;
        }

        #endregion

        #region INFINITE FUEL STILL

        [HarmonyPatch(typeof(Constructing_FUEL_STILL_BOILER), nameof(Constructing_FUEL_STILL_BOILER.StartBoiling))]
        [HarmonyPrefix]
        static void Prefix_Constructing_FUEL_STILL_BOILER_StartBoiling(Constructing_FUEL_STILL_BOILER __instance)
        {
            if (InfiniteFuelStill.Value)
                __instance._mash = 4;
        }

        [HarmonyPatch(typeof(Constructing_FUEL_STILL_BOILER), nameof(Constructing_FUEL_STILL_BOILER.Boil))]
        [HarmonyPrefix]
        static void Prefix_Constructing_FUEL_STILL_BOILER_Boil(Constructing_FUEL_STILL_BOILER __instance)
        {
            if (InfiniteFuelStill.Value)
                __instance._mash = 4;
        }

        [HarmonyPatch(typeof(Constructing_FUEL_STILL_COLLECTOR), nameof(Constructing_FUEL_STILL_COLLECTOR.Fill))]
        [HarmonyPrefix]
        static void Prefix_Constructing_FUEL_STILL_COLLECTOR_Fill(Constructing_FUEL_STILL_COLLECTOR __instance)
        {
            if (InfiniteFuelStill.Value)
                __instance._fuel = __instance.FuelCapacity;
        }

        [HarmonyPatch(typeof(Constructing_FUEL_STILL_COLLECTOR), nameof(Constructing_FUEL_STILL_COLLECTOR.Fill))]
        [HarmonyPostfix]
        static void Postfix_Constructing_FUEL_STILL_COLLECTOR_Fill(Constructing_FUEL_STILL_COLLECTOR __instance, ref float __result)
        {
            if (InfiniteFuelStill.Value)
                __result = __instance._fuel = __instance.FuelCapacity;
        }

        #endregion

        #region INFINITE WATER STILL

        [HarmonyPatch(typeof(Constructing_STILL), nameof(Constructing_STILL.Boil))]
        [HarmonyPrefix]
        static void Prefix_Constructing_STILL_Boil(Constructing_STILL __instance)
        {
            if (InfiniteWaterStill.Value)
                __instance._fibre = 4;
        }

        [HarmonyPatch(typeof(Constructing_STILL), nameof(Constructing_STILL.Boil))]
        [HarmonyPostfix]
        static void Postfix_Constructing_STILL_Boil(Constructing_STILL __instance)
        {
            if (InfiniteWaterStill.Value)
                __instance._fibre = 4;
        }

        [HarmonyPatch(typeof(Constructing_STILL_COLLECTOR), nameof(Constructing_STILL_COLLECTOR.Drink))]
        [HarmonyPatch(typeof(Constructing_STILL_COLLECTOR), nameof(Constructing_STILL_COLLECTOR.InteractWithObject))]
        [HarmonyPrefix]
        static void Prefix_Constructing_STILL_COLLECTOR_Drink_InteractWithObject(Constructing_STILL_COLLECTOR __instance)
        {
            if (InfiniteWaterStill.Value)
                __instance._water = 4;
        }

        [HarmonyPatch(typeof(Constructing_STILL_COLLECTOR), nameof(Constructing_STILL_COLLECTOR.Drink))]
        [HarmonyPatch(typeof(Constructing_STILL_COLLECTOR), nameof(Constructing_STILL_COLLECTOR.InteractWithObject))]
        [HarmonyPostfix]
        static void Postfix_Constructing_STILL_COLLECTOR_Drink_InteractWithObject(Constructing_STILL_COLLECTOR __instance)
        {
            if (InfiniteWaterStill.Value)
                __instance._water = 4;
        }

        #endregion

        #region INFINITE CAMPFIRE

        [HarmonyPatch(typeof(Construction_CAMPFIRE), nameof(Construction_CAMPFIRE.BurnManual))]
        [HarmonyPostfix]
        static void Postfix_Construction_CAMPFIRE_BurnManual(Construction_CAMPFIRE __instance)
        {
            if (InfiniteCampfire.Value)
                __instance._fuelHours = 12f;
        }

        [HarmonyPatch(typeof(Construction_CAMPFIRE), nameof(Construction_CAMPFIRE.RemoveFuel))]
        [HarmonyPrefix]
        static bool Prefix() => !InfiniteCampfire.Value;

        #endregion

        #region INFINITE FIRE TORCH

        [HarmonyPatch(typeof(Interactive_FIRE_TORCH), nameof(Interactive_FIRE_TORCH.BurnManual))]
        [HarmonyPatch(typeof(Interactive_FIRE_TORCH), nameof(Interactive_FIRE_TORCH.RemoveFuel))]
        [HarmonyPrefix]
        static bool Prefix_Interactive_FIRE_TORCH_BurnManual_RemoveFuel()
        {
            return false;
        }

        #endregion
    }
}
