// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
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


            //RootActivityFactory[] rootActivities = (RootActivityFactory[])typeof(BeingSettings)
            //    .GetField("RootActivities", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
            //    .GetValue(beingSettings);

            List<MotiveSettings> tempMotives = new(motives);

            // 1 - Remove unused Motives
            // if there is a motive inside birdMotives that do no exist in RAF we remove it.
            for (int i = 0; i < motives.Length; ++i)
            {
                MotiveSettings motiveSettings = motives[i];
                if (motiveSettings == null ||
                    motiveSettings.RootActivityFactory == null ||
                    !beingSettings.RootActivities.Any(raf => raf.Equals(motiveSettings.RootActivityFactory)))
                {
                    tempMotives.Remove(motiveSettings);
                }
            }

            // 2 - Add new Motives
            // If there is a motive inside RAF that do not exist in birdMotives, we add it.
            foreach (RootActivityFactory rootActivity in beingSettings.RootActivities)
            {
                if (rootActivity == null)
                {
                    continue;
                }
                //tempMotives.ForEach(m => Debug.Log("does motive " + rootActivity.MotiveName.Name m.MotiveName));
                RootActivityFactory activity = rootActivity;
                if (!tempMotives.Any(bm => bm.RootActivityFactory.Equals(activity)))
                {
                    MotiveSettings motiveSettings = new(rootActivity, 0.5f);
                    tempMotives.Add(motiveSettings);
                }
            }

            EditorGUILayout.PropertyField(rootActivties);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                tempMotives.Count <= 0 ? "No Motive(s) selected from Root Activities." : "Motives",
                EditorStyles.boldLabel
            );
            EditorGUILayout.Space();

            foreach (MotiveSettings motiveSettings in tempMotives)
            {
                if (motiveSettings.RootActivityFactory == null)
                {
                    continue;
                }

                if (!motiveSettings.RootActivityFactory.HasMotiveAssociated)
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel($"Salience01 {motiveSettings.RootActivityFactory.MotiveName}");
                motiveSettings.Salience01 = EditorGUILayout.Slider(motiveSettings.Salience01, 0, 1f);
                EditorGUILayout.EndHorizontal();
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
