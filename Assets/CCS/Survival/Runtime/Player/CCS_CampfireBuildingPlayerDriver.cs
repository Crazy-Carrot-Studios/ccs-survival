using CCS.Modules.Building;
using CCS.Modules.CharacterController;
using CCS.Modules.Cooking;
using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_CampfireBuildingPlayerDriver
// CATEGORY: Survival / Runtime / Player
// PURPOSE: Lets the player place a campfire from a kit using the building framework.
// PLACEMENT: PF_CCS_Player alongside CCS_InteractionPlayerDriver.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Uses B to toggle placement mode and Interact to confirm placement.
// =============================================================================

namespace CCS.Survival.Player
{
    [DefaultExecutionOrder(225)]
    public sealed class CCS_CampfireBuildingPlayerDriver : MonoBehaviour
    {
        #region Variables

        [Header("Campfire Placement")]
        [Tooltip("Camera used for forward placement raycasts. Defaults to child camera.")]
        [SerializeField] private Camera placementCamera;

        [Tooltip("Maximum distance for campfire placement raycasts.")]
        [SerializeField] private float placementRayDistance = 8f;

        [Tooltip("Layer mask used for campfire placement raycasts.")]
        [SerializeField] private LayerMask placementLayerMask = Physics.DefaultRaycastLayers;

        private CCS_CharacterInputActionProvider inputProvider;
        private CCS_CampfireService campfireService;
        private CCS_BuildingPlacementService placementService;
        private bool servicesResolved;
        private bool isCampfirePlacementActive;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            inputProvider = GetComponent<CCS_CharacterInputActionProvider>();

            if (placementCamera == null)
            {
                placementCamera = GetComponentInChildren<Camera>();
            }
        }

        private void Update()
        {
            if (!ResolveServices())
            {
                return;
            }

            HandleCampfirePlacementToggle();

            if (!isCampfirePlacementActive)
            {
                return;
            }

            UpdateCampfirePreview();
            HandleCampfirePlacementConfirm();
        }

        #endregion

        #region Private Methods

        private bool ResolveServices()
        {
            if (servicesResolved && campfireService != null && placementService != null)
            {
                return true;
            }

            servicesResolved = CCS_CookingRuntimeBridge.TryGetCampfireService(out campfireService)
                && CCS_BuildingRuntimeBridge.TryGetBuildingPlacementService(out placementService)
                && campfireService != null
                && placementService != null
                && campfireService.IsInitialized
                && placementService.IsInitialized;

            return servicesResolved;
        }

        private void HandleCampfirePlacementToggle()
        {
            if (Keyboard.current == null || !Keyboard.current.bKey.wasPressedThisFrame)
            {
                return;
            }

            if (isCampfirePlacementActive)
            {
                ExitCampfirePlacementMode();
                return;
            }

            if (campfireService.ActiveProfile?.CampfireBuildingPiece == null)
            {
                return;
            }

            if (!placementService.SetActiveDefinition(campfireService.ActiveProfile.CampfireBuildingPiece))
            {
                return;
            }

            isCampfirePlacementActive = true;
        }

        private void UpdateCampfirePreview()
        {
            if (placementCamera == null || !TryGetPlacementPoint(out Vector3 placementPoint))
            {
                return;
            }

            placementService.UpdatePreview(placementPoint, Quaternion.identity);
        }

        private void HandleCampfirePlacementConfirm()
        {
            if (inputProvider == null || !inputProvider.InteractPressedThisFrame)
            {
                return;
            }

            if (!TryGetPlacementPoint(out Vector3 placementPoint))
            {
                return;
            }

            if (campfireService.TryPlaceCampfireFromKit(placementPoint, Quaternion.identity))
            {
                ExitCampfirePlacementMode();
            }
        }

        private bool TryGetPlacementPoint(out Vector3 placementPoint)
        {
            placementPoint = Vector3.zero;

            if (placementCamera == null)
            {
                return false;
            }

            Transform cameraTransform = placementCamera.transform;
            if (!Physics.Raycast(
                    cameraTransform.position,
                    cameraTransform.forward,
                    out RaycastHit hit,
                    placementRayDistance,
                    placementLayerMask,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            placementPoint = hit.point;
            return true;
        }

        private void ExitCampfirePlacementMode()
        {
            isCampfirePlacementActive = false;
            placementService.ExitPlacementMode("Campfire placement cancelled.");
        }

        #endregion
    }
}
