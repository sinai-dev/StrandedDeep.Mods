using Beam;
using Beam.Crafting;
using Beam.UI;
using Beam.Utilities;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModPack.ContainerTweaks
{
    public class ContainerTweaks : SubPlugin
    {
        public override string Name => "Container Tweaks";

        internal static ContainerTweaks Instance { get; private set; }

        private static ConfigEntry<bool> AllowStoringContainers;
        private static ConfigEntry<bool> MaxContainerSizes;
        private static ConfigEntry<bool> NormalLootQuantity;

        private static bool _doneInitialSetup;

        public override void Initialize()
        {
            Instance = this;

            AllowStoringContainers = Bind("Allow storing containers", false, "Allows containers to be placed inside other containers");
            MaxContainerSizes = Bind("Max container sizes", false, "Makes it so inventory and containers use maximum slots.", SettingChanged_MaxContainers);
            NormalLootQuantity = Bind("Normal Loot Quantity", false, "Limits the amount of loot found in containers to the vanilla container size, to prevent extra loot");

            base.Initialize();
        }

        private static float timeOfLastSetupCheck;

        public override void Update()
        {
            base.Update();

            if (!_doneInitialSetup)
            {
                if (Time.realtimeSinceStartup - timeOfLastSetupCheck < 1f)
                    return;
                timeOfLastSetupCheck = Time.realtimeSinceStartup;

                try
                {
                    CheckBuildAllContainerOriginalValues();

                    if (originalContainerSizes.Any())
                    {
                        if (Enabled)
                            SetAllContainers();

                        _doneInitialSetup = true;
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"Exception setting up ContainerTweaks: {ex}");
                }
            }
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            SetAllContainers();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            SetAllContainers();
        }

        #region ALLOW STORING CONTAINERS

        [HarmonyPatch(typeof(SlotStorage), nameof(SlotStorage.CanPush), new[] { typeof(IPickupable), typeof(bool), typeof(bool) })]
        [HarmonyPrefix]
        static bool Prefix_SlotStorage_CanPush(SlotStorage __instance, ref bool __result, IPickupable pickupable, bool force, bool notification)
        {
            if (!AllowStoringContainers.Value)
                return true;

            __result = Override_SlotStorage_CanPush(__instance, pickupable, force, notification);
            return false;
        }

        static bool Override_SlotStorage_CanPush(SlotStorage __instance, IPickupable pickupable, bool force, bool notification)
        {
            if (pickupable.IsNullOrDestroyed() || (!force && !pickupable.CanPickUp) || __instance.Has(pickupable))
                return false;

            /* Changed: Commented this block out */

            //if (!__instance._storeOtherStorage && pickupable.CraftingType.InteractiveType == InteractiveType.CONTAINER)
            //{
            //    if (notification)
            //        __instance.OnPushed(pickupable, false);
            //    return false;
            //}

            if (__instance.GetSlot(pickupable.CraftingType, false) == null)
            {
                if (notification)
                    __instance.OnPushed(pickupable, false);
                return false;
            }

            return true;
        }

        #endregion

        #region MAX CONTAINER SIZES

        static readonly Dictionary<string, int> originalContainerSizes = new();

        const string INVENTORY_KEY = "INVENTORY_MENU_BACKPACK_TITLE";
        const string RAFT_KEY = "RAFT"; // made up by me

        private void SettingChanged_MaxContainers(SettingChangedEventArgs args)
        {
            SetAllContainers();
        }

        private void CheckBuildAllContainerOriginalValues()
        {
            if (originalContainerSizes.Any())
                return;

            foreach (var container in Resources.FindObjectsOfTypeAll<Interactive_STORAGE>())
            {
                if (originalContainerSizes.ContainsKey(container.PrefabId.ToString()))
                    continue;
                originalContainerSizes.Add(container.PrefabId.ToString(), container._slotStorage._slotCount);
            }

            foreach (var container in Resources.FindObjectsOfTypeAll<Interactive_RAFT_STORAGE>())
            {
                originalContainerSizes.Add(RAFT_KEY, container._slotStorage._slotCount);
                break;
            }

            if (originalContainerSizes.Any())
            {
                originalContainerSizes.Add(INVENTORY_KEY, 10);
                Instance.LogWarning($"CheckBuildAllContainerOriginalValues : Setup with {originalContainerSizes.Count} entries.");
            }
        }

        private void SetAllContainers()
        {
            foreach (var container in Resources.FindObjectsOfTypeAll<Interactive_STORAGE>())
                SetSlotStorage(container._slotStorage, Enabled && MaxContainerSizes.Value);

            foreach (var container in Resources.FindObjectsOfTypeAll<Interactive_RAFT_STORAGE>())
                SetSlotStorage(container._slotStorage, Enabled && MaxContainerSizes.Value);
        }

        [HarmonyPatch(typeof(StorageRadialMenuPresenter), nameof(StorageRadialMenuPresenter.Show))]
        [HarmonyPrefix]
        static void Prefix_RadialMenuPresenter_Show(StorageRadialMenuPresenter __instance)
        {
            __instance._elementLayoutCount = 12;
        }

        [HarmonyPatch(typeof(SlotStorage), nameof(SlotStorage.Initialize))]
        [HarmonyPatch(typeof(SlotStorage), nameof(SlotStorage.Open))]
        [HarmonyPrefix]
        static void Prefix_SlotStorage_Initialize_Open(SlotStorage __instance)
        {
            SetSlotStorage(__instance, MaxContainerSizes.Value);
        }

        static string GetKey(SlotStorage storage)
        {
            string key;
            if (storage.Name == INVENTORY_KEY)
                key = storage.Name;
            else if (storage._storageContainer.parent.GetComponent<Interactive_STORAGE>() is Interactive_STORAGE interactiveStorage)
                key = interactiveStorage.PrefabId.ToString();
            else
                key = RAFT_KEY;
            return key;
        }

        static void SetSlotStorage(SlotStorage storage, bool toMax)
        {
            string key = GetKey(storage);

            if (!toMax && !originalContainerSizes.ContainsKey(key))
            {
                Instance.LogWarning($"Original sizes doesn't contain key: {key}");
                return;
            }

            int count = toMax ? (key == INVENTORY_KEY ? 10 : 11) : originalContainerSizes[key];

            if (storage._slotCount == count)
                return;

            if (storage._slotData != null)
            {
                if (storage._slotCount < count)
                {
                    // Add from old count to new count
                    for (int i = storage._slotData.Count; i < count; i++)
                    {
                        storage._slotData.Add(new StorageSlot<IPickupable>(i));
                        storage._temp.Add(null);
                    }
                }
                //else if (storage._slotCount > count)
                //{
                //    // Decreasing count... Don't really want to destroy the items. Do nothing?
                //}
            }

            storage._slotCount = count;
        }

        // Patch for original loot quantity

        [HarmonyPatch(typeof(Interactive_STORAGE), nameof(Interactive_STORAGE.OnSlotStorageOpen))]
        public class Interactive_Storage_OnSlotStorageOpen
        {
            static void Prefix(Interactive_STORAGE __instance, ref object[] __state)
            {
                if (!NormalLootQuantity.Value)
                    return;

                __state = new object[] { __instance._slotStorage.SlotCount };
                // Set it back to the original slotCount before execution
                __instance._slotStorage._slotCount = originalContainerSizes[GetKey(__instance._slotStorage)];
            }

            static void Postfix(Interactive_STORAGE __instance, object[] __state)
            {
                if (!NormalLootQuantity.Value)
                    return;

                // Set it back to the original count after execution
                __instance._slotStorage._slotCount = (int)__state[0];
            }
        }

        #endregion
    }
}
