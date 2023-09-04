// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
        private RenderTexture _previewTexture;

        private UIInputListener _inputListener;

        private double _lastTimestamp;

        public MeshViewer()
        {
            focusable = true;
            Focus();
            CameraRotation = Quaternion.identity;
            _previewCmd = new CommandBuffer();
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
                _previewImage.image = _previewTexture;
                _previewImage.MarkDirtyRepaint();
                return;
            }

            if (_previewImage.contentRect.width == 0 || _previewImage.contentRect.height == 0)
            {
                _previewImage.image = _previewTexture;
                _previewImage.MarkDirtyRepaint();
                return;
            }

            bool recreatedTex = _previewTexture == null ||
                            _previewTexture.width != _previewImage.contentRect.width ||
                            _previewTexture.height != _previewImage.contentRect.height;
            if (recreatedTex)
            {
                if (_previewTexture != null)
                {
                    _previewTexture.Release();
                }

                RenderTextureDescriptor desc = new RenderTextureDescriptor()
                {
                    width = (int)_previewImage.contentRect.width,
                    height = (int)_previewImage.contentRect.height,
                    depthBufferBits = 0,
                    msaaSamples = 1,
                    volumeDepth = 1,
                    enableRandomWrite = false,
                    dimension = TextureDimension.Tex2D,
                    graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat
                };

                _previewTexture = new RenderTexture(desc);
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

            _previewImage.image = _previewTexture;
            _previewImage.MarkDirtyRepaint();
        }

        private void RenderToTexture(Mesh mesh, CommandBuffer previewCmd, Matrix4x4 modelMatrix)
        {
            previewCmd.Clear();
            previewCmd.SetRenderTarget(_previewTexture);
            previewCmd.ClearRenderTarget(true, true, Color.clear);

            float fov = 60;
            float aspect = _previewTexture.width / (float)_previewTexture.height;
            Matrix4x4 proj = Matrix4x4.Perspective(fov, aspect, 0.3f, 100f);
            Matrix4x4 projGPU = GL.GetGPUProjectionMatrix(proj, true);

            Matrix4x4 camTransform = Matrix4x4.TRS(CameraPosition, CameraRotation, Vector3.one);
            camTransform = camTransform.inverse;

            Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1));
            Matrix4x4 viewMatrix = scaleMatrix * camTransform;

            previewCmd.SetViewProjectionMatrices(viewMatrix, projGPU);
            previewCmd.SetGlobalVector("_LightColor0", new Vector4(2, 1.72f, 1.3f, 2));
            previewCmd.SetGlobalVector("_LightShadowData", new Vector4(0, 66.66666f, 0.3333333f, -2.6666670f));
            previewCmd.SetGlobalVector("_WorldSpaceCameraPos", new Vector4( CameraPosition.x , CameraPosition.y , CameraPosition.z , 0));
            previewCmd.SetGlobalVector("_WorldSpaceLightPos0", new Vector4(0.3213938f, 0.7660444f, -0.5566704f, 0));

            if (MeshMaterials == null)
            {
                _previewMessage.text = "Material is null";
            }

            if (UsePreviewMaterial)
            {
                for (int i = 0; i < mesh.subMeshCount; ++i)
                {
                    previewCmd.DrawMesh(mesh, modelMatrix, PreviewMaterial, i, -1);
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
                        previewCmd.DrawMesh(mesh, modelMatrix, MeshMaterials[i], i, -1);
                    }
                }
            }

            Graphics.ExecuteCommandBuffer(previewCmd);
        }


    }
}