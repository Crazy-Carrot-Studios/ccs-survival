using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestDetectionCubeSceneBootstrap
// CATEGORY: Modules / Interaction / Runtime / Components
// PURPOSE: Ensures a detection test cube exists in Master Test during solo play.
// PLACEMENT: CCS_TestDetectionCubeSceneBootstrap object in Master Test scene.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Runtime fallback when the scene cube was not baked by the editor builder.
// =============================================================================

namespace CCS.Modules.Interaction
{
    [DefaultExecutionOrder(-50)]
    public sealed class CCS_TestDetectionCubeSceneBootstrap : MonoBehaviour
    {
        #region Unity Callbacks

        private void Start()
        {
            if (IsNetworkSessionActive())
            {
                return;
            }

            if (HasDetectionTargetInScene())
            {
                return;
            }

            Transform spawnOrigin = CCS_TestDetectionCubeUtility.FindSpawnOrigin();
            if (spawnOrigin == null)
            {
                Debug.LogError(
                    "[Interaction Test] Detection cube bootstrap could not find TP_Spawn_Host.",
                    this);
                return;
            }

            int interactableLayer = LayerMask.NameToLayer(CCS_InteractionConstants.InteractableLayerName);
            if (interactableLayer < 0)
            {
                Debug.LogError(
                    "[Interaction Test] Detection cube bootstrap could not resolve Interactable layer.",
                    this);
                return;
            }

            GameObject cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubeObject.transform.SetPositionAndRotation(
                CCS_TestDetectionCubeUtility.GetDetectionCubeWorldPosition(spawnOrigin),
                spawnOrigin.rotation);
            cubeObject.transform.localScale = Vector3.one;

            CCS_TestDetectionCubeUtility.TryConfigureDetectionCube(cubeObject, interactableLayer);
            Debug.Log("[Interaction Test] Detection cube created for solo test.", cubeObject);
        }

        #endregion

        #region Private Methods

        private static bool HasDetectionTargetInScene()
        {
            CCS_InteractableLabelTarget[] targets =
                FindObjectsByType<CCS_InteractableLabelTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            return targets.Length > 0;
        }

        private static bool IsNetworkSessionActive()
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            return networkManager != null && networkManager.IsListening;
        }

        #endregion
    }
}
