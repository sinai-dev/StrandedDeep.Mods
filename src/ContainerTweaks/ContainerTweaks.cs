using Beam;
using Beam.Crafting;
using Beam.UI;
using Beam.Utilities;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void Initialize()
        {
            Instance = this;

            AllowStoringContainers = Bind("Allow storing containers", false, "Allows containers to be placed inside other containers");
            MaxContainerSizes = Bind("Max container sizes", false, "Makes it so inventory and containers use maximum slots.", SettingChanged_MaxContainers);

            BuildAllContainerOriginalValues();

            base.Initialize();
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            SetAllContainers(MaxContainerSizes.Value);
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            SetAllContainers(MaxContainerSizes.Value);
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

        private void SettingChanged_MaxContainers(SettingChangedEventArgs args)
        {
            SetAllContainers((bool)args.ChangedSetting.BoxedValue);
        }

        private void BuildAllContainerOriginalValues()
        {
            foreach (var container in Resources.FindObjectsOfTypeAll<Interactive_STORAGE>())
            {
                if (originalContainerSizes.ContainsKey(container.PrefabId.ToString()))
                    continue;
                originalContainerSizes.Add(container.PrefabId.ToString(), container._slotStorage._slotCount);
            }

            foreach (var container in Resources.FindObjectsOfTypeAll<Interactive_RAFT_STORAGE>())
            {
                originalContainerSizes.Add("raft", container._slotStorage._slotCount);
                break;
            }
        }

        private void SetAllContainers(bool toMax)
        {
            foreach (var container in Resources.FindObjectsOfTypeAll<Interactive_STORAGE>())
                SetSlotStorage(container._slotStorage, Enabled && toMax);

            foreach (var container in Resources.FindObjectsOfTypeAll<Interactive_RAFT_STORAGE>())
                SetSlotStorage(container._slotStorage, Enabled && toMax);
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

        const string INVENTORY_KEY = "INVENTORY_MENU_BACKPACK_TITLE";

        static void SetSlotStorage(SlotStorage storage, bool toMax)
        {
            string key;
            if (storage.Name == INVENTORY_KEY)
                key = storage.Name;
            else if (storage._storageContainer.parent.GetComponent<Interactive_STORAGE>() is Interactive_STORAGE interactiveStorage)
                key = interactiveStorage.PrefabId.ToString();
            else
                key = "raft";

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

        #endregion
    }
}
