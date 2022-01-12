using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModPack
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class StrandedDeepModPack : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.stranded-deep.modpack";
        const string NAME = "Sinai's Mod Pack";
        const string VERSION = "1.0.0";

        const string TOGGLES_CATEGORY = "All Feature Toggles";

        public static StrandedDeepModPack Instance { get; private set; }
        public static ManualLogSource Logging => Instance.Logger;

        internal static List<SubPlugin> SubPlugins { get; private set; } = new List<SubPlugin>();

        internal void Awake()
        {
            Instance = this;

            foreach (var type in this.GetType().Assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(SubPlugin)))
                {
                    try
                    {
                        // Create an instance
                        var plugin = (SubPlugin)Activator.CreateInstance(type);
                        SubPlugins.Add(plugin);

                        // Bind the master toggle
                        var toggle = Config.Bind(
                            new ConfigDefinition(TOGGLES_CATEGORY, plugin.Name),
                            false,
                            new ConfigDescription($"Enable '{plugin.Name}'"));

                        toggle.SettingChanged += plugin.Toggle_SettingChanged;
                        plugin.ToggleSetting = toggle;

                        plugin.Initialize();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning($"Exception initializing {type.Name}!");
                        Logger.LogMessage(ex);
                    }
                }
            }
        }

        internal void Update()
        {
            foreach (var plugin in SubPlugins)
            {
                if (plugin.Enabled)
                    plugin.Update();
            }
        }

        internal void FixedUpdate()
        {
            foreach (var plugin in SubPlugins)
            {
                if (plugin.Enabled)
                    plugin.FixedUpdate();
            }
        }

        internal void OnGUI()
        {
            foreach (var plugin in SubPlugins)
            {
                if (plugin.Enabled)
                    plugin.OnGUI();
            }
        }
    }
}
