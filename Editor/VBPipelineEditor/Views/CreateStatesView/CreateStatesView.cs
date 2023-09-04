// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using VirtualBeings.UIElements;

namespace VirtualBeings
{
    /// <summary>
    /// data for the <seealso cref="RSEntry"/> UI element in <see cref="CreateStatesView._availableRSsListView"/> to describe the RS info
    /// </summary>
    public class RSEditorInfo
    {
        public string RSName;
        public string StateName;
        public string Path;
    }

    /// <summary>
    /// data for the <seealso cref="STEntry"/> UI element in <see cref="CreateStatesView._associatedSTsListView"/> to describe the STs info
    /// </summary>
    public class STEditorInfo
    {
        public RSEditorInfo   From;
        public string         STName;
        public string         StateName;
        public string         Path;
    }

    /// <summary>
    /// Data representing the state/info of the <see cref="CreateStatesView"/>
    /// </summary>
    internal class CreateStatesContext
    {
        internal AnimatorController animatorController;
        internal List<string>       states = new List<string>();
    }

    /// <summary>
    /// <para>The second step of the VB Pipeline tool</para>
    /// <para>This view is responsible for organizing the RSs and STs and validating their settings</para>
    /// </summary>
    internal class CreateStatesView : VisualElement, IPipelineStep
    {
        private const string BASE_PATH = EditorConsts.GLOBAL_EDTOR_BASE_PATH + "/" + nameof(VBPipelineEditor) + "/Views/" + nameof(CreateStatesView);

        private const string UXML_PATH  = BASE_PATH + "/" + nameof(CreateStatesView) + ".uxml";
        private const string USS_PATH   = BASE_PATH + "/" + nameof(CreateStatesView) + ".uss";

        private ObjectField     _sourceControllerField  => this.Q<ObjectField>(nameof(_sourceControllerField));
        private VisualElement   _rsSection              => this.Q<VisualElement>(nameof(_rsSection));
        private ListView        _availableRSsListView   => this.Q<ListView>(nameof(_availableRSsListView));
        private Label           _stTitle                => this.Q<Label>(nameof(_stTitle));
        private ListView        _associatedSTsListView  => this.Q<ListView>(nameof(_associatedSTsListView));

        internal    readonly    CreateStatesContext     Context;
        private     readonly    VBPipelineEditorData    _editorData;
        private     readonly    VBPipelineEditor        _editor;

        private readonly List<RSEditorInfo> _allRSs;
        private readonly List<STEditorInfo> _currentSTs;

        public event Action<IPipelineStep> OnStepChanged;

        internal CreateStatesView(VBPipelineEditorData data, VBPipelineEditor editor)
        {
            this.Context = new CreateStatesContext();
            this._currentSTs = new List<STEditorInfo>();
            this._allRSs = new List<RSEditorInfo>();
            this._editorData = data;
            this._editor = editor;

            LoadUI();

            Initialize();

            Listen();
        }

        private void LoadUI()
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            StyleSheet styling = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);

            visualTree.CloneTree(this);
            AddToClassList("grow-1");
            styleSheets.Add(styling);
            styleSheets.Add(EditorConsts.GlobalStylesheet);
        }

        private void Initialize()
        {
            _sourceControllerField.objectType = typeof(AnimatorController);

            _availableRSsListView.itemsSource = _allRSs;
            _availableRSsListView.selectionType = SelectionType.Single;
            _availableRSsListView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            _availableRSsListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _availableRSsListView.makeItem = () => new RSEntry();
            _availableRSsListView.bindItem = (v, i) => { ((RSEntry)v).Setup(_allRSs[i]); };
            _availableRSsListView.unbindItem = (v, i) => { ((RSEntry)v).Clean(); };
            _availableRSsListView.destroyItem = (v) => { ((RSEntry)v).Clean(); };

            _associatedSTsListView.itemsSource = _currentSTs;
            _associatedSTsListView.selectionType = SelectionType.Single;
            _associatedSTsListView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            _associatedSTsListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _associatedSTsListView.makeItem = () => new STEntry();
            _associatedSTsListView.bindItem = (v, i) => { ((STEntry)v).Setup(_currentSTs[i]); };
            _associatedSTsListView.unbindItem = (v, i) => { ((STEntry)v).Clean(); };
            _associatedSTsListView.destroyItem = (v) => { ((STEntry)v).Clean(); };

            Refresh();
        }

        private void Listen()
        {
            _sourceControllerField.RegisterValueChangedCallback(HandleControllerChanged);
#if UNITY_2022_1_OR_NEWER
            _availableRSsListView.selectedIndicesChanged += HandleRSSelectedChanged;
#endif
            }

        private void Unlisten()
        {
            _sourceControllerField.UnregisterValueChangedCallback(HandleControllerChanged);
#if UNITY_2022_1_OR_NEWER
            _availableRSsListView.selectedIndicesChanged -= HandleRSSelectedChanged;
#endif
        }

        private void HandleRSSelectedChanged(IEnumerable<int> selectedIndicies)
        {
            using (ListPool<int>.Get(out List<int> indicies))
            {
                indicies.AddRange(selectedIndicies);

                if (indicies.Count == 0)
                {
                    _stTitle.text = $"Select an RS to show the related STs";
                    _associatedSTsListView.Display(false);
                    return;
                }

                int RSindex = selectedIndicies.First();
                RSEditorInfo currentRS = _allRSs[RSindex];

                
                FetchSTsCommand cmd = new FetchSTsCommand();
                FetchSTsInputs input = new FetchSTsInputs()
                {
                    FromRS = currentRS,
                    AnimatorController = Context.animatorController,
                    Results = _currentSTs
                };

                cmd.Execute(input, out FetchSTsOutputs output);

                _stTitle.text = $"{currentRS.RSName} STs";
                _associatedSTsListView.Display(true);
                _associatedSTsListView.RefreshItems();
            }
        }

        private void HandleControllerChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            Context.animatorController = evt.newValue as AnimatorController;
            Refresh();
        }

        private void Refresh()
        {
            if (Context.animatorController == null)
            {
                _rsSection.Display(false);
                return;
            }

            _rsSection.Display(true);

            InitSTs();
        }

        private void InitSTs()
        {
            _allRSs.Clear();

            FetchAllRSsCommand cmd = new FetchAllRSsCommand();

            FetchAllRSsInputs input = new FetchAllRSsInputs()
            {
                AnimatorController = Context.animatorController,
                Results = _allRSs
            };

            cmd.Execute(input, out _);

            _availableRSsListView.RefreshItems();
        }

        private void OnDestroy()
        {
            Unlisten();
        }

        public bool IsValid()
        {
            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
