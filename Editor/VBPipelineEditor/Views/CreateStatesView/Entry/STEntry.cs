// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using UnityEngine.UIElements;
using VirtualBeings.UIElements;

namespace VirtualBeings
{
    /// <summary>
    /// <para>UI Element representing the child elements for <see cref="CreateStatesView._associatedSTsListView"/></para>
    /// </summary>
    internal class STEntry : VisualElement
    {
        private const string BASE_PATH = EditorConsts.GLOBAL_EDTOR_BASE_PATH + "/" + nameof(VBPipelineEditor);

        private const string UXML_PATH = BASE_PATH + "/Views/" + nameof(CreateStatesView) + "/Entry/" + nameof(STEntry) + ".uxml";
        private const string USS_PATH = BASE_PATH + "/Views/" + nameof(CreateStatesView) + "/Entry/" + nameof(STEntry) + ".uss";

        private Label       _title  => this.Q<Label>(nameof(_title));
        private TextField   _stName => this.Q<TextField>(nameof(_stName));
        private TextField   _stPath => this.Q<TextField>(nameof(_stPath));

        private STEditorInfo STInfo;

        internal STEntry()
        {
            LoadUI();
        }

        internal void Setup(STEditorInfo info)
        {
            this.STInfo = info;

            _stName.SetEnabled(false);
            _stPath.SetEnabled(false);

            _title.text = $"{info.From.RSName} → {info.STName}";
            _stName.value = info.StateName;
            _stPath.value = info.Path;

            Listen();
        }

        internal void Clean()
        {
            Unlisten();
        }

        private void LoadUI()
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            StyleSheet styling = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);

            visualTree.CloneTree(this);
            styleSheets.Add(styling);
            styleSheets.Add(EditorConsts.GlobalStylesheet);
        }

        private void Listen()
        {

        }

        private void Unlisten()
        {
        }
    }
}
