// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using UnityEngine;
using VirtualBeings.UnityIntegration;
using static VirtualBeings.UnityIntegration.VirtualBeingsSettings;

namespace VirtualBeings.Tech.Shared
{
    [CustomEditor(typeof(VirtualBeingsSettings), true)]
    public class VirtualBeingsSettingsEditor : Editor
    {
        GUIStyle configuredStyle = new GUIStyle();
        GUIStyle notConfiguredStyle = new GUIStyle();

        private void Awake()
        {
            configuredStyle.normal.textColor = new Color(0.35f, 0.6f, 0.38f);
            notConfiguredStyle.normal.textColor = new Color(0.95f, 0.55f, 0.05f);
        }

        public override void OnInspectorGUI()
        {
            VirtualBeingsSettings settings = (VirtualBeingsSettings)target;
            SubscriptionData subscriptionData = settings.GetSubscriptionData();
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            EditorGUILayout.BeginVertical();
            if (settings.IsKeyValid())
            {
                EditorGUILayout.LabelField("Key is valid.", configuredStyle);
                EditorGUILayout.LabelField($"You have access to :");
                EditorGUILayout.LabelField($"{subscriptionData.MainCharacter} main character(s)");
                EditorGUILayout.LabelField($"{subscriptionData.SideCharacter} side character(s)");
                EditorGUILayout.LabelField($"{subscriptionData.BackgroundCharacter} background character(s)");
            }
            else
            {
                EditorGUILayout.LabelField("Key is not valid.", notConfiguredStyle);
            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

