using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;

namespace Modules
{
    public static class ModuleManager
    {
        private static readonly string ConfigPath = Path.Combine(MelonEnvironment.UserDataDirectory, "ModulesConfig.json");
        private static Dictionary<string, bool> moduleStates = new Dictionary<string, bool>();

        static ModuleManager()
        {
            LoadConfig();
        }

        public static bool IsModuleEnabled(string moduleName)
        {
            if (moduleStates.TryGetValue(moduleName, out bool isEnabled))
                return isEnabled;

            // Default to enabled if not configured
            moduleStates[moduleName] = true;
            SaveConfig();
            return true;
        }

        public static void ToggleModule(string moduleName)
        {
            if (moduleStates.ContainsKey(moduleName))
            {
                moduleStates[moduleName] = !moduleStates[moduleName];
            }
            else
            {
                moduleStates[moduleName] = false; // Disable if not already configured
            }

            MelonLogger.Msg($"[ModuleManager] Module '{moduleName}' is now {(moduleStates[moduleName] ? "enabled" : "disabled")}.");
            SaveConfig();
        }

        private static void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                MelonLogger.Warning("[ModuleManager] Config file not found. Using default states.");
                return;
            }

            try
            {
                string json = File.ReadAllText(ConfigPath);
                moduleStates = JsonConvert.DeserializeObject<Dictionary<string, bool>>(json) ?? new Dictionary<string, bool>();
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[ModuleManager] Failed to load config: " + ex.Message);
            }
        }

        private static void SaveConfig()
        {
            try
            {
                string json = JsonConvert.SerializeObject(moduleStates, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[ModuleManager] Failed to save config: " + ex.Message);
            }
        }
    }
}