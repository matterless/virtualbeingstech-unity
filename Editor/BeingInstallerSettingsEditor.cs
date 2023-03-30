// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VirtualBeings.Installer;
using VirtualBeings.UnityIntegration;

namespace VirtualBeings.Tech.Shared
{
    [CustomEditor(typeof(BeingInstallerSettings), true)]
    public class BeingInstallerSettingsEditor : Editor
    {
        GUIStyle errorStyle = new GUIStyle();

        private void Awake()
        {
            errorStyle.normal.textColor = Color.red;
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EnsureReferenceToVirtualBeingsSettings();

            BeingInstallerSettings settings = (BeingInstallerSettings)target;

            if (settings.VirtualBeingsSettings == null)
            {
                EditorGUILayout.LabelField("Virtual Being Settings not found.", errorStyle);
            }

            DrawPropertiesExcluding(serializedObject, "m_Script");


            serializedObject.ApplyModifiedProperties();
        }

        private void EnsureReferenceToVirtualBeingsSettings()
        {
            BeingInstallerSettings settings = (BeingInstallerSettings)target;

            if (settings.VirtualBeingsSettings == null)
            {
                string[] settingsGUID = AssetDatabase.FindAssets("t:VirtualBeingsSettings", new string[] { "Assets" });

                if (settingsGUID == null || settingsGUID.Length < 1)
                {
                    Debug.LogError("No Virtual Beings Settings found.");
                    return;
                }

                string path = AssetDatabase.GUIDToAssetPath(settingsGUID[0]);
                VirtualBeingsSettings vbSettings = (VirtualBeingsSettings)AssetDatabase.LoadAssetAtPath(path, typeof(VirtualBeingsSettings));
                settings.VirtualBeingsSettings = vbSettings;

                Debug.Log("Fill ref in being installer settings");
            }
        }
    }
}

