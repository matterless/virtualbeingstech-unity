// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Animations;
using VirtualBeings.Tech.BehaviorComposition;
using VirtualBeings.Tech.ActiveCognition;

namespace VirtualBeings
{
    /// <summary>
    /// A custom <see cref="PropertyDrawer"/> for <see cref="FOT_SoundList"/> to make if more intuitive and user-friendly
    /// </summary>
    [CustomPropertyDrawer(typeof(FOT_SoundList))]
    public class FOT_SoundListPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            BeingSharedSettings parent = property.serializedObject.targetObject as BeingSharedSettings;

            VisualElement container = new VisualElement();

            PropertyField soundsField = new PropertyField(property.FindPropertyRelative("Sounds"));
            soundsField.Bind(property.serializedObject);

            if (parent == null)
            {
                container.Add(soundsField);
                return container;
            }

            RSTransitionInfo[] transitions = parent.RSTransitionInfos;
            AnimatorController animController = (AnimatorController)parent.AnimatorController;

            List<string> sourceFS = new List<string>();
            List<string> targetFS = new List<string>();
            List<string> transitionList = new List<string>();

            foreach (IFTT transition in parent.GetFacialTransitions())
            {
                transitionList.Add(transition.Name);
            }
            
            foreach (IFS fs in parent.GetFSs())
            {
                sourceFS.Add(fs.Name);
                targetFS.Add(fs.Name);
            }

            sourceFS.Sort();
            targetFS.Sort();

            PopupField<string> rsDropdown = new PopupField<string>("Source FS", sourceFS, 0, str => str, str => str);
            PopupField<string> targetRsDropdown = new PopupField<string>("Target FS", targetFS, 0, str => str, str => str);
            PopupField<string> transitionTypes = new PopupField<string>("Facial Transition Type", transitionList, 0, str => str, str => str);

            rsDropdown.BindProperty(property.FindPropertyRelative(nameof(FOT_SoundList.FS)));
            targetRsDropdown.BindProperty(property.FindPropertyRelative(nameof(FOT_SoundList.TargetFS)));
            transitionTypes.BindProperty(property.FindPropertyRelative(nameof(FOT_SoundList.TransitionType)));

            // Add fields to the container.
            container.Add(rsDropdown);
            container.Add(targetRsDropdown);
            container.Add(transitionTypes);
            container.Add(soundsField);

            return container;
        }
    }
}