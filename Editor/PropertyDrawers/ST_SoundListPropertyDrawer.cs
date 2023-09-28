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
    /// A custom <see cref="PropertyDrawer"/> for <see cref="ST_SoundList"/> to make if more intuitive and user-friendly
    /// </summary>
    [CustomPropertyDrawer(typeof(ST_SoundList))]
    public class ST_SoundListPropertyDrawer : PropertyDrawer
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

            AnimatorController animController = parent.AnimatorController as AnimatorController;

            List<string> rsList = new List<string>();
            List<string> stList = new List<string>();

            foreach(IRS rs in parent.GetRSs())
            {
                rsList.Add(rs.Name);
            }

            foreach(IST st in parent.GetSTs())
            {
                stList.Add(st.Name);
            }

            rsList.Sort();
            stList.Sort();

            PopupField<string> rsDropdown = new PopupField<string>("RS", rsList, 0, str => str, str => str);
            PopupField<string> stDropdown = new PopupField<string>("ST", stList, 0, str => str, str => str);

            rsDropdown.BindProperty(property.FindPropertyRelative(nameof(ST_SoundList.RS)));
            stDropdown.BindProperty(property.FindPropertyRelative(nameof(ST_SoundList.ST)));

            // Add fields to the container.
            container.Add(rsDropdown);
            container.Add(stDropdown);
            container.Add(soundsField);

            return container;
        }
    }
}

