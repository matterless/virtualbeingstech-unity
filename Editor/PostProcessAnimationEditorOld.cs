// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VirtualBeings.Tech.UnityIntegration;

namespace VirtualBeings.Tech.Shared
{
#if UNITY_EDITOR
    //[CustomEditor(typeof(PostProcessAnimation))]
    public partial class PostProcessAnimationEditorOld : Editor
    {
        ////////////////////////////////////////////////////////////////////////////////
        public Dictionary<string, Transform> GetTransformDictionary(GameObject go)
        {
            Transform[] transforms = go.GetComponentsInChildren<Transform>();
            Dictionary<string, Transform> dict = new Dictionary<string, Transform>();

            foreach (Transform t in transforms)
                dict[t.gameObject.name] = t;

            return dict;
        }

        public override void OnInspectorGUI()
        {
            PostProcessAnimation postProcessAnimation = (PostProcessAnimation)target;
            DrawDefaultInspector();

            AgentType agentType;

            if (postProcessAnimation.gameObject.name.Contains("Cat"))
                agentType = AgentType.SmallQuadruped;
            else if (postProcessAnimation.gameObject.name.Contains("Bird"))
                agentType = AgentType.Bird;
            else
                throw new System.Exception("Only cats and bird are supported ATM");

            //PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(postProcessAnimation.gameObject);

            if (true) // prefabAssetType == PrefabAssetType.Model)
            {
                ////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////
                if (GUILayout.Button("Create 'Being'", GUILayout.Width(255)))
                {
                    OnInspectorGUI_CreateBeing(postProcessAnimation, agentType);
                    EditorUtility.SetDirty(target);
                }

                ////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////
                if (GUILayout.Button("Add colliders", GUILayout.Width(255)))
                {
                    OnInspectorGUI_Colliders(postProcessAnimation, agentType);
                    EditorUtility.SetDirty(target);
                }

                ////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////
                if (GUILayout.Button("Populate postProcessAnimation state (list of driver bones, being etc)", GUILayout.Width(255)))
                {
                    OnInspectorGUI_Bones(postProcessAnimation, agentType);
                    EditorUtility.SetDirty(target);
                }
            }
        }
    }
#endif
}
