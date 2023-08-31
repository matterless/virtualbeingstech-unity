// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VirtualBeings.Tech.ActiveCognition;
using VirtualBeings.Tech.UnityIntegration;
using VirtualBeings.UIElements;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using System.Collections;

namespace VirtualBeings
{
    /// <summary>
    /// A custom <see cref="PropertyDrawer"/> for <see cref="Sound"/> to make if more intuitive and user-friendly
    /// </summary>
    [CustomPropertyDrawer(typeof(Sound))]
    public class SoundPropertyDrawer : PropertyDrawer
    {
        public static IEnumerator CrtPlayCoroutine(AudioClip clip)
        {
            GameObject go = new GameObject("Audio Preview Player");
            go.hideFlags = HideFlags.HideInHierarchy;
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.clip = clip;
            source.Play();

            yield return new WaitForSecondsRealtime(clip.length + 0.1f);

            Object.DestroyImmediate(go);
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            BeingSharedSettings parent = property.serializedObject.targetObject as BeingSharedSettings;

            SerializedProperty animationMulProp = property.FindPropertyRelative(nameof(Sound.AnimationMultiplier));
            SerializedProperty audioClipProp = property.FindPropertyRelative(nameof(Sound.AudioClip));

            HelpBox animationWarning = new HelpBox();
            animationWarning.text = $"{nameof(Sound.AnimationMultiplier)} should always be greater than 0";
            animationWarning.messageType = HelpBoxMessageType.Error;

            PropertyField audioClip = new PropertyField(audioClipProp);
            audioClip.style.flexGrow = 1;

            FloatField animationMultiplier = new FloatField(nameof(Sound.AnimationMultiplier));
            animationMultiplier.BindProperty(animationMulProp);

            PropertyField soundType = new PropertyField(property.FindPropertyRelative(nameof(Sound.SoundType)));
            PropertyField poignancy01 = new PropertyField(property.FindPropertyRelative(nameof(Sound.Poignancy01)));
            PropertyField vocalAnnoyingness = new PropertyField(property.FindPropertyRelative(nameof(Sound.VocalAnnoyingnessSummand)));

            audioClip.Bind(property.serializedObject);
            animationMultiplier.Bind(property.serializedObject);
            soundType.Bind(property.serializedObject);
            poignancy01.Bind(property.serializedObject);
            vocalAnnoyingness.Bind(property.serializedObject);

            // Create property container element
            VisualElement container = new VisualElement();

            // Add fields to the container
            VisualElement row = new VisualElement();
            row.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            Button playAudioBtn = new Button();
            playAudioBtn.text = "►";
            playAudioBtn.clickable.clicked += () =>
            {
                AudioClip clip = audioClipProp.objectReferenceValue as AudioClip;
                EditorCoroutineUtility.StartCoroutineOwnerless(CrtPlayCoroutine(clip));
            };

            // refresh the warning
            {
                bool show = animationMulProp.floatValue <= 0;
                animationWarning.Display(show);

                bool enableBtn = audioClipProp.objectReferenceValue != null;
                playAudioBtn.SetEnabled(enableBtn);
            }

            // listen to mul changes
            container.TrackPropertyValue(animationMulProp, (prop) =>
            {
                bool show = prop.floatValue <= 0;
                animationWarning.Display(show);
            });

            // listen to mul changes
            container.TrackPropertyValue(audioClipProp, (prop) =>
            {
                bool enableBtn = audioClipProp.objectReferenceValue != null;
                playAudioBtn.SetEnabled(enableBtn);
            });

            row.Add(audioClip);
            row.Add(playAudioBtn);

            container.Add(row);
            container.Add(animationMultiplier);
            container.Add(animationWarning);
            container.Add(soundType);
            container.Add(poignancy01);
            container.Add(vocalAnnoyingness);

            return container;
        }
    }
}
