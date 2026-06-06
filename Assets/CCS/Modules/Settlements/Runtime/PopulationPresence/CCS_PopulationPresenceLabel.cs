using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PopulationPresenceLabel
// CATEGORY: Modules / Settlements / Runtime / PopulationPresence
// PURPOSE: Dev-readable label for workforce category and actor counts.
// PLACEMENT: Child of CCS_PopulationPresenceAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 — optional debug display for playtesting.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_PopulationPresenceLabel : MonoBehaviour
    {
        [SerializeField] private float labelHeight = 2.4f;

        private TextMesh textMesh;

        private void Awake()
        {
            EnsureTextMesh();
        }

        public void ApplyPresence(
            string settlementId,
            CCS_SettlementPopulationCategory category,
            int sourceCount,
            int visibleCount)
        {
            EnsureTextMesh();
            if (textMesh == null)
            {
                return;
            }

            textMesh.text =
                $"{category}\n{settlementId}\nSource: {sourceCount}\nVisible: {visibleCount}";
        }

        private void EnsureTextMesh()
        {
            textMesh = CCS_SettlementMarkerLabelUtility.EnsureTextMeshOnHost(this, ref textMesh);
            CCS_SettlementMarkerLabelUtility.ApplyStandardLayout(transform, textMesh, labelHeight);
            textMesh.color = new Color(0.9f, 0.92f, 0.95f, 1f);
        }
    }
}
