using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TrapPlacementPreview
// CATEGORY: Modules / Trapping / Runtime / Placement
// PURPOSE: Primitive placement preview for frontier trap placement mode.
// PLACEMENT: Spawned and driven by CCS_TrapService during placeable trap use.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Trapping
{
    public sealed class CCS_TrapPlacementPreview : MonoBehaviour
    {
        #region Variables

        private Renderer previewRenderer;
        private bool isVisible;

        #endregion

        #region Public Methods

        public void EnsurePreviewObject(PrimitiveType primitiveType)
        {
            if (previewRenderer != null)
            {
                return;
            }

            GameObject previewObject = GameObject.CreatePrimitive(primitiveType);
            previewObject.name = "CCS_TrapPlacementPreview";
            previewObject.transform.SetParent(transform, false);

            Collider collider = previewObject.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            previewRenderer = previewObject.GetComponent<Renderer>();
            SetVisible(false);
        }

        public void UpdatePreview(Vector3 worldPosition, Quaternion worldRotation, Vector3 localScale, bool isValid, Color validColor, Color invalidColor)
        {
            if (previewRenderer == null)
            {
                return;
            }

            transform.SetPositionAndRotation(worldPosition, worldRotation);
            previewRenderer.transform.localScale = localScale;
            previewRenderer.material.color = isValid ? validColor : invalidColor;
            SetVisible(true);
        }

        public void SetVisible(bool visible)
        {
            isVisible = visible;
            if (previewRenderer != null)
            {
                previewRenderer.enabled = visible;
            }
        }

        public bool IsVisible => isVisible;

        #endregion
    }
}
