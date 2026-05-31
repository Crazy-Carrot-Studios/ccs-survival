using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ResourceHarvestingTestHarness
// CATEGORY: Modules / WorldResources / Runtime / Testing
// PURPOSE: Development-only harness that drives interaction scan and harvest attempts.
// PLACEMENT: Bootstrap verification scenes only. Disable for shipping builds.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Uses the same service path as player interaction. Not final gameplay input.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    [DefaultExecutionOrder(250)]
    public sealed class CCS_ResourceHarvestingTestHarness : MonoBehaviour
    {
        #region Variables

        [Header("Development Testing")]
        [Tooltip("When enabled, the harness scans from the main camera and triggers harvest attempts.")]
        [SerializeField] private bool enableHarness = true;

        [Tooltip("Seconds between automated interaction attempts.")]
        [SerializeField] private float interactIntervalSeconds = 3f;

        [Tooltip("Optional camera used for interaction scanning. Defaults to Camera.main.")]
        [SerializeField] private Camera scanCamera;

        private float nextInteractTime;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!enableHarness)
            {
                return;
            }

            Camera activeCamera = scanCamera != null ? scanCamera : Camera.main;
            if (activeCamera == null)
            {
                return;
            }

            if (!CCS_WorldResourceRuntimeBridge.TryGetInteractionService(out CCS_InteractionService interactionService)
                || !interactionService.IsInitialized)
            {
                return;
            }

            interactionService.TickScan(activeCamera.transform.position, activeCamera.transform.forward);

            if (Time.time < nextInteractTime)
            {
                return;
            }

            nextInteractTime = Time.time + interactIntervalSeconds;

            if (!interactionService.HasCurrentTarget)
            {
                Debug.Log("[CCS_ResourceHarvestingTestHarness] No interactable resource target in scan.");
                return;
            }

            bool interactionSucceeded = interactionService.RequestInteraction();
            Debug.Log(interactionSucceeded
                ? "[CCS_ResourceHarvestingTestHarness] Interaction harvest request succeeded."
                : "[CCS_ResourceHarvestingTestHarness] Interaction harvest request failed.");
        }

        #endregion
    }
}
