// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using VirtualBeings.Tech.BehaviorComposition;
using VirtualBeings.UIElements;

namespace VirtualBeings
{
    internal interface IAnimationTreeNode { }

    /// <summary>
    /// Data for <seealso cref="TreeView"/> child elements that describe a parent domain
    /// </summary>
    internal class RootAnimationTreeNode : IAnimationTreeNode
    {
        internal string _name;
    }

    /// <summary>
    /// data for <seealso cref="TreeView"/> child elements that describe an animation entry
    /// </summary>
    internal class DataAnimationTreeNode : IAnimationTreeNode
    {
        internal GameObject _animationAsset;
        internal AnimationClip _animationClip;
    }

    /// <summary>
    /// Data representing the state/info of the <see cref="ImportModelView"/>
    /// </summary>
    internal class ImportModelContext
    {
        internal GameObject _modelAsset;
        internal Mesh _modelMesh;
        internal List<Material> _meshMaterials;
        internal Matrix4x4 _modelMatrix;
        internal string _modelName;
    }

    /// <summary>
    /// <para>The first step of the VB Pipeline tool</para>
    /// <para>This view is responsible for importing and previewing all the animations and models for the VB Pipeline</para>
    /// <para>It allows for drag and drop and previewing</para>
    /// </summary>
    internal class ImportModelView : VisualElement, IPipelineStep
    {
        private const string BASE_PATH = EditorConsts.GLOBAL_EDTOR_BASE_PATH + "/" + nameof(VBPipelineEditor) + "/Views/" + nameof(ImportModelView);

        private const string UXML_PATH = BASE_PATH + "/" + nameof(ImportModelView) + ".uxml";
        private const string USS_PATH = BASE_PATH + "/" + nameof(ImportModelView) + ".uss";

        private const string MESSAGE_NEUTRAL = "Drag and Drop your folder/assets here ...";
        private const string DRAG_NEUTRAL = "drag-neutral";
        private const string DRAG_VALID = "drag-valid";
        private const string DRAG_ERROR = "drag-error";

        private ObjectField _modelPicker => this.Q<ObjectField>(nameof(_modelPicker));
        private VisualElement _dragNDropZone => this.Q<VisualElement>(nameof(_dragNDropZone));
        private Label _dragNDropText => this.Q<Label>(nameof(_dragNDropText));
        private MeshViewer _meshViewer => this.Q<MeshViewer>(nameof(_meshViewer));
        private Toggle _useEditorMaterialToggle => this.Q<Toggle>(nameof(_useEditorMaterialToggle));
        private Slider _animationSlider => this.Q<Slider>(nameof(_animationSlider));
        private ObjectField _meshModel => this.Q<ObjectField>(nameof(_meshModel));
        private VisualElement _beingTypeContainer => this.Q<VisualElement>(nameof(_beingTypeContainer));
        private TreeView _animationsTree => this.Q<TreeView>(nameof(_animationsTree));
        private Toggle _moveToggle => this.Q<Toggle>(nameof(_moveToggle));
        private Button _apply => this.Q<Button>(nameof(_apply));

        private PopupField<BeingArchetype> _beingTypePopup;
        private List<TreeViewItemData<IAnimationTreeNode>> _animationTreeSource;

        private List<BeingArchetype> _allBeings;
        private Dictionary<BeingArchetype, string[]> _beingDomains;

        private readonly VBPipelineEditorData _editorData;
        private readonly VBPipelineEditor _editor;
        internal readonly ImportModelContext Context;

        public event Action<IPipelineStep> OnStepChanged;

        public ImportModelView(VBPipelineEditorData data, VBPipelineEditor editor)
        {
            this.Context = new ImportModelContext();
            this._animationTreeSource = new List<TreeViewItemData<IAnimationTreeNode>>();
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

        private static string BeingArchetypeToString(BeingArchetype being)
        {
            return being.ToString();
        }

        private void RefreshAnimationTree(BeingArchetype beingArchetype)
        {
            using (ListPool<int>.Get(out var tmp))
            {
                tmp.AddRange(_animationsTree.GetRootIds());

                foreach (int rootID in tmp)
                {
                    _animationsTree.TryRemoveItem(rootID);
                }
            }

            string[] domains = _beingDomains[beingArchetype];
            for (int i = 0; i < domains.Length; i++)
            {
                string domainName = domains[i];
                RootAnimationTreeNode rootNode = new RootAnimationTreeNode() { _name = domainName };
                _animationsTree.AddItem(new TreeViewItemData<IAnimationTreeNode>(i, rootNode));
            }
        }

        private void Initialize()
        {
            _meshViewer.UsePreviewMaterial = _useEditorMaterialToggle.value;

            BeingArchetype[] beingsValues = (BeingArchetype[])Enum.GetValues(typeof(BeingArchetype));
            _allBeings = new List<BeingArchetype>(beingsValues);

            _beingDomains = new Dictionary<BeingArchetype, string[]>(beingsValues.Length);
            foreach (BeingArchetype being in beingsValues)
            {
                _beingDomains.Add(being, Being.GetBeingArchetypeDomains(being));
            }

            _beingTypePopup = new PopupField<BeingArchetype>("Being type", _allBeings, BeingArchetype.Humanoid, BeingArchetypeToString, BeingArchetypeToString);
            _beingTypeContainer.Add(_beingTypePopup);

            _animationsTree.showBorder = true;
            _animationsTree.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            _animationsTree.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _animationsTree.SetRootItems(_animationTreeSource);

            RefreshAnimationTree(_beingTypePopup.value);

            _modelPicker.objectType = typeof(UnityEngine.Object);
            _meshModel.objectType = typeof(Mesh);
        }

        private void Listen()
        {
            _apply.clickable.clicked += HandleMoveAssetsBtn;
            _beingTypePopup.RegisterValueChangedCallback(HandleBeingTypeChanged);
            _animationsTree.makeItem += HandleMakeItem;
            _animationsTree.bindItem += HandleBindItem;
            _animationsTree.unbindItem += HandleUnbindItem;
            _dragNDropZone.RegisterCallback<DragEnterEvent>(HandleDragEnter);
            _dragNDropZone.RegisterCallback<DragLeaveEvent>(HandleDragExit);
            _dragNDropZone.RegisterCallback<DragExitedEvent>(HandleDragDone);
            _useEditorMaterialToggle.RegisterValueChangedCallback(HandlePreviewMaterialChanged);
        }


        private void Unlisten()
        {
            _apply.clickable.clicked -= HandleApplyBtn;
            _beingTypePopup.UnregisterValueChangedCallback(HandleBeingTypeChanged);
            _animationsTree.makeItem -= HandleMakeItem;
            _animationsTree.bindItem -= HandleBindItem;
            _animationsTree.unbindItem -= HandleUnbindItem;
            _dragNDropZone.UnregisterCallback<DragEnterEvent>(HandleDragEnter);
            _dragNDropZone.UnregisterCallback<DragLeaveEvent>(HandleDragExit);
            _dragNDropZone.UnregisterCallback<DragExitedEvent>(HandleDragDone);
            _useEditorMaterialToggle.RegisterValueChangedCallback(HandlePreviewMaterialChanged);
        }

        private void HandleApplyBtn()
        {
            throw new NotImplementedException();
        }
        private void HandlePreviewMaterialChanged(ChangeEvent<bool> evt)
        {
            _meshViewer.UsePreviewMaterial = evt.newValue;
        }

        private void HandleBeingTypeChanged(ChangeEvent<BeingArchetype> evt)
        {
            RefreshAnimationTree(evt.newValue);
        }

        private bool MoveOrCopyAssets(out MoveAssetsOutputs output)
        {
            string saveFolder = EditorUtility.OpenFolderPanel("Select folder to move to", string.Empty, "Assets");

            saveFolder = EditorUtils.AbsoluteToRelativePath(saveFolder);

            string exportFolder = $"{saveFolder}/{Context._modelName}";

            // animations
            using (ListPool<int>.Get(out List<int> rootIDs))
            using (ListPool<int>.Get(out List<int> animIDs))
            using (DictionaryPool<string, List<GameObject>>.Get(out Dictionary<string, List<GameObject>> animsByDomainDict))
            {
                // fetch and prepare all the inputs for the move assets commands
                rootIDs.AddRange(_animationsTree.GetRootIds());

                foreach (int rootID in rootIDs)
                {
                    int index = _animationsTree.viewController.GetIndexForId(rootID);
                    RootAnimationTreeNode root = _animationsTree.GetItemDataForId<RootAnimationTreeNode>(rootID);

                    string rootFolder = $"{exportFolder}/{root._name}";
                    EditorUtils.CreateFoldersFromPath(rootFolder);

                    animIDs.Clear();
                    animIDs.AddRange(_animationsTree.GetChildrenIdsForIndex(index));

                    List<GameObject> domainAnims = new List<GameObject>(animIDs.Count);
                    animsByDomainDict.Add(root._name, domainAnims);

                    foreach (int animID in animIDs)
                    {
                        DataAnimationTreeNode anim = (DataAnimationTreeNode)_animationsTree.GetItemDataForId<IAnimationTreeNode>(animID);
                        domainAnims.Add(anim._animationAsset);
                    }
                }

                // execute the move command
                MoveAssetsInputs input = new MoveAssetsInputs()
                {
                    ModelAsset = Context._modelAsset,
                    AnimationAssets = animsByDomainDict,
                    ExportPath = exportFolder,
                    MoveAssets = _moveToggle.value
                };

                MoveAssetsCommand cmd = new MoveAssetsCommand();
                return cmd.Execute(input, out output);
            }
        }

        private void HandleMoveAssetsBtn()
        {
            MoveOrCopyAssets(out MoveAssetsOutputs moveOutput);

            ApplyImportSettings(moveOutput, out ApplyImportSettingsOutput importOutput);
        }

        private void ApplyImportSettings(MoveAssetsOutputs moveOutput, out ApplyImportSettingsOutput importOutput)
        {
            ApplyImportSettingsInputs input = new ApplyImportSettingsInputs()
            {
                AnimationAssets = moveOutput.ModelAnimations,
                BeingArchetype = _beingTypePopup.value,
                ExportPath = moveOutput.ExportPath,
                ModelAsset = moveOutput.ModelAsset
            };

            ApplyImportSettingsCommand cmd = new ApplyImportSettingsCommand();
            cmd.Execute(input, out importOutput);
        }

        private void HandleUnbindItem(VisualElement ui, int index)
        {
            ModelAnimationEntry casted = (ModelAnimationEntry)ui;
            casted.Clean();
        }

        private void HandleBindItem(VisualElement ui, int index)
        {
            ModelAnimationEntry casted = (ModelAnimationEntry)ui;
            IAnimationTreeNode node = (IAnimationTreeNode)_animationsTree.viewController.GetItemForIndex(index);
            casted.Setup(node);
        }

        private VisualElement HandleMakeItem()
        {
            return new ModelAnimationEntry();
        }

        private void HandleDragExit(DragLeaveEvent evt)
        {
            _dragNDropZone.RemoveFromClassList(DRAG_VALID);
            _dragNDropZone.RemoveFromClassList(DRAG_ERROR);
            _dragNDropZone.AddToClassList(DRAG_NEUTRAL);
            _dragNDropText.text = MESSAGE_NEUTRAL;
        }

        private void HandleDragDone(DragExitedEvent evt)
        {
            _dragNDropZone.RemoveFromClassList(DRAG_VALID);
            _dragNDropZone.RemoveFromClassList(DRAG_ERROR);
            _dragNDropZone.AddToClassList(DRAG_NEUTRAL);
            _dragNDropText.text = MESSAGE_NEUTRAL;

            FetchRelevantAssetsCommand cmd = new FetchRelevantAssetsCommand();
            FetchRelevantAssetsInputs input = new FetchRelevantAssetsInputs()
            {
                ScannablePaths = new List<string>(DragAndDrop.paths)
            };

            cmd.Execute(input, out FetchRelevantAssetsOutputs output);

            // if there are any errors
            if (output.Errors.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string err in output.Errors)
                {
                    sb.AppendLine(err);
                }
                EditorUtility.DisplayDialog($"{output.Errors.Count} erros occured", sb.ToString(), "Ok");
                return;
            }

            // if multiple models are found
            if (output.Models.Count > 1)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Multiple models asset dropped at the same time");

                foreach (ModelAssetData m in output.Models)
                {
                    sb.AppendLine(AssetDatabase.GetAssetPath(m.ModelAsset));
                }

                sb.AppendLine("make sure only only mesh model exists");

                EditorUtility.DisplayDialog($"{output.Models.Count} models found", sb.ToString(), "Ok");
                EditorUtility.DisplayDialog("Error", sb.ToString(), "Ok");
                return;
            }

            // set model
            {
                if (output.Models.Count != 0)
                {
                    ModelAssetData model = output.Models[0];

                    Context._modelAsset = model.ModelAsset;
                    Context._modelMesh = model.ModelMesh;
                    Context._modelName = model.ModelAsset.name;
                    Context._modelMatrix = model.SkinnedMeshRenderer.transform.localToWorldMatrix;
                    Context._meshMaterials = new List<Material>();

                    model.SkinnedMeshRenderer.GetSharedMaterials(Context._meshMaterials);

                    _meshModel.value = Context._modelMesh;

                    _meshViewer.Mesh = Context._modelMesh;
                    _meshViewer.ModelMatrix = Context._modelMatrix;
                    _meshViewer.MeshMaterials = Context._meshMaterials;
                    _meshViewer.PreviewMaterial = _editorData.PreviewMaterial;
                }
            }

            // set anims
            {
                foreach (AnimationAssetData anim in output.Anims)
                {
                    string animName = anim.AnimAsset.name;

                    string[] split = animName.Split("@");

                    if (split.Length != 2)
                    {
                        EditorUtility.DisplayDialog("Error", $"The model asset's name {animName} doesn't respect the naming convension <ModelName>@<Domain>_<Action>", "Ok");
                        continue;
                    }

                    DataAnimationTreeNode animModel = new DataAnimationTreeNode()
                    {
                        _animationAsset = anim.AnimAsset,
                        _animationClip = anim.AnimationClip
                    };

                    int domainIndex = -1;

                    string[] domains = _beingDomains[_beingTypePopup.value];

                    for (int i = 0; i < domains.Length; i++)
                    {
                        string domain = domains[i];

                        if (!anim.AnimationClip.name.StartsWith(domain))
                            continue;

                        domainIndex = i;
                        break;
                    }

                    if (domainIndex == -1)
                    {
                        EditorUtility.DisplayDialog("Error", $"The animation {anim.AnimationClip.name} doesn't have a prefix that matches one of the existing domains, make sure to name it appropriatly", "Ok");
                        continue;
                    }

                    IAnimationTreeNode item = _animationsTree.GetItemDataForId<IAnimationTreeNode>(anim.GetHashCode());

                    if (item != null)
                    {
                        _editor.ShowNotification(new GUIContent($"The animation {anim.AnimationClip.name} alreayd exists"), 0.5f);
                        continue;
                    }

                    int rootId = _animationsTree.GetIdForIndex(domainIndex);
                    TreeViewItemData<IAnimationTreeNode> itemData = new TreeViewItemData<IAnimationTreeNode>(anim.GetHashCode(), animModel);
                    _animationsTree.AddItem(itemData, domainIndex);
                    _animationsTree.ExpandItem(rootId);
                    _animationsTree.Rebuild();
                    _animationsTree.RefreshItems();
                }
            }
        }

        private void HandleDragEnter(DragEnterEvent evt)
        {
            _dragNDropZone.RemoveFromClassList(DRAG_NEUTRAL);

            FetchRelevantAssetsCommand cmd = new FetchRelevantAssetsCommand();
            FetchRelevantAssetsInputs input = new FetchRelevantAssetsInputs()
            {
                ScannablePaths = new List<string>(DragAndDrop.paths)
            };

            cmd.Execute(input, out FetchRelevantAssetsOutputs output);

            int totalAssetCount = output.Models.Count + output.Anims.Count;

            if (totalAssetCount == 0)
            {
                _dragNDropZone.AddToClassList(DRAG_ERROR);
                _dragNDropText.text = "Not valid assets found";
                return;
            }

            if (output.Models.Count > 1)
            {
                _dragNDropZone.AddToClassList(DRAG_ERROR);
                _dragNDropText.text = "Too many model asset, you can only drag one model asset";
                return;
            }

            // valid
            {
                _dragNDropZone.AddToClassList(DRAG_VALID);
                _dragNDropText.text = $"Dragging {output.Models.Count} model asset(s) and {output.Anims.Count} animations assets(s)";
                return;
            }
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
