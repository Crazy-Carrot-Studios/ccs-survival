using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SharedDualAimPointRegistry
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Runtime read-only access to the shared dual-aim world point.
// PLACEMENT: Static registry updated by CCS_SharedDualAimPointPresenter.
// AUTHOR: James Schilz
// CREATED: 2026-07-02
// NOTES: Visual-only. Single source for arm bias and shared aim reticle projection.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_SharedDualAimPointRegistry
    {
        public static bool IsActive { get; private set; }

        public static Vector3 WorldPosition { get; private set; }

        public static Vector3 CameraForward { get; private set; }

        public static Vector3 CameraPosition { get; private set; }

        public static void Clear()
        {
            IsActive = false;
            WorldPosition = Vector3.zero;
            CameraForward = Vector3.forward;
            CameraPosition = Vector3.zero;
        }

        public static void UpdateState(
            bool isActive,
            Vector3 worldPosition,
            Vector3 cameraPosition,
            Vector3 cameraForward)
        {
            IsActive = isActive;
            WorldPosition = worldPosition;
            CameraPosition = cameraPosition;
            CameraForward = cameraForward.sqrMagnitude > 0.0001f
                ? cameraForward.normalized
                : Vector3.forward;
        }
    }
}
