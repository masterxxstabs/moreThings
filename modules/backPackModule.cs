using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using ScheduleOne.Storage;
using ScheduleOne.ItemFramework;
using MelonLoader;
using ScheduleOne.Persistence.Datas;
using MelonLoader.Utils;
using ScheduleOne.Persistence;
using HarmonyLib;

namespace Modules
{
    public class BackpackMod : IModModule
    {
        private const int ROWS = 4;
        private const int COLUMNS = 3;
        private const KeyCode TOGGLE_KEY = KeyCode.B;
        public static StorageEntity backpackEntity;
        private bool isInitialized = false;

        public static string SavePath => Path.Combine(MelonEnvironment.UserDataDirectory, "BackpackMod.json");

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

                LoadBackpack();

                isInitialized = true;
                MelonLogger.Msg("[BackpackMod] Setup complete.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[BackpackMod] Error during setup: " + ex);
            }
        }

        private const KeyCode saveKey = KeyCode.F5;
        private const KeyCode loadKey = KeyCode.F6;

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

            if (Input.GetKeyDown(saveKey))
            {
                SaveBackpack();
            }

            if (Input.GetKeyDown(loadKey))
            {
                LoadBackpack();
            }
        }

        [HarmonyPatch(typeof(SaveManager), "Save", new System.Type[] { })]
        public class SaveManagerPatch
        {
            [HarmonyPostfix]
            private static void SavePostfix()
            {
                try
                {
                    if (BackpackMod.backpackEntity == null)
                    {
                        MelonLogger.Warning("[BackpackMod] Backpack entity is null during save. skipping save");
                    }

                    var itemSet = new ItemSet(BackpackMod.backpackEntity.ItemSlots);
                    File.WriteAllText(BackpackMod.SavePath, itemSet.GetJSON());
                    MelonLogger.Msg("[BackpackMod] Backpack saved.");
                }
                catch (Exception ex)
                {
                    MelonLogger.Error("[BackpackMod] Save error: " + ex);
                }
            }
        }

        private void SaveBackpack()
        {
            try
            {
                var itemSet = new ItemSet(backpackEntity.ItemSlots);
                File.WriteAllText(SavePath, itemSet.GetJSON());
                MelonLogger.Msg("[BackpackMod] Backpack saved.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[BackpackMod] Save error: " + ex);
            }
        }

        private void LoadBackpack()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    MelonLogger.Warning("[BackpackMod] No save file found.");
                    return;
                }

                string json = File.ReadAllText(SavePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    MelonLogger.Error("[BackpackMod] Save file is empty or invalid.");
                    return;
                }

                MelonLogger.Msg("[BackpackMod] Deserializing JSON: " + json);

                var items = ItemSet.Deserialize(json);
                if (items == null)
                {
                    MelonLogger.Error("[BackpackMod] Deserialization returned null.");
                    return;
                }

                if (backpackEntity?.ItemSlots == null)
                {
                    MelonLogger.Error("[BackpackMod] Backpack entity or item slots not initialized properly.");
                    return;
                }

                for (int i = 0; i < items.Length; i++)
                {
                    if (i >= backpackEntity.ItemSlots.Count)
                        break;

                    var slot = backpackEntity.ItemSlots[i];
                    if (slot == null)
                    {
                        MelonLogger.Warning($"[BackpackMod] Item slot {i} is null.");
                        continue;
                    }

                    slot.SetStoredItem(items[i], false);
                }

                MelonLogger.Msg("[BackpackMod] Backpack loaded successfully.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[BackpackMod] Load error: " + ex.Message);
            }
        }
    }
}