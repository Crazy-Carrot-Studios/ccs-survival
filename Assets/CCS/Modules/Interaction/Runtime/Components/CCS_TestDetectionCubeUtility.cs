using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestDetectionCubeUtility
// CATEGORY: Modules / Interaction / Runtime / Components
// PURPOSE: Backward-compatible helpers for the Master Test pickup detection cube.
// PLACEMENT: Runtime static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Delegates to CCS_TestInteractableUtility.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public static class CCS_TestDetectionCubeUtility
    {
        #region Public Methods

        public static Transform FindSpawnOrigin()
        {
            return CCS_TestInteractableUtility.FindSpawnOrigin();
        }

        public static Vector3 GetDetectionCubeWorldPosition(Transform spawnOrigin)
        {
            return CCS_TestInteractableUtility.GetDetectionCubeWorldPosition(spawnOrigin);
        }

        public static bool TryConfigureDetectionCube(GameObject cubeObject, int interactableLayer)
        {
            return CCS_TestInteractableUtility.TryConfigurePickupCube(cubeObject, interactableLayer);
        }

        #endregion
    }
}
