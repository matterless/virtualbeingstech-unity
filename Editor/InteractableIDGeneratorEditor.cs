// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VirtualBeings.Tech.UnityIntegration;

namespace VirtualBeings.Tech.Shared
{
#if UNITY_EDITOR
    [CustomEditor(typeof(InteractableIDGenerator))]
    public class InteractableIDGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var generator = (InteractableIDGenerator)target;

            if (GUILayout.Button("Update interactable IDs"))
            {
                List<MonoBehaviour> modifiedBehaviours = generator.GenerateIDs();
                foreach (MonoBehaviour interactable in modifiedBehaviours)
                {
                    EditorUtility.SetDirty(interactable);
                }
            }
        }
    }
#endif
}
