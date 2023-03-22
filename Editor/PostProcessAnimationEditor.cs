// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VirtualBeings.Tech.BehaviorComposition;
using System.Net.NetworkInformation;

namespace VirtualBeings.Tech.UnityIntegration
{
    public abstract class PostProcessAnimationEditor : Editor
    {
        public const int BEING_EXECUTION_ORDER = 100;

        public abstract AgentType AgentType { get; }

        ////////////////////////////////////////////////////////////////////////////////
        public static Dictionary<string, Transform> GetTransformDictionary(GameObject go)
        {
            Transform[] transforms = go.GetComponentsInChildren<Transform>();
            Dictionary<string, Transform> dict = new();

            foreach (Transform t in transforms)
                dict[t.gameObject.name] = t;

            return dict;
        }

        GUIStyle configuredStyle    = new GUIStyle();
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

        protected virtual void ResetSpecific() { }
        protected virtual void CreateBeingSpecific()
        {
            PostProcessAnimation postProcessAnimation = (PostProcessAnimation)target;
            Being being = postProcessAnimation.GetComponent<Being>();

            SetExecutionOrder(being, BEING_EXECUTION_ORDER);
        }

        protected virtual void CreateColliderSpecific() { }
        protected virtual void CreateDriversSpecific() { }

        public override void OnInspectorGUI()
        {
            PostProcessAnimation postProcessAnimation = (PostProcessAnimation)target;
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            if (postProcessAnimation.IsBeingReady())
            {
                EditorGUILayout.LabelField("Being is configured.", configuredStyle);
            }
            else
            {
                EditorGUILayout.LabelField("Being is not configured.", notConfiguredStyle);
            }

            EditorGUILayout.Space(10);

            GUI.enabled = postProcessAnimation.BeingStepDone || postProcessAnimation.ColliderStepDone || postProcessAnimation.DriverStepDone;
            if ((postProcessAnimation.BeingStepDone || postProcessAnimation.ColliderStepDone || postProcessAnimation.DriverStepDone)
                && GUILayout.Button("Reset", GUILayout.Width(255)))
            {
                EditorUtility.SetDirty(target);
                ResetSpecific(); // call before so that you can remove stuff added after the creation (so removed first)
                postProcessAnimation.ResetPostProcessAnimation();
            }

            GUI.enabled = !postProcessAnimation.BeingStepDone && !postProcessAnimation.ColliderStepDone && !postProcessAnimation.DriverStepDone;
            if (GUILayout.Button("1 - Create Being", GUILayout.Width(255)))
            {
                EditorUtility.SetDirty(target);
                postProcessAnimation.CreateBeing();
                CreateBeingSpecific();
            }

            GUI.enabled = postProcessAnimation.BeingStepDone && !postProcessAnimation.ColliderStepDone && !postProcessAnimation.DriverStepDone;
            if (GUILayout.Button("2 - Create Colliders", GUILayout.Width(255)))
            {
                EditorUtility.SetDirty(target);

                PhysicMaterial zeroFrictionMaterial = null;
                string[] zeroFrictionMaterialGuid = AssetDatabase.FindAssets("ZeroFriction t:physicMaterial", new string[] { "Packages" });
                if (zeroFrictionMaterialGuid.Length <= 0)
                {
                    Debug.LogError("Could not found ZeroFriction Material in packages.");
                }
                else
                {
                    zeroFrictionMaterial = (PhysicMaterial)AssetDatabase.LoadAssetAtPath(
                        AssetDatabase.GUIDToAssetPath(zeroFrictionMaterialGuid[0]), typeof(PhysicMaterial));
                }

                postProcessAnimation.CreateColliders(zeroFrictionMaterial);
                CreateColliderSpecific();
            }

            GUI.enabled = postProcessAnimation.BeingStepDone && postProcessAnimation.ColliderStepDone && !postProcessAnimation.DriverStepDone;
            if (GUILayout.Button("3 - Create Drivers", GUILayout.Width(255)))
            {
                EditorUtility.SetDirty(target);
                postProcessAnimation.CreateDrivers();
                CreateDriversSpecific();
            }

            GUI.enabled = true;
        }
    }
}
