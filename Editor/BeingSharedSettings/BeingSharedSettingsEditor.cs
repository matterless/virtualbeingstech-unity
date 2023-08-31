// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using UnityEngine.UIElements;
using VirtualBeings.UIElements;
using UnityEditor.UIElements;
using VirtualBeings.Tech.ActiveCognition;
using UnityEditor.Animations;
using System.Collections.Generic;
using System;

namespace VirtualBeings
{
    /// <summary>
    /// a custom UI Element used as the base editor for <see cref="BeingSharedSettings"/> assets
    /// </summary>
    public class BeingSharedSettingsEditor : VisualElement
    {
        private static List<string> _animatorLayerIndicies = new List<string>()
        {
            nameof(BeingSharedSettings.LayerIndexRootScale ),
            nameof(BeingSharedSettings.LayerIndexFullBodyAttitudes ),
            nameof(BeingSharedSettings.LayerIndexTailBaseAdditive),
            nameof(BeingSharedSettings.LayerIndexTailAdditive),
            nameof(BeingSharedSettings.LayerIndexEyes),
            nameof(BeingSharedSettings.LayerIndexMouth ),
            nameof(BeingSharedSettings.LayerIndexEarsAdditive),
            nameof(BeingSharedSettings.LayerIndexEarBaseAdditive ),
            nameof(BeingSharedSettings.LayerIndexBlink ),
            nameof(BeingSharedSettings.LayerIndexBreathing),
            nameof(BeingSharedSettings.LayerIndexBellyFull ),
            nameof(BeingSharedSettings.LayerIndexEyeSize ),
            nameof(BeingSharedSettings.LayerIndexMouthOpen ),
            nameof(BeingSharedSettings.LayerIndexPupilDilation),
            nameof(BeingSharedSettings.LayerIndexEyeSquint),
            nameof(BeingSharedSettings.LayerIndexFeet ),
            nameof(BeingSharedSettings.LayerIndexLeftHand ),
            nameof(BeingSharedSettings.LayerIndexRightHand )
        };

        private const string BASE_PATH  = EditorConsts.GLOBAL_EDTOR_BASE_PATH + "/BeingSharedSettings";
        private const string UXML_PATH  = BASE_PATH + "/" + nameof(BeingSharedSettingsEditor) + ".uxml";
        private const string USS_PATH   = BASE_PATH + "/" + nameof(BeingSharedSettingsEditor) + ".uss";
 
        private readonly BeingSharedSettings    _beingSharedSettings;
        private readonly Action                 _updateAnimationCallback;
      
        private VisualElement   _animationSection           => this.Q<VisualElement>(nameof(_animationSection));
        private VisualElement   _animationPopups            => this.Q<VisualElement>(nameof(_animationPopups));
        private HelpBox         _animationValidationState   => this.Q<HelpBox>(nameof(_animationValidationState));
        private Button          _updateAnimationDataBtn     => this.Q<Button>(nameof(_updateAnimationDataBtn));

        private List<PopupField<int>>       _popupFields = new List<PopupField<int>>();
        private AnimatorController          _animatorController;
        private AnimatorControllerLayer[]   _layers;
        private List<int>                   _allIndicies = new List<int>();
        private List<int>[]                 _dynamicIndicies = new List<int>[_animatorLayerIndicies.Count];
        
        public BeingSharedSettingsEditor(BeingSharedSettings beingSharedSettings , Action updateAnimationCallback)
        {
            this._beingSharedSettings = beingSharedSettings;
            this._updateAnimationCallback = updateAnimationCallback;
            LoadUI();
            Initialize();
        }

        private void LoadUI()
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            StyleSheet styling = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);

            visualTree.CloneTree(this);
            styleSheets.Add(styling);
            styleSheets.Add(EditorConsts.GlobalStylesheet);
        }

        private string IndexToDropdownLabel(int value)
        {
            if (_layers == null || value < 0)
                return "None";

            return _layers[value].name;
        }

        private Func<int, string> IndexToSelectedLabel(PopupField<int> popup)
        {
            int index = _popupFields.IndexOf(popup);
            string displayName = _animatorLayerIndicies[index];

            Func<int, string> func = (value) =>
            {
                if (_layers == null || value < 0)
                    return $"{displayName} \t → \t None";

                return $"{displayName} \t → \t {_layers[value].name}";
            };

            return func;
        }

        private void InitDynamicIndicies()
        {
            for (int i = 0; i < _dynamicIndicies.Length; i++)
            {
                _dynamicIndicies[i] = new List<int>();
            }
        }

        void RefrehshLayers()
        {
            _allIndicies.Clear();
            
            foreach(List<int> d in _dynamicIndicies)
            {
                d.Clear();
            }

            _layers = null;

            if (_animatorController == null)
                return;

            _layers = _animatorController.layers;
            _allIndicies.Capacity = _layers.Length;

            _allIndicies.Add(-1);

            // we skip the first "Base Layer" since we don't want it in the selection list
            for (int i = 1; i < _animatorController.layers.Length; i++)
            {
                AnimatorControllerLayer l = _animatorController.layers[i];
                _allIndicies.Add(i);
            }
        }

        private void Initialize()
        {
            InitDynamicIndicies();
            SerializedObject serializedObject = new SerializedObject(_beingSharedSettings);

            _animatorController = _beingSharedSettings.AnimatorController as AnimatorController;

            _updateAnimationDataBtn.clickable.clicked += HandleUpdateAnimationDataClicked;
            RefrehshLayers();
            HandleAssetChanged(serializedObject);

            // track animators value
            SerializedProperty animatorProp = serializedObject.FindProperty(nameof(BeingSharedSettings.AnimatorController));
            this.TrackPropertyValue(animatorProp, HandleAnimatorChanged);
            this.TrackSerializedObjectValue(serializedObject, HandleAssetChanged);
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);

            while (iterator.NextVisible(false))
            {
                // animator controller field
                if (iterator.name == nameof(BeingSharedSettings.AnimatorController))
                {
                    PropertyField propertyField = new PropertyField(iterator) { name = "PropertyField:" + iterator.propertyPath };
                    propertyField.BindProperty(iterator);
                    _animationSection.Insert(0, propertyField);
                    continue;
                }

                // layers indicies
                int index = -1;
                if ((index = _animatorLayerIndicies.IndexOf(iterator.name)) != -1)
                {
                    PopupField<int> popup = new PopupField<int>(_dynamicIndicies[index], 0, null, IndexToDropdownLabel);
                    _popupFields.Add(popup);

                    popup.formatSelectedValueCallback = IndexToSelectedLabel(popup);
                    popup.BindProperty(iterator);
                    popup.SetEnabled(_animatorController != null);

                    this.TrackPropertyValue(iterator, HandleLayerChanged);
                    _animationPopups.Add(popup);
                    continue;
                }

                FilterDropdownChoices();

                // rest of the properties
                {
                    PropertyField propertyField = new PropertyField(iterator) { name = "PropertyField:" + iterator.propertyPath };
                    propertyField.BindProperty(iterator);

                    if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                        propertyField.SetEnabled(value: false);

                    this.Add(propertyField);
                }
            }
        }

        private void HandleUpdateAnimationDataClicked()
        {
            _updateAnimationCallback?.Invoke();
        }

        private bool ValidateAsset()
        {
            return true;
        }
        private void HandleAssetChanged(SerializedObject @object)
        {
            if (ValidateAsset())
            {
                _animationValidationState.messageType = HelpBoxMessageType.Info;
                _animationValidationState.text = "The settings are setup correctly";
            }
            else
            {
                _animationValidationState.messageType = HelpBoxMessageType.Error;
                _animationValidationState.text = "The settings are not setup correctly, please fix the missing fields/values";
            }
        }

        private void HandleLayerChanged(SerializedProperty property)
        {
            FilterDropdownChoices();
        }

        private void FilterDropdownChoices()
        {
            for (int i = 0; i < _popupFields.Count; i++)
            {
                PopupField<int> p = _popupFields[i];
                List<int> validChoice = _dynamicIndicies[i];
                validChoice.Clear();
                validChoice.AddRange(_allIndicies);

                for (int j = 0; j < _popupFields.Count; j++)
                {
                    PopupField<int> other = _popupFields[j];
                    if (i == j)
                        continue;

                    if (other.value == -1)
                        continue;

                    validChoice.Remove(other.value);
                }

                p.choices = validChoice;
            }
        }

        private void HandleAnimatorChanged(SerializedProperty property)
        {
            _animatorController = _beingSharedSettings.AnimatorController as AnimatorController;

            RefrehshLayers();

            bool enabled = _beingSharedSettings.AnimatorController != null;

            for (int i = 0; i < _popupFields.Count; i++)
            {
                PopupField<int> p = _popupFields[i];
                p.SetEnabled(enabled);
                p.value = -1;
                p.choices = _dynamicIndicies[i];
            }
        }
    }
}
