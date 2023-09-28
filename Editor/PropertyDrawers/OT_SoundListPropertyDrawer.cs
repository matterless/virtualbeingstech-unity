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
    /// A custom <see cref="PropertyDrawer"/> for <see cref="OT_SoundList"/> to make if more intuitive and user-friendly
    /// </summary>
    [CustomPropertyDrawer(typeof(OT_SoundList))]
    public class OT_SoundListPropertyDrawer : PropertyDrawer
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


            List<string> sourceRSList = new List<string>();
            List<string> targetRSList = new List<string>();
            List<string> transitionList = new List<string>();

            foreach (ITransitionType transition in parent.GetTransitions())
            {
                transitionList.Add(transition.Name);
            }
            foreach (IRS rs in parent.GetRSs())
            {
                sourceRSList.Add(rs.Name);
                targetRSList.Add(rs.Name);
            }

            sourceRSList.Sort();
            targetRSList.Sort();
            transitionList.Sort();

            PopupField<string> rsDropdown = new PopupField<string>("Source RS", sourceRSList, 0, str => str, str => str);
            PopupField<string> targetRsDropdown = new PopupField<string>("Target RS", targetRSList, 0, str => str, str => str);
            PopupField<string> transitionTypes = new PopupField<string>("Transition Type", transitionList, 0, str => str, str => str);


            rsDropdown.BindProperty(property.FindPropertyRelative(nameof(OT_SoundList.RS)));
            targetRsDropdown.BindProperty(property.FindPropertyRelative(nameof(OT_SoundList.TargetRS)));
            transitionTypes.BindProperty(property.FindPropertyRelative(nameof(OT_SoundList.TransitionType)));


            // Add fields to the container.
            container.Add(rsDropdown);
            container.Add(targetRsDropdown);
            container.Add(transitionTypes);
            container.Add(soundsField);

            return container;
        }
    }

   
}
