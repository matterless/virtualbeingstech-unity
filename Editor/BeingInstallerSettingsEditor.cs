// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using UnityEngine;
using VirtualBeings.Installer;

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

            BeingInstallerSettings settings = (BeingInstallerSettings)target;

            if (settings.VirtualBeingsSettings == null)
            {
                EditorGUILayout.LabelField("Virtual Being Settings not found.", errorStyle);
            }

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }
    }
}

