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

        /// <summary>
        /// Called each time an assets has been created / deleted / moved, and when a domain was reloaded.
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
        /// <param name="didDomainReload"></param>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            // We want to ensure that only one VirtualBeingsSettings instance exist in the project.
            // Then, we want to fill all of the BeingInstallerSettings the ref to the unique VirtualBeingsSettings.

            VirtualBeingsSettings virtualBeingsSettings = null;

            string[] settingsGuid = AssetDatabase.FindAssets("t:VirtualBeingsSettings", new string[] { "Assets" });

            // If no instance of VirtualBeingsSettings is found, create one.
            if (settingsGuid == null || settingsGuid.Length <= 0)
            {
                virtualBeingsSettings = CreateVirtualBeingsSettings();
            }
            else if (settingsGuid != null &&settingsGuid.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(settingsGuid[0]);
                virtualBeingsSettings = (VirtualBeingsSettings)AssetDatabase.LoadAssetAtPath(path, typeof(VirtualBeingsSettings));
            }

            // If more that one instance of VirtualBeingsSettings is found, delete all except the first
            if (settingsGuid != null && settingsGuid.Length > 1)
            {
                for (int i = 1; i < settingsGuid.Length; ++i)
                {
                    string pathToDelete = AssetDatabase.GUIDToAssetPath(settingsGuid[i]);
                    Debug.LogWarning("Deleting " + pathToDelete + " because there is more than one VirtualBeingsSettings");
                    AssetDatabase.DeleteAsset(pathToDelete);
                }
            }

            FillVirtualSettingsReference(virtualBeingsSettings);
        }

        private static VirtualBeingsSettings CreateVirtualBeingsSettings()
        {
            Debug.Log("Create Virtual Beings Settings at " + $"Assets/{SETTINGS_SAVE_FOLDER}");

            ValidateDirectory($"{Application.dataPath}/{SETTINGS_SAVE_FOLDER}");
            AssetDatabase.Refresh();

            VirtualBeingsSettings vbSettings = ScriptableObject.CreateInstance<VirtualBeingsSettings>();

            AssetDatabase.CreateAsset(vbSettings, $"Assets/{SETTINGS_SAVE_FOLDER}/{SETTINGS_ASSET_NAME}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return vbSettings;
        }

        private static void ValidateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Find all instance of BeingInstallerSettings in the project, and fill their VirtualBeingsSettings field with the one given.
        /// </summary>
        /// <param name="vbSettings"></param>
        private static void FillVirtualSettingsReference(VirtualBeingsSettings vbSettings)
        {
            string[] settingsGUID = AssetDatabase.FindAssets("t:BeingInstallerSettings", new string[] { "Assets" });

            foreach(string settingGUID in settingsGUID)
            {
                string path = AssetDatabase.GUIDToAssetPath(settingGUID);
                BeingInstallerSettings beingInstallerSettings = (BeingInstallerSettings)AssetDatabase.LoadAssetAtPath(path, typeof(BeingInstallerSettings));

                if(beingInstallerSettings.VirtualBeingsSettings == null)
                {
                    beingInstallerSettings.VirtualBeingsSettings = vbSettings;
                    Debug.Log("Fill VirtualBeingsSettings reference in " + beingInstallerSettings);
                }
            }
        }
    }
}
