using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ScheduleOne.Storage;
using ScheduleOne.ItemFramework;
using MelonLoader;

namespace Modules
{
    public class BackpackMod : IModModule
    {
        private const int ROWS = 4;
        private const int COLUMNS = 3;
        private const KeyCode TOGGLE_KEY = KeyCode.B;
        private StorageEntity backpackEntity;
        private bool isInitialized = false;

        public void OnModInit()
        {
            MelonLogger.Msg("[BackpackMod] Initialized");
        }

        public void OnSceneLoaded(int buildIndex, string sceneName)
        {
            if (sceneName != "Main")
            {
                if (backpackEntity != null)
                    backpackEntity.gameObject.SetActive(false);

                MelonLogger.Msg("[BackpackMod] Scene not gameplay, disabling.");
                return;
            }

            MelonCoroutines.Start(SetupBackpack());
        }

        private IEnumerator SetupBackpack()
        {
            yield return new WaitForSeconds(1f);

            try
            {
                var templates = UnityEngine.Object.FindObjectsOfType<StorageEntity>();
                if (templates == null || templates.Length == 0)
                {
                    MelonLogger.Error("[BackpackMod] No StorageEntity template found.");
                    yield break;
                }

                backpackEntity = UnityEngine.Object.Instantiate(templates[0]);
                backpackEntity.name = "BackpackStorage";
                backpackEntity.StorageEntityName = "Backpack";
                backpackEntity.SlotCount = ROWS * COLUMNS;
                backpackEntity.DisplayRowCount = COLUMNS;
                backpackEntity.MaxAccessDistance = 999f;
                backpackEntity.AccessSettings = StorageEntity.EAccessSettings.Full;

                var slots = new List<ItemSlot>();
                for (int i = 0; i < ROWS * COLUMNS; i++)
                    slots.Add(new ItemSlot());

                backpackEntity.ItemSlots = slots;

                isInitialized = true;
                MelonLogger.Msg("[BackpackMod] Setup complete.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[BackpackMod] Error during setup: " + ex);
            }
        }

        public void OnUpdate()
        {
            if (SceneManager.GetActiveScene().name != "Main" || !isInitialized)
                return;

            if (Input.GetKeyDown(TOGGLE_KEY))
            {
                try
                {
                    if (backpackEntity.IsOpened)
                        backpackEntity.Close();
                    else
                        backpackEntity.Open();
                }
                catch (Exception ex)
                {
                    MelonLogger.Error("[BackpackMod] Toggle error: " + ex);
                }
            }
        }
    }
}
