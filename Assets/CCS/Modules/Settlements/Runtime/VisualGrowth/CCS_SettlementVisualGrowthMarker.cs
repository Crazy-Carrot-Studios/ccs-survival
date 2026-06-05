using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthMarker
// CATEGORY: Modules / Settlements / Runtime / VisualGrowth
// PURPOSE: Primitive placeholder marker reflecting settlement growth stage status.
// PLACEMENT: Child of CCS_SettlementVisualGrowthAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 — cube/sign/crate style dev-readable visuals only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementVisualGrowthMarker : MonoBehaviour
    {
        [SerializeField] private Color activeColor = new Color(0.3f, 0.75f, 0.95f, 1f);

        [SerializeField] private Color inactiveColor = new Color(0.75f, 0.65f, 0.25f, 1f);

        [SerializeField] private Color lockedColor = new Color(0.4f, 0.4f, 0.42f, 1f);

        [SerializeField] private Vector3 activeScale = new Vector3(1.15f, 1.35f, 1.15f);

        [SerializeField] private Vector3 inactiveScale = new Vector3(1f, 1f, 1f);

        [SerializeField] private Vector3 lockedScale = new Vector3(0.8f, 0.8f, 0.8f);

        private Renderer markerRenderer;

        private void Awake()
        {
            markerRenderer = GetComponent<Renderer>();
        }

        public void ApplyStatus(CCS_SettlementVisualGrowthStatus status)
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
                case CCS_SettlementVisualGrowthStatus.Active:
                    markerRenderer.sharedMaterial.color = activeColor;
                    transform.localScale = activeScale;
                    gameObject.SetActive(true);
                    break;
                case CCS_SettlementVisualGrowthStatus.Inactive:
                    markerRenderer.sharedMaterial.color = inactiveColor;
                    transform.localScale = inactiveScale;
                    gameObject.SetActive(true);
                    break;
                default:
                    markerRenderer.sharedMaterial.color = lockedColor;
                    transform.localScale = lockedScale;
                    gameObject.SetActive(true);
                    break;
            }
        }
    }
}
