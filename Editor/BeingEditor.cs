// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VirtualBeings.Tech.BehaviorComposition;
using System.Reflection;
using System;
using UnityEditor.SceneManagement;

namespace VirtualBeings.Tech.UnityIntegration
{
    public abstract class BeingEditor : Editor
    {
        public const int BEING_EXECUTION_ORDER = 100;

        public static Dictionary<string, Transform> GetTransformDictionary(GameObject go)
        {
            Transform[] transforms = go.GetComponentsInChildren<Transform>();
            Dictionary<string, Transform> dict = new();

            foreach (Transform t in transforms)
                dict[t.gameObject.name] = t;

            return dict;
        }

        GUIStyle configuredStyle = new GUIStyle();
        GUIStyle notConfiguredStyle = new GUIStyle();

        /// <summary>
        /// From https://forum.unity.com/threads/script-execution-order-manipulation.130805/#post-1323087
        /// </summary>
        protected void SetExecutionOrder(MonoBehaviour mb, int order)
        {
            // First you get the MonoScript of your MonoBehaviour
            MonoScript monoScript = MonoScript.FromMonoBehaviour(mb);

            // Getting the current execution order of that MonoScript
            // int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);

            // Changing the MonoScript's execution order
            if (order != MonoImporter.GetExecutionOrder(monoScript))
                MonoImporter.SetExecutionOrder(monoScript, order);
        }


        private void OnEnable()
        {
            configuredStyle.normal.textColor = new Color(0.35f, 0.6f, 0.38f);
            notConfiguredStyle.normal.textColor = new Color(0.95f, 0.55f, 0.05f);
        }

        protected abstract PostProcessAnimation CreatePostProcessAnimationSpecific();

        protected virtual void ResetSpecific() { }
        protected virtual void CreatePostProcessAnimation(Being being)
        {
            PostProcessAnimation postProcessAnimation = CreatePostProcessAnimationSpecific();

            typeof(Being).GetField("_postProcessAnimation", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(being, postProcessAnimation);

            SetExecutionOrder(being, BEING_EXECUTION_ORDER);
        }

        protected virtual void CreateColliderSpecific() { }
        protected virtual void CreateDriversSpecific() { }

        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, "m_Script");
            EditorGUILayout.Space(10);

            Being being = (Being)target;
            bool isBeingConfigured = being.PostProcessAnimation != null && being.PostProcessAnimation.IsBeingReady();

            if (!isBeingConfigured)
            {
                EditorGUILayout.LabelField("Being is not configured.", notConfiguredStyle);
                EditorGUILayout.Space(10);

                if (GUILayout.Button("Configure Being", GUILayout.Width(255)))
                {
                    string assetPath = AssetDatabase.GetAssetPath(being.TopLevelParent);
                    if (assetPath == null || assetPath == String.Empty)
                    {
                        assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(being.TopLevelParent);
                    }

                    // If the prefab file was not found
                    if (assetPath == null || assetPath == String.Empty)
                    {
                        Debug.LogWarning("You need to create a prefab of your being if you want to configure a being from the editor.");
                    }
                    // We have already opened the being prefab
                    else if (PrefabStageUtility.GetCurrentPrefabStage()?.prefabContentsRoot == being.TopLevelParent)
                    {
                        ConfigureBeing(being);
                    }
                    // We have not opened the being prefab. Create a temporary context to manipulate the prefab
                    else
                    {
                        using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
                        {
                            GameObject beingPrefab = editingScope.prefabContentsRoot;
                            Being beingScope = beingPrefab.GetComponent<Being>();

                            if(beingScope == null)
                            {
                                Debug.LogWarning("No Being script found on the prefab. Did you apply the change on the prefab after adding the Being component ?");
                                return;
                            }
                            ConfigureBeing(beingScope);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("Being is configured.", configuredStyle);
                EditorGUILayout.Space(10);

                if (isBeingConfigured && GUILayout.Button("Reset", GUILayout.Width(255)))
                {
                    string assetPath = AssetDatabase.GetAssetPath(being.TopLevelParent);
                    if (assetPath == null || assetPath == String.Empty)
                    {
                        assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(being.TopLevelParent);
                    }

                    // If the prefab file was not found
                    if (assetPath == null || assetPath == String.Empty)
                    {
                        Debug.LogWarning("You need to create a prefab of your being if you want to configure a being from the editor.");
                    }
                    // We have already opened the being prefab
                    else if (PrefabStageUtility.GetCurrentPrefabStage()?.prefabContentsRoot == being.TopLevelParent)
                    {
                        ResetBeing(being);
                    }
                    // We have not opened the being prefab. Create a temporary context to manipulate the prefab
                    else
                    {
                        using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
                        {
                            GameObject floaterPrefab = editingScope.prefabContentsRoot;
                            Being beingScope = floaterPrefab.GetComponent<Being>();
                            ResetBeing(beingScope);
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void ConfigureBeing(Being being)
        {
            // 1 - Create Being
            EditorUtility.SetDirty(target);
            CreatePostProcessAnimation(being);
            being.PostProcessAnimation.CreateBeing(being);

            // 2 - Create Colliders
            PhysicMaterial zeroFrictionMaterial = null;
            string[] zeroFrictionMaterialGuid = AssetDatabase.FindAssets("ZeroFriction t:physicMaterial", new string[] { "Packages" });
            if (zeroFrictionMaterialGuid.Length <= 0)
            {
                Misc.LogError("Could not found ZeroFriction Material in VirtualBeingTech packages.");
            }
            else
            {
                zeroFrictionMaterial = (PhysicMaterial)AssetDatabase.LoadAssetAtPath(
                    AssetDatabase.GUIDToAssetPath(zeroFrictionMaterialGuid[0]), typeof(PhysicMaterial));
            }
            being.PostProcessAnimation.CreateColliders(zeroFrictionMaterial);
            CreateColliderSpecific();

            // 3 - Create Drivers
            being.PostProcessAnimation.CreateDrivers();
            CreateDriversSpecific();
        }

        private void ResetBeing(Being being)
        {
            EditorUtility.SetDirty(target);
            ResetSpecific(); // call before so that you can remove stuff added after the creation (so removed first)
            being.PostProcessAnimation.ResetPostProcessAnimation();
        }
    }
}
