using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterMovementCameraContext
// CATEGORY: Modules / CharacterController / Runtime / Utilities
// PURPOSE: Process-local bound scene camera used for camera-relative movement.
// PLACEMENT: Static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Avoids Camera.main when multiple cameras exist in network test scenes.
//        Registered by the scene camera rig bound to the local owned player.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CharacterMovementCameraContext
    {
        #region Variables

        private static Camera activeMovementCamera;

        #endregion

        #region Properties

        public static bool HasActiveCamera => activeMovementCamera != null && activeMovementCamera.isActiveAndEnabled;

        public static Camera ActiveCamera => activeMovementCamera;

        #endregion

        #region Public Methods

        public static void Register(Camera camera)
        {
            activeMovementCamera = camera;
        }

        public static void Clear(Camera camera)
        {
            if (activeMovementCamera == camera)
            {
                activeMovementCamera = null;
            }
        }

        public static Vector3 GetPlanarForward()
        {
            return HasActiveCamera
                ? CCS_CameraDirectionUtility.GetPlanarForward(activeMovementCamera)
                : CCS_CameraDirectionUtility.GetPlanarForward();
        }

        public static Vector3 GetPlanarRight()
        {
            return HasActiveCamera
                ? CCS_CameraDirectionUtility.GetPlanarRight(activeMovementCamera)
                : CCS_CameraDirectionUtility.GetPlanarRight();
        }

        public static float GetYawDegrees()
        {
            return HasActiveCamera
                ? CCS_CameraDirectionUtility.GetYawDegrees(activeMovementCamera)
                : CCS_CameraDirectionUtility.GetYawDegrees();
        }

        public static float GetPitchDegrees()
        {
            return HasActiveCamera
                ? CCS_CameraDirectionUtility.GetPitchDegrees(activeMovementCamera)
                : CCS_CameraDirectionUtility.GetPitchDegrees();
        }

        #endregion
    }
}
