using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraRelativeDirectionUtility
// CATEGORY: Modules / CharacterController / Runtime / Utilities
// PURPOSE: Generic and module-local camera direction helpers.
// PLACEMENT: Static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: CCS_CameraDirectionUtility is a framework extraction candidate.
//        CCS_CharacterCameraRelativeDirectionUtility is the module wrapper.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CameraDirectionUtility
    {
        #region Public Methods

        public static Camera ResolveActiveCamera(Camera preferredCamera = null)
        {
            return preferredCamera != null ? preferredCamera : Camera.main;
        }

        public static Vector3 GetPlanarForward(Camera preferredCamera = null, Transform fallbackTransform = null)
        {
            Camera activeCamera = ResolveActiveCamera(preferredCamera);
            if (activeCamera != null)
            {
                return ProjectOntoXZ(
                    activeCamera.transform.forward,
                    fallbackTransform != null ? fallbackTransform.forward : Vector3.forward);
            }

            if (fallbackTransform != null)
            {
                return ProjectOntoXZ(fallbackTransform.forward, Vector3.forward);
            }

            return Vector3.forward;
        }

        public static Vector3 GetPlanarRight(Camera preferredCamera = null, Transform fallbackTransform = null)
        {
            Camera activeCamera = ResolveActiveCamera(preferredCamera);
            if (activeCamera != null)
            {
                return ProjectOntoXZ(
                    activeCamera.transform.right,
                    fallbackTransform != null ? fallbackTransform.right : Vector3.right);
            }

            if (fallbackTransform != null)
            {
                return ProjectOntoXZ(fallbackTransform.right, Vector3.right);
            }

            return Vector3.right;
        }

        public static float GetYawDegrees(Camera preferredCamera = null)
        {
            Camera activeCamera = ResolveActiveCamera(preferredCamera);
            return activeCamera != null ? activeCamera.transform.eulerAngles.y : 0f;
        }

        public static float GetPitchDegrees(Camera preferredCamera = null)
        {
            Camera activeCamera = ResolveActiveCamera(preferredCamera);
            if (activeCamera == null)
            {
                return 0f;
            }

            float pitch = activeCamera.transform.eulerAngles.x;
            return pitch > 180f ? pitch - 360f : pitch;
        }

        #endregion

        #region Private Methods

        private static Vector3 ProjectOntoXZ(Vector3 direction, Vector3 fallback)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                return direction.normalized;
            }

            fallback.y = 0f;
            return fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector3.forward;
        }

        #endregion
    }

    public static class CCS_CharacterCameraRelativeDirectionUtility
    {
        #region Public Methods

        public static Vector3 GetPlanarForward(Transform fallbackTransform)
        {
            return CCS_CameraDirectionUtility.GetPlanarForward(null, fallbackTransform);
        }

        public static Vector3 GetPlanarRight(Transform fallbackTransform)
        {
            return CCS_CameraDirectionUtility.GetPlanarRight(null, fallbackTransform);
        }

        #endregion
    }
}
