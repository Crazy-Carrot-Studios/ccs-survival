using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementEventLabel
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: World-space label for active settlement events.
// PLACEMENT: Child of CCS_SettlementEventAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 — shows Current Event display name when active.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementEventLabel : MonoBehaviour
    {
        [SerializeField] private float labelHeight = 2.4f;

        [SerializeField] private Color labelColor = new Color(0.55f, 0.9f, 1f, 1f);

        private TextMesh textMesh;

        private void Awake()
        {
            EnsureLabel();
        }

        public void ApplyIdlePresentation(string idleName = "Event Area")
        {
            EnsureLabel();
            if (textMesh == null)
            {
                return;
            }

            textMesh.text = string.IsNullOrWhiteSpace(idleName) ? "Event Area" : idleName;
            textMesh.color = labelColor;
        }

        public void ApplyActiveEvent(string displayName)
        {
            EnsureLabel();
            if (textMesh == null)
            {
                return;
            }

            textMesh.text = $"Current Event:\n{displayName}";
            textMesh.color = labelColor;
        }

        private void EnsureLabel()
        {
            textMesh = CCS_SettlementMarkerLabelUtility.EnsureTextMeshOnHost(this, ref textMesh);
            CCS_SettlementMarkerLabelUtility.ApplyStandardLayout(transform, textMesh, labelHeight);
        }
    }
}
