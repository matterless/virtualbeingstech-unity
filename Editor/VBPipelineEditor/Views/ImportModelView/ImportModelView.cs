// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;
using System.Linq;
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
        internal GameObject AnimationAsset;
        internal AnimationClip AnimationClip;
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
        private const string BODY_ATTITUDE_DOMAIN = "BodyAttitude";

        private ObjectField _modelPicker => this.Q<ObjectField>(nameof(_modelPicker));
        private VisualElement _dragNDropZone => this.Q<VisualElement>(nameof(_dragNDropZone));
        private Label _dragNDropText => this.Q<Label>(nameof(_dragNDropText));
        private MeshViewer _meshViewer => this.Q<MeshViewer>(nameof(_meshViewer));
        private Toggle _useEditorMaterialToggle => this.Q<Toggle>(nameof(_useEditorMaterialToggle));
        private Slider _animationSlider => this.Q<Slider>(nameof(_animationSlider));
        private ObjectField _importAssetField => this.Q<ObjectField>(nameof(_importAssetField));
        private ObjectField _meshModel => this.Q<ObjectField>(nameof(_meshModel));
        private VisualElement _beingTypeContainer => this.Q<VisualElement>(nameof(_beingTypeContainer));
#if UNITY_2022_1_OR_NEWER
        private TreeView _animationsTree => this.Q<TreeView>(nameof(_animationsTree));
#endif
        private FloatField _importScaleFactor => this.Q<FloatField>(nameof(_importScaleFactor));
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

        private static Dictionary<string, BeingAssetData> beingAssets = new Dictionary<string, BeingAssetData>();

        [InitializeOnLoadMethod]
        public static void CacheBeingAssets()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(BeingAssetData)}");

            // get all BeingAssetData paths
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                BeingAssetData beingData = AssetDatabase.LoadAssetAtPath<BeingAssetData>(assetPath);
                beingAssets.Add(assetPath, beingData);
            }
        }

        public ImportModelView(VBPipelineEditorData data, VBPipelineEditor editor, ImportModelContext context = null)
        {
            if (context != null)
            {
                this.Context = context;
            }
            else
            {
                this.Context = new ImportModelContext();
            }

            this._animationTreeSource = new List<TreeViewItemData<IAnimationTreeNode>>();
            this._editorData = data;
            this._editor = editor;

            LoadUI();

            Initialize();

            Listen();

            ApplyContext();
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
#if UNITY_2022_1_OR_NEWER
        private void RefreshTreeRootDomains()
        {
            using (ListPool<int>.Get(out List<int> tmp))
            {
                tmp.AddRange(_animationsTree.GetRootIds());

                foreach (int rootID in tmp)
                {
                    _animationsTree.TryRemoveItem(rootID);
                }
            }

            string[] domains = _beingDomains[Context.BeingArchetype];

            for (int i = 0; i < domains.Length; i++)
            {
                string domainName = domains[i];
                RootAnimationTreeNode rootNode = new RootAnimationTreeNode() { _name = domainName };
                _animationsTree.AddItem(new TreeViewItemData<IAnimationTreeNode>(i, rootNode));
            }
        }
#endif
        private void ResetDomainAnimations(BeingArchetype beingArchetype)
        {
            Context.DomainAnimations.Clear();
            string[] domains = _beingDomains[beingArchetype];

            for (int i = 0; i < domains.Length; i++)
            {
                string domainName = domains[i];
                Context.DomainAnimations.Add(new DomainAnimationData()
                {
                    DomainName = domainName,
                    Animations = new List<AnimationData>()
                });
            }
        }

        private void Initialize()
        {
            // init animation tree roots
            {
                BeingArchetype[] beingsValues = (BeingArchetype[])Enum.GetValues(typeof(BeingArchetype));
                _allBeings = new List<BeingArchetype>(beingsValues);

                _beingDomains = new Dictionary<BeingArchetype, string[]>(beingsValues.Length);
                foreach (BeingArchetype being in beingsValues)
                {
                    using (ListPool<string>.Get(out List<string> tmp))
                    {
                        tmp.AddRange(Being.GetBeingArchetypeDomains(being));
                        tmp.Insert(0, BODY_ATTITUDE_DOMAIN);

                        _beingDomains.Add(being, tmp.ToArray());
                    }
                }
            }

            _beingTypePopup = new PopupField<BeingArchetype>("Being type", _allBeings, BeingArchetype.Humanoid, BeingArchetypeToString, BeingArchetypeToString);
            _beingTypeContainer.Add(_beingTypePopup);

#if UNITY_2022_1_OR_NEWER
            _animationsTree.showBorder = true;
            _animationsTree.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            _animationsTree.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _animationsTree.SetRootItems(_animationTreeSource);

            ResetDomainAnimations(_beingTypePopup.value);
#endif
            _meshViewer.UsePreviewMaterial = _useEditorMaterialToggle.value;


            _modelPicker.objectType = typeof(UnityEngine.Object);

            _importAssetField.objectType = typeof(BeingAssetData);
            _importAssetField.SetEnabled(false);

            _meshModel.objectType = typeof(Mesh);
            _meshModel.SetEnabled(false);
        }

        private void Listen()
        {
            _apply.clickable.clicked += HandleMoveAssetsBtn;
            _beingTypePopup.RegisterValueChangedCallback(HandleBeingTypeChanged);

#if UNITY_2022_1_OR_NEWER
            _animationsTree.makeItem += HandleMakeItem;
            _animationsTree.bindItem += HandleBindItem;
            _animationsTree.unbindItem += HandleUnbindItem;
#endif
            _dragNDropZone.RegisterCallback<DragEnterEvent>(HandleDragEnter);
            _dragNDropZone.RegisterCallback<DragLeaveEvent>(HandleDragExit);
            _dragNDropZone.RegisterCallback<DragExitedEvent>(HandleDragDone);
            _useEditorMaterialToggle.RegisterValueChangedCallback(HandlePreviewMaterialChanged);
            _importScaleFactor.RegisterValueChangedCallback(HandleScaleFactorChanged);
        }

        private void Unlisten()
        {
            _apply.clickable.clicked -= HandleMoveAssetsBtn;
            _beingTypePopup.UnregisterValueChangedCallback(HandleBeingTypeChanged);

#if UNITY_2022_1_OR_NEWER
            _animationsTree.makeItem -= HandleMakeItem;
            _animationsTree.bindItem -= HandleBindItem;
            _animationsTree.unbindItem -= HandleUnbindItem;
#endif
            _dragNDropZone.UnregisterCallback<DragEnterEvent>(HandleDragEnter);
            _dragNDropZone.UnregisterCallback<DragLeaveEvent>(HandleDragExit);
            _dragNDropZone.UnregisterCallback<DragExitedEvent>(HandleDragDone);
            _useEditorMaterialToggle.UnregisterValueChangedCallback(HandlePreviewMaterialChanged);
            _importScaleFactor.UnregisterValueChangedCallback(HandleScaleFactorChanged);
        }

        private void HandleScaleFactorChanged(ChangeEvent<float> evt)
        {
            Context.ScaleFactor = evt.newValue;
        }

        private void HandlePreviewMaterialChanged(ChangeEvent<bool> evt)
        {
            _meshViewer.UsePreviewMaterial = evt.newValue;
        }

        private void HandleBeingTypeChanged(ChangeEvent<BeingArchetype> evt)
        {
#if UNITY_2022_1_OR_NEWER
            Context.BeingArchetype = evt.newValue;
            ResetDomainAnimations(evt.newValue);
#endif
        }

        private bool MoveOrCopyAssets(out MoveAssetsOutputs output)
        {
            string saveFolder = EditorUtility.OpenFolderPanel("Select folder to move to", string.Empty, "Assets");

            saveFolder = EditorUtils.AbsoluteToRelativePath(saveFolder);

            string exportFolder = $"{saveFolder}/{Context.ModelAsset.name}";
#if UNITY_2022_1_OR_NEWER

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
                        domainAnims.Add(anim.AnimationAsset);
                    }
                }

                // execute the move command
                MoveAssetsInputs input = new MoveAssetsInputs()
                {
                    ModelAsset = Context.ModelAsset,
                    AnimationAssets = animsByDomainDict,
                    ExportPath = exportFolder,
                    MoveAssets = _moveToggle.value
                };

                MoveAssetsCommand cmd = new MoveAssetsCommand();

                return cmd.Execute(input, out output);

            }
#else
            output = default;
            return false;
#endif

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
                BeingArchetype = Context.BeingArchetype,
                ExportPath = moveOutput.ExportPath,
                ModelAsset = moveOutput.ModelAsset,
                ScaleFactor = _importScaleFactor.value
            };

            ApplyImportSettingsCommand cmd = new ApplyImportSettingsCommand();
            cmd.Execute(input, out importOutput);
        }

#if UNITY_2022_1_OR_NEWER
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
#endif
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

            if (DragAndDrop.paths.Length == 1)
            {
                string dragPath = DragAndDrop.paths[0];

                HasChildOrParentImportInputs checkInput = new HasChildOrParentImportInputs()
                {
                    AssetPath = dragPath,
                    AllBeingAssetsInProject = beingAssets
                };

                HasChildOrParentImportCommand checkCmd = new HasChildOrParentImportCommand();

                if (checkCmd.Execute(checkInput, out HasChildOrParentImportOutputs checkOutput))
                {
                    if (checkOutput.AssetImportError == ImportPathError.AssetIsImportFolder)
                    {
                        _editor.LoadAsset(checkOutput.BeingAsset);
                        ApplyContext();
                    }

                    return;
                }
            }

            FetchAssets();
            ApplyContext();
        }

        private void LoadAsset(BeingAssetData asset)
        {
            // copy the context data from the import asset
            Context.ModelAsset = asset.ImportContext.ModelAsset;
            Context.BeingArchetype = asset.ImportContext.BeingArchetype;
            Context.DomainAnimations.Clear();
            Context.DomainAnimations.AddRange(asset.ImportContext.DomainAnimations);
        }

        private void FetchAssets()
        {
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

                    Context.ModelAsset = model.ModelAsset;
                    Context.AvatarMasks.Clear();
                    Context.AvatarMasks.AddRange(output.AvatarMasks);
                }
            }

            // TODO (Ahmed) : Make this push to the context and not directly to the tree
            // set anims
            {
                string[] domains = _beingDomains[Context.BeingArchetype];

                foreach (AnimationAssetData anim in output.Anims)
                {
                    string animName = anim.AnimAsset.name;

                    string[] split = animName.Split("@");

                    if (split.Length != 2)
                    {
                        // EditorUtility.DisplayDialog("Error", $"The model asset's name {animName} doesn't respect the naming convension <ModelName>@<Domain>_<Action>", "Ok");
                        continue;
                    }

                    int domainIndex = -1;

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
                        // EditorUtility.DisplayDialog("Error", $"The animation {anim.AnimationClip.name} doesn't have a prefix that matches one of the existing domains, make sure to name it appropriatly", "Ok");
                        continue;
                    }


                    Context.DomainAnimations[domainIndex].Animations.Add(new AnimationData()
                    {
                        AnimationClip = anim.AnimationClip,
                        AnimtionAsset = anim.AnimAsset
                    });
                }
            }
        }

        internal void ApplyContext()
        {
            Mesh modelMesh = null;
            SkinnedMeshRenderer skinnedMesh = null;
            Matrix4x4 meshMatrix = Matrix4x4.identity;

            if (Context.ModelAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(Context.ModelAsset);
                UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                modelMesh = allAssets.OfType<Mesh>().FirstOrDefault();
                skinnedMesh = allAssets.OfType<SkinnedMeshRenderer>().FirstOrDefault();
                meshMatrix = skinnedMesh.transform.localToWorldMatrix;
            }

            _meshModel.value = modelMesh;
            _meshViewer.Mesh = modelMesh;
            _meshViewer.ModelMatrix = meshMatrix;
            _meshViewer.MeshMaterials.Clear();

            if (skinnedMesh != null)
            {
                skinnedMesh.GetSharedMaterials(_meshViewer.MeshMaterials);
            }

            _meshViewer.PreviewMaterial = _editorData.PreviewMaterial;
            _beingTypePopup.value = Context.BeingArchetype;
#if UNITY_2022_1_OR_NEWER
            RefreshTreeRootDomains();
            using (ListPool<string>.Get(out List<string> tmpDomains))
            {
                tmpDomains.AddRange(_beingDomains[Context.BeingArchetype]);

                foreach (DomainAnimationData animsPerDomain in Context.DomainAnimations)
                {
                    int domainIndex = tmpDomains.IndexOf(animsPerDomain.DomainName);

                    if (domainIndex == -1)
                    {
                        Debug.LogError("Couldn't find domain");
                    }

                    foreach (AnimationData animData in animsPerDomain.Animations)
                    {
                        string animName = animData.AnimtionAsset.name;

                        string[] split = animName.Split("@");

                        if (split.Length != 2)
                        {
                            EditorUtility.DisplayDialog("Error", $"The model asset's name {animName} doesn't respect the naming convension <ModelName>@<Domain>_<Action>", "Ok");
                            continue;
                        }

                        DataAnimationTreeNode animModel = new DataAnimationTreeNode()
                        {
                            AnimationAsset = animData.AnimtionAsset,
                            AnimationClip = animData.AnimationClip
                        };

                        int id = animModel.AnimationClip.GetHashCode();
                        IAnimationTreeNode item = _animationsTree.GetItemDataForId<IAnimationTreeNode>(id);

                        if (item != null)
                        {
                            _editor.ShowNotification(new GUIContent($"The animation {animData.AnimationClip.name} alreayd exists"), 0.5f);
                            continue;
                        }

                        int rootId = _animationsTree.GetIdForIndex(domainIndex);
                        TreeViewItemData<IAnimationTreeNode> itemData = new TreeViewItemData<IAnimationTreeNode>(id, animModel);
                        _animationsTree.AddItem(itemData, domainIndex);
                        _animationsTree.ExpandItem(rootId);
                        _animationsTree.Rebuild();
                        _animationsTree.RefreshItems();
                    }
                }
            }
#endif
        }

        private void HandleDragEnter(DragEnterEvent evt)
        {
            _dragNDropZone.RemoveFromClassList(DRAG_NEUTRAL);

            if (DragAndDrop.paths.Length == 1)
            {
                string dragPath = DragAndDrop.paths[0];

                HasChildOrParentImportInputs checkInput = new HasChildOrParentImportInputs()
                {
                    AssetPath = dragPath,
                    AllBeingAssetsInProject = beingAssets
                };
                HasChildOrParentImportOutputs checkOutput = new HasChildOrParentImportOutputs();

                HasChildOrParentImportCommand checkCmd = new HasChildOrParentImportCommand();

                if (checkCmd.Execute(checkInput, out checkOutput))
                {
                    string importPath = AssetDatabase.GetAssetPath(checkOutput.BeingAsset);

                    switch (checkOutput.AssetImportError)
                    {
                        case ImportPathError.AssetAlreadyInsideImport:
                            {
                                _dragNDropZone.AddToClassList(DRAG_ERROR);
                                _dragNDropText.text = $"Can't import an asset that is already inside an import folder , import asset found at {importPath}";
                                break;
                            }
                        case ImportPathError.AssetIsImportFolder:
                            {
                                _dragNDropZone.AddToClassList(DRAG_VALID);
                                _dragNDropText.text = $"Loadable import folder found , import asset found at {importPath}";
                                break;
                            }
                        case ImportPathError.ImportIsInsideAsset:
                            {
                                _dragNDropZone.AddToClassList(DRAG_ERROR);
                                _dragNDropText.text = $"Can't import a folder that indirectly contains the import asset , import asset found at {importPath}";
                                break;
                            }
                    }

                    return;
                }
            }


            FetchRelevantAssetsInputs input = new FetchRelevantAssetsInputs()
            {
                ScannablePaths = new List<string>(DragAndDrop.paths)
            };

            FetchRelevantAssetsOutputs output;

            FetchRelevantAssetsCommand cmd = new FetchRelevantAssetsCommand();
            cmd.Execute(input, out output);


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
