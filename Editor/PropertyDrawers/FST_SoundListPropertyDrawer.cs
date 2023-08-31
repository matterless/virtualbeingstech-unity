// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using VirtualBeings.Tech.BehaviorComposition;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VirtualBeings.Tech.ActiveCognition;
using System.Collections.Generic;
using UnityEditor.Animations;

namespace VirtualBeings
{
    /// <summary>
    /// A custom <see cref="PropertyDrawer"/> for <see cref="FST_SoundList"/> to make if more intuitive and user-friendly
    /// </summary>
    [CustomPropertyDrawer(typeof(FST_SoundList))]
    public class FST_SoundListPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            BeingSharedSettings parent = property.serializedObject.targetObject as BeingSharedSettings;
            AnimatorController animController = (AnimatorController)parent.AnimatorController;

            List<string> rsList = new List<string>();
            List<string> stList = new List<string>();

            foreach (AnimatorControllerLayer layer in animController.layers)
            {
                foreach (ChildAnimatorState s in layer.stateMachine.states)
                {
                    if (s.state.name.StartsWith("Root_") && layer.name == "Base Layer")
                    {
                        rsList.Add(s.state.name.Substring("Root_".Length));
                    }

                    // todo : only add ST's linked to the RS in the animatorController
                    if (s.state.name.StartsWith("ST"))
                    {
                        stList.Add($"{s.state.name}");
                    }
                }
            }

            PopupField<string> rsDropdown = new PopupField<string>(nameof(FST_SoundList.FS), rsList, 0, str => str, str => str);
            PopupField<string> stDropdown = new PopupField<string>(nameof(FST_SoundList.FST), stList, 0, str => str, str => str);
            PropertyField soundsField = new PropertyField(property.FindPropertyRelative("Sounds"));

            rsDropdown.BindProperty(property.FindPropertyRelative(nameof(FST_SoundList.FS)));
            stDropdown.BindProperty(property.FindPropertyRelative(nameof(FST_SoundList.FST)));
            soundsField.Bind(property.serializedObject);

            // Create property container element
            VisualElement container = new VisualElement();

            // Add fields to the container.
            container.Add(rsDropdown);
            container.Add(stDropdown);
            container.Add(soundsField);

            return container;
        }
    }
}

