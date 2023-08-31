// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using UnityEngine.UIElements;
using VirtualBeings.UIElements;
using UnityEngine;
using UnityEditor.UIElements;

namespace VirtualBeings
{
    /// <summary>
    /// <para>UI Element representing the <see cref="TreeView"/> child elements for <see cref="ImportModelView._animationsTree"/></para>
    /// </summary>
    public class ModelAnimationEntry : VisualElement
    {
        private const string BASE_PATH = EditorConsts.GLOBAL_EDTOR_BASE_PATH + "/" + nameof(VBPipelineEditor);

        private const string UXML_PATH  = BASE_PATH + "/Views/" + nameof(ImportModelView) + "/Entry/" + nameof(ModelAnimationEntry) + ".uxml";
        private const string USS_PATH   = BASE_PATH + "/Views/" + nameof(ImportModelView) + "/Entry/" + nameof(ModelAnimationEntry) + ".uss";

        private VisualElement   _root            => this.Q<VisualElement>(nameof(_root));
        private VisualElement   _data            => this.Q<VisualElement>(nameof(_data));
        private ObjectField     _animationAsset  => this.Q<ObjectField>(nameof(_animationAsset));
        private Label           _rootName        => this.Q<Label>(nameof(_rootName));

        private IAnimationTreeNode treeNode;

        internal ModelAnimationEntry()
        {
            LoadUI();
        }

        internal void Setup(IAnimationTreeNode modelAnimation)
        {
            this.treeNode = modelAnimation;
            this._animationAsset.objectType = typeof(AnimationClip);

            if (treeNode is DataAnimationTreeNode data)
            {
                _root.Display(false);
                _data.Display(true);
                _animationAsset.value = data._animationClip;
            }

            if (treeNode is RootAnimationTreeNode root)
            {
                _root.Display(true);
                _data.Display(false);
                _rootName.text = root._name;
            }

            Listen();
        }

        public void Clean()
        {
            Unlisten();

            treeNode = null;
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
