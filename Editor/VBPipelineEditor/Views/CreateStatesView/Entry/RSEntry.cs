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
    /// <para>UI Element representing the child elements for <see cref="CreateStatesView._availableRSsListView"/></para>
    /// </summary>
    internal class RSEntry : VisualElement
    {
        private const string BASE_PATH = EditorConsts.GLOBAL_EDTOR_BASE_PATH + "/" + nameof(VBPipelineEditor);

        private const string UXML_PATH  = BASE_PATH + "/Views/" + nameof(CreateStatesView) + "/Entry/" + nameof(RSEntry) + ".uxml";
        private const string USS_PATH   = BASE_PATH + "/Views/" + nameof(CreateStatesView) + "/Entry/" + nameof(RSEntry) + ".uss";

        private Label       _title  => this.Q<Label>(nameof(_title));
        private TextField   _rsName => this.Q<TextField>(nameof(_rsName));
        private TextField   _rsPath => this.Q<TextField>(nameof(_rsPath));

        private RSEditorInfo RSInfo;
        
        internal RSEntry()
        {
            LoadUI();
        }

        internal void Setup(RSEditorInfo info)
        {
            this.RSInfo = info;

            _rsName.SetEnabled(false);
            _rsPath.SetEnabled(false);

            _title.text = info.RSName;
            _rsName.value = info.StateName;
            _rsPath.value = info.Path;

            Listen();
        }

        public void Clean()
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
