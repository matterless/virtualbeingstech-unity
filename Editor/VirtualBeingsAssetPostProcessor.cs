// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.IO;
using UnityEditor;
using UnityEngine;
using VirtualBeings.UnityIntegration;

namespace VirtualBeings.Tech.Shared
{
    class VirtualBeingsAssetPostprocessor : AssetPostprocessor
    {
        private const string SETTINGS_SAVE_FOLDER = "Virtual Beings/Settings";
        private const string SETTINGS_ASSET_NAME = "VirtualBeingsSettings.asset";

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            string[] settingsGuid = AssetDatabase.FindAssets("Virtual Beings Settings t:scriptableobject", new string[] { "Assets" });
            if (settingsGuid == null || settingsGuid.Length <= 0)
            {
                CreateVirtualBeingsSettings();
            }
            if (settingsGuid != null && settingsGuid.Length > 1)
            {
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
        }

        public static void ValidateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
