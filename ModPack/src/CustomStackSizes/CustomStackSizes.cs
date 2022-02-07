using Beam;
using Beam.Crafting;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModPack.CustomStackSizes
{
    public class CustomStackSizes : SubPlugin
    {
        public override string Name => "Custom Stack Sizes";

        public static ConfigEntry<int> DefaultStackSize;
        public static ConfigEntry<int> ContainerStackSize;
        public static ConfigEntry<int> LeavesStackSize;
        public static ConfigEntry<int> LogStackSize;
        public static ConfigEntry<int> ArrowStackSize;
        public static ConfigEntry<int> SpeargunAmmoStackSize;
        public static ConfigEntry<int> FishStackSize;
        public static ConfigEntry<int> MeatStackSize;
        public static ConfigEntry<int> FruitStackSize;

        private static Dictionary<InteractiveType, int> originalStackSizes;

        public override void Initialize()
        {
            originalStackSizes = new();
            foreach (var entry in SlotStorage.STACK_SIZES)
                originalStackSizes.Add(entry.Key, entry.Value);

            DefaultStackSize = Bind("Default stack size", 4, "The default size for all stacks not explicitly listed", SettingChanged);
            ContainerStackSize = Bind("Containers", 1, "Stack size of Containers", SettingChanged);
            LeavesStackSize = Bind("Leaves", 24, "Stack size of leaves", SettingChanged);
            LogStackSize = Bind("Logs", 1, "Stack size of logs", SettingChanged);
            ArrowStackSize = Bind("Bow arrows", 24, "Stack size of bow arrows", SettingChanged);
            SpeargunAmmoStackSize = Bind("Speargun ammo", 24, "Stack size of Speargun ammo", SettingChanged);
            FishStackSize = Bind("Fish", 8, "Stack size of fish", SettingChanged);
            MeatStackSize = Bind("Meat", 8, "Stack size of (non-fish) meat", SettingChanged);
            FruitStackSize = Bind("Fruit", 8, "Stack size of fruit", SettingChanged);

            base.Initialize();
        }

        private void SettingChanged(SettingChangedEventArgs args)
        {
            if (Enabled)
                SetCustomStackSizes();
        }

        public override void OnEnabled()
        {
            base.OnEnabled();
            SetCustomStackSizes();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            foreach (var entry in originalStackSizes)
                SlotStorage.STACK_SIZES[entry.Key] = entry.Value;
        }

        private static void SetCustomStackSizes()
        {
            SlotStorage.STACK_SIZES[InteractiveType.CONTAINER] = ContainerStackSize.Value;
            SlotStorage.STACK_SIZES[InteractiveType.CRAFTING_LEAVES] = LeavesStackSize.Value;
            SlotStorage.STACK_SIZES[InteractiveType.CRAFTING_LOG] = LogStackSize.Value;
            SlotStorage.STACK_SIZES[InteractiveType.TOOLS_ARROW] = ArrowStackSize.Value;
            SlotStorage.STACK_SIZES[InteractiveType.TOOLS_SPEARGUN_ARROW] = SpeargunAmmoStackSize.Value;
            SlotStorage.STACK_SIZES[InteractiveType.ANIMALS_FISH] = FishStackSize.Value;
            SlotStorage.STACK_SIZES[InteractiveType.FOOD_MEAT] = MeatStackSize.Value;
            SlotStorage.STACK_SIZES[InteractiveType.FOOD_FRUIT] = FruitStackSize.Value;
        }

        [HarmonyPatch(typeof(SlotStorage), nameof(SlotStorage.GetStackSize))]
        public class GetStackSize
        {
            static void Postfix(ref int __result, CraftingType type)
            {
                if (!SlotStorage.STACK_SIZES.TryGetValue(type.InteractiveType, out int value))
                    value = DefaultStackSize.Value;

                __result = value;
            }
        }
    }
}
