using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementSocialMarker
// CATEGORY: Modules / Settlements / Runtime / Social
// PURPOSE: Primitive placeholder marker for settlement social gathering areas.
// PLACEMENT: Child of CCS_SettlementSocialAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 — dev-readable fire/rail/camp markers only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementSocialMarker : MonoBehaviour
    {
        [SerializeField] private Color markerColor = new Color(0.95f, 0.45f, 0.2f, 1f);

        [SerializeField] private Vector3 markerScale = new Vector3(1.2f, 0.6f, 1.2f);

        private Renderer markerRenderer;

        private void Awake()
        {
            markerRenderer = GetComponent<Renderer>();
            ApplyMarker();
        }

        public void ApplyMarker()
        {
            if (markerRenderer == null)
            {
                markerRenderer = GetComponent<Renderer>();
            }

            if (markerRenderer == null)
            {
                return;
            }

            markerRenderer.sharedMaterial.color = markerColor;
            transform.localScale = markerScale;
        }
    }
}
