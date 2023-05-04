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
    /// <summary>
    /// Unity handling for post asset processing callback. Checks existence of settings assets every time assets change.
    /// </summary>
    class VirtualBeingsAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            VirtualBeingsConfig.EnsureVirtualBeingsSettingsExists();
        }
    }

    [InitializeOnLoad]
    public static class VirtualBeingsConfig
    {
        private const string SETTINGS_SAVE_FOLDER = "VIRTUAL BEINGS/Resources";
        private const string SETTINGS_ASSET_NAME = "VIRTUAL BEINGS - Settings.asset";


        // Constructor runs on project load, allows for startup check
        static VirtualBeingsConfig()
        {
            EnsureAssetExists();
        }

        /// <summary>
        /// Attempts enforce existence of singleton. If Editor is not ready, this method will be deferred one editor update and try again until it succeeds.
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void EnsureAssetExists()
        {
            EnsureVirtualBeingsSettingsExists();
        }

        internal static void EnsureVirtualBeingsSettingsExists()
        {
            VirtualBeingsSettings virtualBeingsSettings = VirtualBeingsSettings.Instance;

            if (virtualBeingsSettings)
                return;

            // Keep deferring this check until Unity is ready to deal with asset find/create.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += EnsureVirtualBeingsSettingsExists;
                return;
            }
            EditorApplication.delayCall += () => GetOrCreateVirtualBeingsSettingsAsset();
        }

        /// <summary>
        /// Gets the <see cref="PhotonAppSettings"/> singleton. If none was found, attempts to create one.
        /// </summary>
        /// <returns></returns>
        public static VirtualBeingsSettings GetOrCreateVirtualBeingsSettingsAsset()
        {
            VirtualBeingsSettings virtualBeingSettings;

            virtualBeingSettings = VirtualBeingsSettings.Instance;

            if (virtualBeingSettings != null)
                return virtualBeingSettings;

            // If trying to get instance returned null - create a new asset.
            Debug.Log($"{nameof(VirtualBeingsSettings)} not found. Creating a new one at {SETTINGS_SAVE_FOLDER}/{SETTINGS_ASSET_NAME}/");

            virtualBeingSettings = ScriptableObject.CreateInstance<VirtualBeingsSettings>();
            string folder = $"{Application.dataPath}/{SETTINGS_SAVE_FOLDER}";
            ValidateDirectory(folder);
            AssetDatabase.CreateAsset(virtualBeingSettings, $"Assets/{SETTINGS_SAVE_FOLDER}/{SETTINGS_ASSET_NAME}");
            VirtualBeingsSettings.Instance = virtualBeingSettings;
            EditorUtility.SetDirty(virtualBeingSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // If key null, open windows

            return VirtualBeingsSettings.Instance;
        }

        private static void ValidateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
