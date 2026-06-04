using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BusinessPresenceMarker
// CATEGORY: Modules / Settlements / Runtime / BusinessPresence
// PURPOSE: Primitive placeholder marker that reflects business presence status.
// PLACEMENT: Child of CCS_BusinessPresenceAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — cube/sign style dev-readable visuals only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_BusinessPresenceMarker : MonoBehaviour
    {
        [SerializeField] private Color activeColor = new Color(0.25f, 0.85f, 0.35f, 1f);

        [SerializeField] private Color inactiveColor = new Color(0.85f, 0.75f, 0.2f, 1f);

        [SerializeField] private Color lockedColor = new Color(0.45f, 0.45f, 0.45f, 1f);

        [SerializeField] private Vector3 activeScale = new Vector3(1.2f, 1.4f, 1.2f);

        [SerializeField] private Vector3 inactiveScale = new Vector3(1f, 1f, 1f);

        [SerializeField] private Vector3 lockedScale = new Vector3(0.85f, 0.85f, 0.85f);

        private Renderer markerRenderer;

        private void Awake()
        {
            markerRenderer = GetComponent<Renderer>();
        }

        public void ApplyStatus(CCS_BusinessPresenceStatus status)
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
                case CCS_BusinessPresenceStatus.Active:
                    markerRenderer.sharedMaterial.color = activeColor;
                    transform.localScale = activeScale;
                    break;
                case CCS_BusinessPresenceStatus.Inactive:
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
