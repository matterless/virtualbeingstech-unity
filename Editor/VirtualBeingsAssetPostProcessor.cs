// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.IO;
using UnityEditor;
using UnityEngine;
using VirtualBeings.Installer;
using VirtualBeings.UnityIntegration;

namespace VirtualBeings.Tech.Shared
{
    class VirtualBeingsAssetPostprocessor : AssetPostprocessor
    {
        private const string SETTINGS_SAVE_FOLDER = "Virtual Beings/Settings";
        private const string SETTINGS_ASSET_NAME = "VirtualBeingsSettings.asset";

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            string[] settingsGuid = AssetDatabase.FindAssets("t:VirtualBeingsSettings", new string[] { "Assets" });
            if (settingsGuid == null || settingsGuid.Length <= 0)
            {
                CreateVirtualBeingsSettings();
            }
            if (settingsGuid != null && settingsGuid.Length > 1)
            {
                // Ensure only one "Virtual Beings Settings" exist by deleting all other files.
                for (int i = 1; i < settingsGuid.Length; ++i)
                {
                    string pathToDelete = AssetDatabase.GUIDToAssetPath(settingsGuid[i]);
                    Debug.LogWarning("Deleting " + pathToDelete + " because there is more than one VirtualBeingsSettings");
                    AssetDatabase.DeleteAsset(pathToDelete);
                }
            }
        }

        private static void CreateVirtualBeingsSettings()
        {
            Debug.Log("Create Virtual Beings Settings at " + $"Assets/{SETTINGS_SAVE_FOLDER}");

            ValidateDirectory($"{Application.dataPath}/{SETTINGS_SAVE_FOLDER}");
            AssetDatabase.Refresh();

            VirtualBeingsSettings vbSettings = ScriptableObject.CreateInstance<VirtualBeingsSettings>();

            AssetDatabase.CreateAsset(vbSettings, $"Assets/{SETTINGS_SAVE_FOLDER}/{SETTINGS_ASSET_NAME}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            FillVirtualSettingsReference(vbSettings);
        }

        public static void ValidateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void FillVirtualSettingsReference(VirtualBeingsSettings vbSettings)
        {
            string[] settingsGUID = AssetDatabase.FindAssets("t:BeingInstallerSettings", new string[] { "Assets" });

            foreach(string settingGUID in settingsGUID)
            {
                string path = AssetDatabase.GUIDToAssetPath(settingGUID);
                Debug.Log("Found file in : " + path);
                BeingInstallerSettings beingInstallerSettings = (BeingInstallerSettings)AssetDatabase.LoadAssetAtPath(path, typeof(BeingInstallerSettings));
                beingInstallerSettings.VirtualBeingsSettings = vbSettings;
            }
        }
    }
}
