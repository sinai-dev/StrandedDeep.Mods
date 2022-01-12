using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModPack
{
    public abstract class SubPlugin
    {
        public abstract string Name { get; }

        public ConfigEntry<bool> ToggleSetting;
        public bool Enabled => ToggleSetting.Value;

        protected Harmony Harmony;

        public virtual void Initialize()
        {
            Harmony = new Harmony($"{StrandedDeepModPack.GUID}.{this.GetType().Name}");

            if (ToggleSetting.Value)
                OnEnabled();
        }

        public virtual void OnEnabled()
        {
            // patch main class
            Harmony.PatchAll(this.GetType());
            // patch nested types
            foreach (var type in this.GetType().GetNestedTypes())
                Harmony.PatchAll(type);

            Log($"Enabled {Name}, patched methods: {Harmony.GetPatchedMethods().Count()}");
        }

        public virtual void OnDisabled()
        {
            Harmony.UnpatchSelf();

            Log($"Disabled {Name}, patched methods: {Harmony.GetPatchedMethods().Count()}");
        }

        public virtual void Update() { }

        public virtual void FixedUpdate() { }

        public virtual void OnGUI() { }

        public void Toggle_SettingChanged(object sender, EventArgs e)
        {
            if ((bool)(e as SettingChangedEventArgs).ChangedSetting.BoxedValue)
                OnEnabled();
            else
                OnDisabled();
        }

        protected ConfigEntry<T> Bind<T>(string name, 
            T defaultValue, 
            string description, 
            Action<SettingChangedEventArgs> onSettingChanged = null)
        {
            return Bind(name, defaultValue, new ConfigDescription(description), onSettingChanged);
        }

        protected ConfigEntry<T> Bind<T>(string name, 
            T defaultValue, 
            ConfigDescription description, 
            Action<SettingChangedEventArgs> onSettingChanged = null)
        {
            var config = StrandedDeepModPack.Instance.Config.Bind(new ConfigDefinition(this.Name, name), defaultValue, description);

            if (onSettingChanged != null)
            {
                config.SettingChanged += (obj, arg) =>
                {
                    onSettingChanged(arg as SettingChangedEventArgs);
                };
            }

            return config;
        }

        protected Coroutine StartCoroutine(IEnumerator coroutine)
        {
            return StrandedDeepModPack.Instance.StartCoroutine(coroutine);
        }

        public void Log(object o) => StrandedDeepModPack.Logging.LogMessage(o?.ToString());
        public void LogWarning(object o) => StrandedDeepModPack.Logging.LogWarning(o?.ToString());
        public void LogError(object o) => StrandedDeepModPack.Logging.LogError(o?.ToString());
    }
}
