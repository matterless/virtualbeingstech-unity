using UnityEngine;

namespace VirtualBeings
{
    /// <summary>
    /// A Data asset containing the settings/prefs of the editor tool/>
    /// </summary>
    public class VBPipelineEditorData : ScriptableObject
    {
        public Material PreviewMaterial;

        public Vector3 ModelRotation;

        public Vector3 ModelPosition;

        public Vector2 CameraRotation;

        public float CameraZoom;

        public Vector2 WindowMinSize;
    }
}
