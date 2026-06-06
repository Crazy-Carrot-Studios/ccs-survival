using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementHousingMarker
// CATEGORY: Modules / Settlements / Runtime / Housing
// PURPOSE: Primitive placeholder marker that reflects settlement housing status.
// PLACEMENT: Child of CCS_SettlementHousingAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — dev-readable cube/house style markers only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementHousingMarker : MonoBehaviour
    {
        [SerializeField] private Color activeColor = new Color(0.35f, 0.75f, 0.95f, 1f);

        [SerializeField] private Color inactiveColor = new Color(0.85f, 0.75f, 0.35f, 1f);

        [SerializeField] private Color lockedColor = new Color(0.45f, 0.45f, 0.45f, 1f);

        [SerializeField] private Vector3 activeScale = new Vector3(1.4f, 1.2f, 1.4f);

        [SerializeField] private Vector3 inactiveScale = new Vector3(1.1f, 1f, 1.1f);

        [SerializeField] private Vector3 lockedScale = new Vector3(0.85f, 0.75f, 0.85f);

        private Renderer markerRenderer;

        private void Awake()
        {
            markerRenderer = GetComponent<Renderer>();
        }

        public void ApplyStatus(CCS_SettlementHousingStatus status)
        {
            if (markerRenderer == null)
            {
                markerRenderer = GetComponent<Renderer>();
            }

            if (markerRenderer == null)
            {
                return;
            }

            switch (status)
            {
                case CCS_SettlementHousingStatus.Active:
                    markerRenderer.sharedMaterial.color = activeColor;
                    transform.localScale = activeScale;
                    break;
                case CCS_SettlementHousingStatus.Inactive:
                    markerRenderer.sharedMaterial.color = inactiveColor;
                    transform.localScale = inactiveScale;
                    break;
                default:
                    markerRenderer.sharedMaterial.color = lockedColor;
                    transform.localScale = lockedScale;
                    break;
            }
        }
    }
}
