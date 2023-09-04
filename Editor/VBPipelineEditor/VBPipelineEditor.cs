// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using UnityEngine.UIElements;
using VirtualBeings.UIElements;
using UnityEngine;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;

namespace VirtualBeings
{
    /// <summary>
    /// Interface representing different setup steps for <see cref="VBPipelineEditor"/>
    /// </summary>
    internal interface IPipelineStep
    {
        event Action<IPipelineStep> OnStepChanged;
        bool IsValid();
        void Reset();
    }

    /// <summary>
    /// Data representing the global data/info of all the views for <see cref="VBPipelineEditor"/>
    /// </summary>
    internal class PipelineContext
    {
        internal ImportModelContext ImportModelContext;
        internal CreateStatesContext createStatesContext;
    }

    /// <summary>
    /// <para>Entry point for importing and validating assets to be used for KuteEngine</para>
    /// <para>This tool is made to be the one stop shop for setting the models and assets that will be used/driven by the animations</para>
    /// </summary>
    internal class VBPipelineEditor : EditorWindow
    {
        internal const string BASE_PATH = EditorConsts.GLOBAL_EDTOR_BASE_PATH + "/" + nameof(VBPipelineEditor);

        internal const string UXML_PATH = BASE_PATH + "/" + nameof(VBPipelineEditor) + ".uxml";
        internal const string DATA_PATH = BASE_PATH + "/" + nameof(VBPipelineEditorData) + ".asset";
        internal const string USS_PATH  = BASE_PATH + "/" + nameof(VBPipelineEditor) + ".uss";

        [MenuItem("VIRTUAL BEINGS/VB Pipeline")]
        public static void ShowWindow()
        {
            VBPipelineEditor wnd = GetWindow<VBPipelineEditor>();
            wnd.titleContent = new GUIContent(nameof(VBPipelineEditor));
        }

        private ObjectField EditorDataField => rootVisualElement.Q<ObjectField>(nameof(EditorDataField));
        private VisualElement VBBanner => rootVisualElement.Q<VisualElement>(nameof(VBBanner));
        private VisualElement ImportModelContainer => rootVisualElement.Q<VisualElement>(nameof(ImportModelContainer));
        private Button NextStepBtn => rootVisualElement.Q<Button>(nameof(NextStepBtn));
        private Button PrevStepBtn => rootVisualElement.Q<Button>(nameof(PrevStepBtn));
        private VBPipelineEditorData EditorData { get; set; }

        private PipelineContext pipelineContext;

        private ImportModelView importModelView;
        private CreateStatesView createStatesView;

        private List<IPipelineStep> Steps = new List<IPipelineStep>();

        private int currentStepIndex;

        private void CreateGUI()
        {
            EnsureDataAssetExists();

            LoadUI();

            Initialize();

            Listen();
        }

        private void EnsureDataAssetExists()
        {
            EditorData = AssetDatabase.LoadAssetAtPath<VBPipelineEditorData>(DATA_PATH);

            if (EditorData == null)
            {
                EditorData = CreateInstance<VBPipelineEditorData>();
                AssetDatabase.CreateAsset(EditorData, DATA_PATH);
                EditorData = AssetDatabase.LoadAssetAtPath<VBPipelineEditorData>(DATA_PATH);
            }

            if (EditorData == null)
            {
                EditorUtility.DisplayDialog("Error", "Error creating the editor data file!", "Ok");
            }
        }

        private void LoadUI()
        {

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            StyleSheet styling = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);

            visualTree.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(styling);
            rootVisualElement.styleSheets.Add(EditorConsts.GlobalStylesheet);

            pipelineContext = new PipelineContext();

            importModelView = new ImportModelView(EditorData, this);
            pipelineContext.ImportModelContext = importModelView.Context;

            createStatesView = new CreateStatesView(EditorData, this);
            pipelineContext.createStatesContext  = createStatesView.Context;

            ImportModelContainer.Add(importModelView);
            ImportModelContainer.Add(createStatesView);
        }


        private void Initialize()
        {
            minSize = EditorData.WindowMinSize;

            EditorDataField.value = EditorData;
            EditorDataField.SetEnabled(false);
            VBBanner.usageHints = UsageHints.DynamicTransform;

            currentStepIndex = 0;

            Steps.Add(importModelView);
            Steps.Add(createStatesView);

            RefreshStep();
        }

        private void Listen()
        {
            foreach (IPipelineStep step in Steps)
            {
                step.OnStepChanged += HandleStepChanged;
            }

            NextStepBtn.clickable.clicked += HandleNextStep;
            PrevStepBtn.clickable.clicked += HandlePreviousStep;
        }

        private void Unlisten()
        {
            foreach (IPipelineStep step in Steps)
            {
                step.OnStepChanged -= HandleStepChanged;
            }

            NextStepBtn.clickable.clicked -= HandleNextStep;
            PrevStepBtn.clickable.clicked -= HandlePreviousStep;
        }

        private void HandleStepChanged(IPipelineStep pipelineStep)
        {
            RefreshStep();
        }

        private void RefreshStep()
        {
            bool canStepNext = (currentStepIndex < (Steps.Count - 1)) && Steps[currentStepIndex].IsValid();
            bool canStepPrev = currentStepIndex > 0;

            NextStepBtn.SetEnabled(canStepNext);
            PrevStepBtn.SetEnabled(canStepPrev);

            foreach (VisualElement stepUi in Steps)
            {
                stepUi.Display(false);
            }

            VisualElement curr = (VisualElement)Steps[currentStepIndex];
            curr.Display(true);
        }

        private void HandlePreviousStep()
        {
            currentStepIndex--;
            RefreshStep();
        }

        private void HandleNextStep()
        {
            currentStepIndex++;
            RefreshStep();
        }

        private void Update()
        {
            AnimateLogo();
        }

        private void AnimateLogo()
        {
            float amplitude = 0.05f;
            float frequency = 3.0f;
            float normaliedOsc = Mathf.Sin((float)EditorApplication.timeSinceStartup * frequency);

            float scale = 1 + (normaliedOsc * amplitude);
            VBBanner.transform.scale = new Vector3(scale, scale, scale);
        }

        private void OnDestroy()
        {
            Unlisten();
        }
    }
}
