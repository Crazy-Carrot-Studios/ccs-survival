using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioPreviewCamera
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Hidden editor-only preview camera with orbit/pan/zoom controls.
// PLACEMENT: Editor utility used by Equipment Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Destroyed when window closes or Play Mode changes.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed class CCS_EquipmentFitStudioPreviewCamera
    {
        #region Variables

        private GameObject cameraRoot;

        private Camera previewCamera;

        private RenderTexture renderTexture;

        private Vector3 focusPoint = Vector3.up;

        private float orbitYaw;

        private float orbitPitch = 15f;

        private float distance = 1.5f;

        private bool isDraggingOrbit;

        private bool isDraggingPan;

        private bool isDraggingLook;

        private Vector2 lastMousePosition;

        private GameObject framePlayerRoot;

        private string frameSocketId = string.Empty;

        private int textureWidth = 640;

        private int textureHeight = 360;

        #endregion

        #region Properties

        public RenderTexture RenderTexture => renderTexture;

        public Camera PreviewCamera => previewCamera;

        #endregion

        #region Public Methods

        public void EnsureCamera(CCS_EquipmentFitStudioSettings settings)
        {
            if (cameraRoot == null)
            {
                cameraRoot = new GameObject(CCS_EquipmentConstants.EditorPreviewCameraObjectName);
                cameraRoot.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                previewCamera = cameraRoot.AddComponent<Camera>();
                previewCamera.enabled = true;
                previewCamera.clearFlags = CameraClearFlags.SolidColor;
            }

            if (settings != null)
            {
                previewCamera.fieldOfView = settings.PreviewCameraFov;
                previewCamera.nearClipPlane = settings.PreviewCameraNearClip;
                previewCamera.farClipPlane = settings.PreviewCameraFarClip;
                previewCamera.backgroundColor = settings.PreviewBackgroundColor;
            }

            EnsureRenderTexture(textureWidth, textureHeight);
            previewCamera.targetTexture = renderTexture;
            UpdateCameraTransform();
        }

        public void EnsureRenderTexture(int width, int height)
        {
            width = Mathf.Max(64, width);
            height = Mathf.Max(64, height);
            if (renderTexture != null && renderTexture.width == width && renderTexture.height == height)
            {
                return;
            }

            ReleaseRenderTexture();
            textureWidth = width;
            textureHeight = height;
            renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                hideFlags = HideFlags.DontSave,
                name = "CCS_EquipmentFitStudioPreviewRT",
            };
            renderTexture.Create();
            if (previewCamera != null)
            {
                previewCamera.targetTexture = renderTexture;
            }
        }

        public void RenderNow()
        {
            if (previewCamera == null)
            {
                return;
            }

            UpdateCameraTransform();
            previewCamera.Render();
        }

        public void HandleInput(Rect previewRect, Event currentEvent)
        {
            if (previewCamera == null || currentEvent == null || !previewRect.Contains(currentEvent.mousePosition))
            {
                return;
            }

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (currentEvent.button == 0)
                    {
                        isDraggingOrbit = true;
                        lastMousePosition = currentEvent.mousePosition;
                        currentEvent.Use();
                    }
                    else if (currentEvent.button == 2)
                    {
                        isDraggingPan = true;
                        lastMousePosition = currentEvent.mousePosition;
                        currentEvent.Use();
                    }
                    else if (currentEvent.button == 1)
                    {
                        isDraggingLook = true;
                        lastMousePosition = currentEvent.mousePosition;
                        currentEvent.Use();
                    }

                    break;

                case EventType.MouseUp:
                    isDraggingOrbit = false;
                    isDraggingPan = false;
                    isDraggingLook = false;
                    break;

                case EventType.MouseDrag:
                    Vector2 delta = currentEvent.mousePosition - lastMousePosition;
                    lastMousePosition = currentEvent.mousePosition;
                    if (isDraggingOrbit)
                    {
                        orbitYaw += delta.x * 0.35f;
                        orbitPitch -= delta.y * 0.35f;
                        orbitPitch = Mathf.Clamp(orbitPitch, -80f, 80f);
                        currentEvent.Use();
                    }
                    else if (isDraggingPan)
                    {
                        Vector3 right = previewCamera.transform.right;
                        Vector3 up = previewCamera.transform.up;
                        focusPoint -= (right * delta.x + up * delta.y) * 0.0025f * distance;
                        currentEvent.Use();
                    }
                    else if (isDraggingLook)
                    {
                        orbitYaw += delta.x * 0.35f;
                        orbitPitch -= delta.y * 0.35f;
                        orbitPitch = Mathf.Clamp(orbitPitch, -80f, 80f);
                        currentEvent.Use();
                    }

                    break;

                case EventType.ScrollWheel:
                    distance += currentEvent.delta.y * 0.08f;
                    distance = Mathf.Clamp(distance, 0.15f, 8f);
                    currentEvent.Use();
                    break;

                case EventType.KeyDown:
                    if (currentEvent.keyCode == KeyCode.F)
                    {
                        FrameCurrentFocus();
                        currentEvent.Use();
                    }

                    break;
            }
        }

        public void FrameTransform(Transform target, float padding = 1.2f)
        {
            if (target == null)
            {
                return;
            }

            focusPoint = target.position;
            distance = EstimateFrameDistance(target, padding);
            orbitPitch = 15f;
            UpdateCameraTransform();
        }

        public void SetFrameContext(GameObject playerRoot, string socketId)
        {
            framePlayerRoot = playerRoot;
            frameSocketId = socketId ?? string.Empty;
        }

        public void ResetCameraOrientation()
        {
            orbitYaw = 0f;
            orbitPitch = 15f;
            distance = 1.5f;
            UpdateCameraTransform();
        }

        public void FrameCurrentFocus()
        {
            if (framePlayerRoot == null)
            {
                return;
            }

            Transform target = ResolvePresetTarget(framePlayerRoot, CCS_EquipmentFitStudioCameraPreset.Frame, frameSocketId);
            if (target != null)
            {
                FrameTransform(target, 1.15f);
            }
        }

        public void ApplyPreset(CCS_EquipmentFitStudioCameraPreset preset, GameObject playerRoot, string socketId)
        {
            if (playerRoot == null)
            {
                return;
            }

            SetFrameContext(playerRoot, socketId);
            Transform target = ResolvePresetTarget(playerRoot, preset, socketId);
            if (target == null)
            {
                target = playerRoot.transform;
            }

            switch (preset)
            {
                case CCS_EquipmentFitStudioCameraPreset.FullBody:
                    focusPoint = playerRoot.transform.position + Vector3.up * 1.0f;
                    distance = 3.5f;
                    orbitPitch = 10f;
                    break;
                case CCS_EquipmentFitStudioCameraPreset.Back:
                    focusPoint = target.position;
                    distance = 1.8f;
                    orbitYaw = 180f;
                    orbitPitch = 5f;
                    break;
                case CCS_EquipmentFitStudioCameraPreset.UpperBody:
                    focusPoint = playerRoot.transform.position + Vector3.up * 1.25f;
                    distance = 1.4f;
                    orbitPitch = 5f;
                    orbitYaw = -25f;
                    break;
                case CCS_EquipmentFitStudioCameraPreset.RightShoulder:
                    focusPoint = target.position + Vector3.up * 0.15f;
                    distance = 0.9f;
                    orbitYaw = -35f;
                    orbitPitch = 8f;
                    break;
                case CCS_EquipmentFitStudioCameraPreset.WeaponCloseUp:
                    focusPoint = target.position + target.forward * 0.05f;
                    distance = 0.45f;
                    orbitPitch = 5f;
                    break;
                case CCS_EquipmentFitStudioCameraPreset.TriggerCloseUp:
                case CCS_EquipmentFitStudioCameraPreset.MuzzleView:
                    focusPoint = target.position + target.forward * 0.08f;
                    distance = 0.35f;
                    orbitPitch = 0f;
                    break;
                default:
                    FrameTransform(target, preset == CCS_EquipmentFitStudioCameraPreset.Frame ? 1.1f : 1.0f);
                    return;
            }

            UpdateCameraTransform();
        }

        public void DestroyCamera()
        {
            ReleaseRenderTexture();
            if (cameraRoot != null)
            {
                Object.DestroyImmediate(cameraRoot);
                cameraRoot = null;
                previewCamera = null;
            }
        }

        #endregion

        #region Private Methods

        private void UpdateCameraTransform()
        {
            if (previewCamera == null)
            {
                return;
            }

            Quaternion rotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
            previewCamera.transform.position = focusPoint + offset;
            previewCamera.transform.rotation = rotation;
        }

        private static float EstimateFrameDistance(Transform target, float padding)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return 1.2f * padding;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float radius = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            return Mathf.Max(0.25f, radius * 2.5f * padding);
        }

        private static Transform ResolvePresetTarget(
            GameObject playerRoot,
            CCS_EquipmentFitStudioCameraPreset preset,
            string socketId)
        {
            CCS_EquipmentSocketRegistry registry = playerRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            Transform visualRoot = FindDeepChild(playerRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            Transform ikRoot = visualRoot != null
                ? visualRoot.Find(CCS_EquipmentConstants.WeaponIkTargetsObjectName)
                : null;

            switch (preset)
            {
                case CCS_EquipmentFitStudioCameraPreset.RightHand:
                    return FindSocket(registry, CCS_EquipmentConstants.HandSocketRightId)
                        ?? FindIkTarget(ikRoot, CCS_EquipmentConstants.RightHandIkTargetObjectName);
                case CCS_EquipmentFitStudioCameraPreset.LeftHand:
                    return FindSocket(registry, CCS_EquipmentConstants.HandSocketLeftId)
                        ?? FindIkTarget(ikRoot, CCS_EquipmentConstants.LeftHandIkTargetObjectName);
                case CCS_EquipmentFitStudioCameraPreset.RightHip:
                    return FindSocket(registry, CCS_EquipmentConstants.HolsterSocketRightHipId);
                case CCS_EquipmentFitStudioCameraPreset.LeftHip:
                    return FindSocket(registry, CCS_EquipmentConstants.HolsterSocketLeftHipId);
                case CCS_EquipmentFitStudioCameraPreset.Back:
                    return FindSocket(registry, CCS_EquipmentConstants.BackSocketLongGunAId)
                        ?? FindSocket(registry, CCS_EquipmentConstants.BackSocketLongGunBId);
                default:
                    return FindSocket(registry, socketId) ?? playerRoot.transform;
            }
        }

        private static Transform FindSocket(CCS_EquipmentSocketRegistry registry, string socketId)
        {
            if (registry == null || string.IsNullOrEmpty(socketId))
            {
                return null;
            }

            return registry.TryGetSocket(socketId, out Transform socketTransform) ? socketTransform : null;
        }

        private static Transform FindIkTarget(Transform ikRoot, string targetName)
        {
            return ikRoot != null ? ikRoot.Find(targetName) : null;
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeepChild(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private void ReleaseRenderTexture()
        {
            if (renderTexture == null)
            {
                return;
            }

            if (previewCamera != null && previewCamera.targetTexture == renderTexture)
            {
                previewCamera.targetTexture = null;
            }

            renderTexture.Release();
            Object.DestroyImmediate(renderTexture);
            renderTexture = null;
        }

        #endregion
    }

    public enum CCS_EquipmentFitStudioCameraPreset
    {
        Frame = 0,
        FullBody = 1,
        RightHand = 2,
        LeftHand = 3,
        RightHip = 4,
        LeftHip = 5,
        Back = 6,
        TriggerCloseUp = 7,
        MuzzleView = 8,
        UpperBody = 9,
        RightShoulder = 10,
        WeaponCloseUp = 11,
    }
}
