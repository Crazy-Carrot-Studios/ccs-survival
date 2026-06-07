using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementEventMarker
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: Primitive placeholder marker for active settlement events.
// PLACEMENT: Child of CCS_SettlementEventAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 — dev-readable event markers only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementEventMarker : MonoBehaviour
    {
        [SerializeField] private Color markerColor = new Color(0.35f, 0.75f, 1f, 1f);

        [SerializeField] private Vector3 markerScale = new Vector3(1.4f, 0.8f, 1.4f);

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
