// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using VirtualBeings.Tech.ActiveCognition;
using VirtualBeings.Tech.BehaviorComposition;
using static VirtualBeings.Tech.ActiveCognition.BeingSettings;

namespace VirtualBeings.Tech.Shared
{
    [CustomEditor(typeof(BeingSettings), true)]
    public class BeingSettingsEditor : Editor
    {
        BeingSettings beingSettings;
        SerializedProperty rootActivties;

        private void Awake()
        {
            beingSettings = (BeingSettings)target;
        }

        private void OnEnable()
        {
            rootActivties = serializedObject.FindProperty("RootActivities");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "_motives", "RootActivities");

            MotiveSettings[] motives = (MotiveSettings[])typeof(BeingSettings)
                .GetField("_motives", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(beingSettings);

            RootActivityFactory[] rootActivities = (RootActivityFactory[])typeof(BeingSettings)
                .GetField("RootActivities", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .GetValue(beingSettings);

            List<MotiveSettings> tempMotives = new List<MotiveSettings>(motives);

            // 1 - Remove unused Motives
            // if there is a motive inside birdMotives that do no exist in RAF we remove it.
            for (int i = 0; i < motives.Length; ++i)
            {
                MotiveSettings motiveSettings = motives[i];
                if (motiveSettings == null ||
                    motiveSettings.MotiveName == null ||
                    rootActivities.Where(raf => raf.MotiveName != null && raf.MotiveName.Name.Equals(motiveSettings.MotiveName.Name)).Count() == 0)
                {
                    tempMotives.Remove(motiveSettings);
                }
            }

            // 2 - Add new Motives
            // If there is a motive inside RAF that do not exist in birdMotives, we add it.
            for (int i = 0; i < rootActivities.Length; ++i)
            {
                RootActivityFactory rootActivity = rootActivities[i];
                if (rootActivity == null || rootActivity.MotiveName == null)
                {
                    continue;
                }
                //tempMotives.ForEach(m => Debug.Log("does motive " + rootActivity.MotiveName.Name m.MotiveName));
                if (tempMotives.Where(bm => bm.MotiveName.Name.Equals(rootActivity.MotiveName.Name)).Count() == 0)
                {
                    MotiveSettings motiveSettings = new MotiveSettings(rootActivity.MotiveName, 0f);
                    tempMotives.Add(motiveSettings);
                }
            }

            EditorGUILayout.PropertyField(rootActivties);
            EditorGUILayout.Space();
            if (tempMotives.Count <= 0)
            {
                EditorGUILayout.LabelField("No Motive(s) selected from Root Activities.", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Motives", EditorStyles.boldLabel);
            }
            EditorGUILayout.Space();

            foreach (MotiveSettings motiveSettings in tempMotives)
            {
                if (motiveSettings.MotiveName != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel($"Salience01 {motiveSettings.MotiveName.Name}");
                    motiveSettings.Salience01 = EditorGUILayout.Slider(motiveSettings.Salience01, 0, 1f);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            typeof(BeingSettings)
                .GetField("_motives", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(beingSettings, tempMotives.ToArray());

            // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
