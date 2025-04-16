using MelonLoader;
using Modules;
using System.Collections.Generic;
using UnityEngine;

public class Core : MelonMod
{
    private readonly List<IModModule> modules = new List<IModModule>();

    public override void OnApplicationStart()
    {
        try
        {
            MelonLogger.Msg("Loading More Things Modules...");

            if (ModuleManager.IsModuleEnabled("BackpackMod"))
                modules.Add(new BackpackMod());

            if (ModuleManager.IsModuleEnabled("StackModule"))
                modules.Add(new stackModule());           

            // Initialize modules
            foreach (var module in modules)
            {
                module.OnModInit();
            }

            MelonLogger.Msg($"Loaded {modules.Count} module(s).");
        }
        catch (System.Exception ex)
        {
            MelonLogger.Error($"Error initializing modules: {ex.Message}");
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        foreach (var module in modules)
        {
            try
            {
                module.OnSceneLoaded(buildIndex, sceneName);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in OnSceneWasLoaded for module {module.GetType().Name}: {ex.Message}");
            }
        }
    }

    public override void OnUpdate()
    {
        foreach (var module in modules)
        {
            try
            {
                module.OnUpdate();
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in OnUpdate for module {module.GetType().Name}: {ex.Message}");
            }
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            ModuleManager.ToggleModule("BackpackMod");
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            ModuleManager.ToggleModule("StackModule");
        }
    }
}