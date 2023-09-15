// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Unity.Profiling;

namespace VirtualBeings.UIElements
{
    /// <summary>
    /// <para>A Custom UI Element that renders a mesh onto a texture and displays it as a UI Element</para>
    /// <para>The rendering can be controlled by passing a list of materials along with the camera's position and rotation</para>
    /// </summary>
    public class MeshViewer : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<MeshViewer, UxmlTraits> { }

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

        private const string BASE_PATH = EditorConsts.GLOBAL_EDTOR_BASE_PATH + "/CustomElement/MeshViewer";
        private const string UXML_PATH = BASE_PATH + "/" + nameof(MeshViewer) + ".uxml";
        private const string USS_PATH = BASE_PATH + "/" + nameof(MeshViewer) + ".uss";

        private Label _previewMessage => this.Q<Label>(nameof(_previewMessage));
        private Image _previewImage => this.Q<Image>(nameof(_previewImage));

        public Mesh Mesh { get; set; }
        public List<Material> MeshMaterials { get; set; }
        public Material PreviewMaterial { get; set; }
        public bool UsePreviewMaterial { get; set; }
        public Matrix4x4 ModelMatrix { get; set; }
        public Vector3 CameraPosition { get; set; }

        public float Yaw;
        public float Pitch;
        public Quaternion CameraRotation { get; set; }

        private CommandBuffer _previewCmd;
        private RenderTexture _colorBuffer;
        private RenderTexture _depthBuffer;

        private UIInputListener _inputListener;

        private double _lastTimestamp;

        public MeshViewer()
        {
            focusable = true;
            Focus();
            CameraRotation = Quaternion.identity;
            MeshMaterials = new List<Material>();
            _previewCmd = new CommandBuffer();
            _previewCmd.name = "Editor rendering CommandBuffer";

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            StyleSheet styling = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);

            visualTree.CloneTree(this);
            styleSheets.Add(EditorConsts.GlobalStylesheet);
            styleSheets.Add(styling);

            RegisterCallback<AttachToPanelEvent>(HandleAdded);
            RegisterCallback<DetachFromPanelEvent>(HandleRemoved);

            _lastTimestamp = EditorApplication.timeSinceStartup;

            _inputListener = new UIInputListener(this);
        }

        private void HandleRemoved(DetachFromPanelEvent evt)
        {
            UnregisterCallback<AttachToPanelEvent>(HandleAdded);
            UnregisterCallback<DetachFromPanelEvent>(HandleRemoved);

            EditorApplication.update -= OnTick;

            _inputListener.Unbind();
        }

        private void HandleAdded(AttachToPanelEvent evt)
        {
            EditorApplication.update += OnTick;

            _inputListener.Bind();
        }

        private void OnTick()
        {
            double delta = EditorApplication.timeSinceStartup - _lastTimestamp;

            Input((float)delta);

            Render((float)delta);

            _lastTimestamp = EditorApplication.timeSinceStartup;

            _inputListener.PostTick();
        }

        private void Input(float delta)
        {
            UIInputListener.InputState inputState = _inputListener.State;

            // mouse input
            if (inputState.IsMousePressed)
            {
                int rotSpeed = 20;

                Yaw += inputState.MouseDelta.x * rotSpeed * delta;
                Pitch += inputState.MouseDelta.y * rotSpeed * delta;
                Pitch = Mathf.Clamp(Pitch, -70, 70);

                Quaternion rotUp = Quaternion.AngleAxis(Yaw, Vector3.up);
                Quaternion rotRight = rotUp * Quaternion.AngleAxis(Pitch, Vector3.right);
                Quaternion totalRot = rotUp * (rotUp * rotRight);

                Vector3 euler = totalRot.eulerAngles;

                Quaternion zCancel = Quaternion.AngleAxis(-euler.z, Vector3.forward);

                Quaternion rot = totalRot * zCancel;

                CameraRotation = rot;
            }

            // keyboard input
            Vector2 keyboard = Vector2.zero;

            {
                bool upPressed = inputState.KeyboardInput[(int)KeyCode.Z] == UIInputListener.KeyState.Pressed;
                bool downPressed = inputState.KeyboardInput[(int)KeyCode.S] == UIInputListener.KeyState.Pressed;
                bool rightPressed = inputState.KeyboardInput[(int)KeyCode.D] == UIInputListener.KeyState.Pressed;
                bool leftPressed = inputState.KeyboardInput[(int)KeyCode.Q] == UIInputListener.KeyState.Pressed;

                if (upPressed)
                {
                    keyboard.y += 1;
                }

                if (downPressed)
                {
                    keyboard.y -= 1;
                }

                if (rightPressed)
                {
                    keyboard.x += 1;
                }

                if (leftPressed)
                {
                    keyboard.x -= 1;
                }
            }

            float strafeSpeed = 2;
            float scrollSpeed = 2;

            Vector3 vel =
                (CameraRotation * Vector3.right * (keyboard.x * strafeSpeed)) +
                (CameraRotation * Vector3.up * (keyboard.y * strafeSpeed)) +
                (CameraRotation * Vector3.forward * (inputState.ScrollDelta * scrollSpeed));

            CameraPosition += vel * delta;
        }

        private void Render(float delta)
        {
            if (!_previewImage.IsLayoutBuilt())
            {
                _previewImage.image = _colorBuffer;
                _previewImage.MarkDirtyRepaint();
                return;
            }

            if (_previewImage.contentRect.width == 0 || _previewImage.contentRect.height == 0)
            {
                _previewImage.image = _colorBuffer;
                _previewImage.MarkDirtyRepaint();
                return;
            }

            bool recreatedTex = _colorBuffer == null ||
                            _colorBuffer.width != _previewImage.contentRect.width ||
                            _colorBuffer.height != _previewImage.contentRect.height;
            if (recreatedTex)
            {
                if (_colorBuffer != null)
                {
                    _colorBuffer.Release();
                }
                if (_depthBuffer != null)
                {
                    _depthBuffer.Release();
                }

                RenderTextureDescriptor desc = new RenderTextureDescriptor()
                {
                    width = (int)_previewImage.contentRect.width,
                    height = (int)_previewImage.contentRect.height,
                    depthBufferBits = 16,
                    msaaSamples = 1,
                    volumeDepth = 1,
                    depthStencilFormat = GraphicsFormat.None,
                    stencilFormat = GraphicsFormat.None,
                    enableRandomWrite = false,
                    dimension = TextureDimension.Tex2D,
                    graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat
                };

                var colorDesc = desc;
                var depthDesc = desc;
                depthDesc.depthBufferBits = 32;
                depthDesc.graphicsFormat = GraphicsFormat.None;
                depthDesc.depthStencilFormat = GraphicsFormat.D32_SFloat;
                _colorBuffer = new RenderTexture(colorDesc);
                _depthBuffer = new RenderTexture(depthDesc);
            }

            if (Mesh != null)
            {
                _previewMessage.text = string.Empty;
                RenderToTexture(Mesh, _previewCmd, ModelMatrix);
            }
            else
            {
                _previewMessage.text = "Select a model to preview";
            }

            _previewImage.image = _colorBuffer;
            _previewImage.MarkDirtyRepaint();
        }

        private void RenderToTexture(Mesh mesh, CommandBuffer previewCmd, Matrix4x4 modelMatrix)
        {
            float fov = 60;
            float aspect = _colorBuffer.width / (float)_colorBuffer.height;
            Matrix4x4 proj = Matrix4x4.Perspective(fov, aspect, 0.3f, 100f);
            Matrix4x4 projGPU = GL.GetGPUProjectionMatrix(proj, true);

            Matrix4x4 lookMatrix = Matrix4x4.TRS(CameraPosition, CameraRotation, Vector3.one);

            Matrix4x4 scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));

            Matrix4x4 viewMatrix = scaleMatrix * lookMatrix.inverse;

            GlobalKeyword directonal = new GlobalKeyword("DIRECTIONAL");
            previewCmd.Clear();
            previewCmd.BeginSample("Editor rendering");
            previewCmd.EnableKeyword(directonal);
            previewCmd.SetRenderTarget(_colorBuffer, _depthBuffer);
            previewCmd.ClearRenderTarget(RTClearFlags.All, Color.clear, 1, 0);
            previewCmd.SetViewProjectionMatrices(viewMatrix, projGPU);
            previewCmd.SetGlobalVector("_LightColor0", new Vector4(2, 1.72f, 1.3f, 2));
            previewCmd.SetGlobalVector("_LightShadowData", new Vector4(0, 66.66666f, 0.3333333f, -2.6666670f));
            previewCmd.SetGlobalVector("_WorldSpaceCameraPos", new Vector4(CameraPosition.x, CameraPosition.y, CameraPosition.z, 0));
            previewCmd.SetGlobalVector("_WorldSpaceLightPos0", new Vector4(0.3213938f, 0.7660444f, -0.5566704f, 0));

            if (MeshMaterials == null)
            {
                _previewMessage.text = "Material is null";
            }

            if (UsePreviewMaterial)
            {
                int passIndex = PreviewMaterial.FindPass("FORWARD");

                for (int i = 0; i < PreviewMaterial.passCount; ++i)
                {
                    Debug.Log(PreviewMaterial.GetPassName(i));
                }

                for (int i = 0; i < mesh.subMeshCount; ++i)
                {
                    previewCmd.DrawMesh(mesh, modelMatrix, PreviewMaterial, i, 0);
                }
            }
            else
            {
                if (MeshMaterials.Count != mesh.subMeshCount)
                    _previewMessage.text = "Material count different than submesh count";

                else
                {
                    for (int i = 0; i < mesh.subMeshCount; ++i)
                    {
                        Material mat = MeshMaterials[i];
                        int passIndex = mat.FindPass("FORWARD");
                        previewCmd.DrawMesh(mesh, modelMatrix, mat, i, passIndex);
                    }
                }
            }

            previewCmd.EndSample("Editor rendering");
            Graphics.ExecuteCommandBuffer(previewCmd);
        }


    }
}