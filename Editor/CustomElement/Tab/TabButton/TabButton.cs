// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace VirtualBeings.UIElements
{
    /// <summary>
    /// A UI Element used to repserent the header of a single tab that is parented by <see cref="TabRoot"/>
    /// </summary>
    public class TabButton : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TabButton, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
            }
        }

        private const string BASE_PATH  = EditorConsts.GLOBAL_EDTOR_BASE_PATH + "/CustomElement/Tab/TabButton";
        private const string UXML_PATH  = BASE_PATH + "/" + nameof(TabButton) + ".uxml";
        private const string USS_PATH   = BASE_PATH + "/" + nameof(TabButton) + ".uss";

        private const string SELECTED_CLASS = "selected";
        private const string ROOT_CLASS     = "tab-button";

        private Label HeaderTitle => this.Q<Label>(nameof(HeaderTitle));

        public event Action<TabButton> OnClicked;

        public TabButton()
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            StyleSheet styling = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);

            visualTree.CloneTree(this);
            this.AddToClassList(ROOT_CLASS);
            styleSheets.Add(EditorConsts.GlobalStylesheet);
            styleSheets.Add(styling);

            this.RegisterCallback<ClickEvent>(HandleClicked);
        }

        private void HandleClicked(ClickEvent evt)
        {
            OnClicked?.Invoke(this);
        }

        public void Select()
        {
            this.AddToClassList(SELECTED_CLASS);
        }

        public void Unselect()
        {
            this.RemoveFromClassList(SELECTED_CLASS);
        }

        public TabButton(string title) : this() 
        {
            HeaderTitle.text = title;
        }
    }
}