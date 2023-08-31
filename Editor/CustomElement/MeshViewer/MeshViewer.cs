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

        private const string BASE_PATH  = EditorConsts.GLOBAL_EDTOR_BASE_PATH + "/CustomElement/MeshViewer";
        private const string UXML_PATH  = BASE_PATH + "/" + nameof(MeshViewer) + ".uxml";
        private const string USS_PATH   = BASE_PATH + "/" + nameof(MeshViewer) + ".uss";

        private Label _previewMessage   => this.Q<Label>(nameof(_previewMessage));
        private Image _previewImage     => this.Q<Image>(nameof(_previewImage));

        public Mesh             Mesh { get; set; }
        public List<Material>   MeshMaterials { get; set; }
        public Material         PreviewMaterial { get; set; }
        public bool             UsePreviewMaterial { get; set; }
        public Matrix4x4        ModelMatrix { get; set; }
        public Vector3          CameraPosition { get; set; }
        public Quaternion       CameraRotation { get; set; }

        private CommandBuffer _previewCmd;
        private RenderTexture _previewTexture;
        private bool _isDragging;

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
        }

        private void HandleRemoved(DetachFromPanelEvent evt)
        {
            UnregisterCallback<AttachToPanelEvent>(HandleAdded);
            UnregisterCallback<DetachFromPanelEvent>(HandleRemoved);

            EditorApplication.update -= OnTick;

            UnregisterCallback<MouseMoveEvent>(HandleMove);
            UnregisterCallback<MouseDownEvent>(HandleDown);
            UnregisterCallback<MouseUpEvent>(HandleUp);
            UnregisterCallback<MouseLeaveEvent>(HandleLeave);

            UnregisterCallback<KeyDownEvent>(HandleKeydown);
            UnregisterCallback<WheelEvent>(HandleScroll);

        }
        private void HandleAdded(AttachToPanelEvent evt)
        {
            EditorApplication.update += OnTick;

            RegisterCallback<MouseMoveEvent>(HandleMove);
            RegisterCallback<MouseDownEvent>(HandleDown);
            RegisterCallback<MouseUpEvent>(HandleUp);
            RegisterCallback<MouseLeaveEvent>(HandleLeave);

            RegisterCallback<KeyDownEvent>(HandleKeydown);
            RegisterCallback<WheelEvent>(HandleScroll);
        }

        private void HandleDown(MouseDownEvent evt)
        {
            _isDragging = true;
        }

        private void HandleUp(MouseUpEvent evt)
        {
            _isDragging = false;
        }

        private void HandleLeave(MouseLeaveEvent evt)
        {
            _isDragging = false;
        }

        private void HandleScroll(WheelEvent evt)
        {
            Vector3 old = CameraPosition;
            old .z -= evt.delta.y;

            CameraPosition = old;
        }


        private void HandleKeydown(KeyDownEvent evt)
        {
            Vector3 vel = Vector3.zero;

            switch (evt.keyCode)
            {
                case KeyCode.Z:
                    {
                        vel.y += 1;
                        break;
                    }
                case KeyCode.S:
                    {
                        vel.y -= 1;
                        break;
                    }
                case KeyCode.D:
                    {
                        vel.x += 1;
                        break;
                    }
                case KeyCode.Q:
                    {
                        vel.x -= 1;
                        break;
                    }
            }

            vel = vel.normalized;

            CameraPosition += vel;
        }

        private void HandleMove(MouseMoveEvent evt)
        {
            if (!_isDragging)
                return;

            CameraRotation *= Quaternion.AngleAxis(evt.mouseDelta.x, Vector2.up);
            CameraRotation *= Quaternion.AngleAxis(evt.mouseDelta.y, Vector2.right);
        }

        private void OnTick()
        {
            Render();
        }

        private void Render()
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
                RenderToTexture(Mesh, _previewCmd, ModelMatrix);
            }

            _previewImage.image = _previewTexture;
            _previewImage.MarkDirtyRepaint();
        }

        public void SetCamera(Vector3 position, Quaternion rotation)
        {
            CameraPosition = position;
            CameraRotation = rotation;
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

            Debug.Log($"pos : {CameraPosition} , rot : {CameraRotation}");

            Matrix4x4 camTransform = Matrix4x4.TRS(CameraPosition, CameraRotation, Vector3.one);
            camTransform = camTransform.inverse;

            Vector3 from = new Vector3(0, 1, -3);
            Vector3 to = new Vector3(0, 1, 0);
            Vector3 up = Vector3.up;

            Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1));

            Matrix4x4 lookMatrix = Matrix4x4.LookAt(from, to, up);
            Matrix4x4 viewMatrix = scaleMatrix * camTransform;

            previewCmd.SetViewProjectionMatrices(viewMatrix, projGPU);
            previewCmd.SetGlobalVector("_WorldSpaceLightPos0", Vector4.one.normalized);
            previewCmd.SetGlobalVector("_LightColor0", Vector4.one);

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