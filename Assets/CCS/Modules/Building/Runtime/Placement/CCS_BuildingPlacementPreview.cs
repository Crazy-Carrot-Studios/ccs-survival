using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingPlacementPreview
// CATEGORY: Modules / Building / Runtime / Placement
// PURPOSE: Development-only primitive preview for active building placement.
// PLACEMENT: Child of CCS_BuildingTestArea in bootstrap verification scenes.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Cube primitive only. Visible while placement mode is active.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingPlacementPreview : MonoBehaviour
    {
        #region Variables

        [Header("Preview")]
        [Tooltip("Local scale applied to the preview cube.")]
        [SerializeField] private Vector3 previewScale = Vector3.one;

        private GameObject previewObject;
        private CCS_BuildingPlacementService placementService;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            EnsurePreviewObject();
            HidePreview();
        }

        private void Update()
        {
            if (!TryBindPlacementService())
            {
                HidePreview();
                return;
            }

            CCS_BuildingPlacementSnapshot snapshot = placementService.GetSnapshot();
            if (!snapshot.IsPlacementModeActive || !snapshot.IsPlacementValid)
            {
                HidePreview();
                return;
            }

            ShowPreview(snapshot.PreviewPosition, snapshot.PreviewRotation);
        }

        private void OnDisable()
        {
            HidePreview();
        }

        #endregion

        #region Private Methods

        private bool TryBindPlacementService()
        {
            if (placementService != null && placementService.IsInitialized)
            {
                return true;
            }

            if (!CCS_BuildingRuntimeBridge.TryGetBuildingPlacementService(out placementService)
                || placementService == null
                || !placementService.IsInitialized)
            {
                placementService = null;
                return false;
            }

            return true;
        }

        private void EnsurePreviewObject()
        {
            if (previewObject != null)
            {
                return;
            }

            previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            previewObject.name = "CCS_BuildingPlacementPreview";
            previewObject.transform.SetParent(transform, false);
            previewObject.transform.localScale = previewScale;

            Collider previewCollider = previewObject.GetComponent<Collider>();
            if (previewCollider != null)
            {
                previewCollider.enabled = false;
            }
        }

        private void ShowPreview(Vector3 position, Quaternion rotation)
        {
            EnsurePreviewObject();
            previewObject.SetActive(true);
            previewObject.transform.SetPositionAndRotation(position, rotation);
        }

        private void HidePreview()
        {
            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }
        }

        #endregion
    }
}
