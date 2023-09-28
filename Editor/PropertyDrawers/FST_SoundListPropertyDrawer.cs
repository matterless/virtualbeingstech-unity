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

            VisualElement container = new VisualElement();

            PropertyField soundsField = new PropertyField(property.FindPropertyRelative("Sounds"));
            soundsField.Bind(property.serializedObject);

            if (parent == null)
            {
                container.Add(soundsField);
                return container;
            }

            AnimatorController animController = (AnimatorController)parent.AnimatorController;

            List<string> fsList = new List<string>();
            List<string> fstList = new List<string>();

            foreach(IFS fs in parent.GetFSs())
            {
                fsList.Add(fs.Name);
            }

            foreach(IFST fst in parent.GetFSTs())
            {
                fstList.Add(fst.Name);
            }

            fsList.Sort();
            fstList.Sort();

            PopupField<string> rsDropdown = new PopupField<string>(nameof(FST_SoundList.FS), fsList, 0, str => str, str => str);
            PopupField<string> stDropdown = new PopupField<string>(nameof(FST_SoundList.FST), fstList, 0, str => str, str => str);

            rsDropdown.BindProperty(property.FindPropertyRelative(nameof(FST_SoundList.FS)));
            stDropdown.BindProperty(property.FindPropertyRelative(nameof(FST_SoundList.FST)));

            // Add fields to the container.
            container.Add(rsDropdown);
            container.Add(stDropdown);
            container.Add(soundsField);

            return container;
        }
    }
}

