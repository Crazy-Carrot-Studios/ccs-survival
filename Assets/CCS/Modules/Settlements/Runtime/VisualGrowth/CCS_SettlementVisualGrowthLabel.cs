using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthLabel
// CATEGORY: Modules / Settlements / Runtime / VisualGrowth
// PURPOSE: TextMesh label for settlement growth marker name and stage status.
// PLACEMENT: Child of CCS_SettlementVisualGrowthAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 — dev-readable labels only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementVisualGrowthLabel : MonoBehaviour
    {
        [SerializeField] private float labelHeight = 2.2f;

        private TextMesh textMesh;

        private void Awake()
        {
            EnsureTextMesh();
        }

        public void ApplyGrowthVisual(string displayName, CCS_SettlementVisualGrowthStatus status)
        {
            EnsureTextMesh();
            if (textMesh == null)
            {
                return;
            }

            string statusLabel = status switch
            {
                CCS_SettlementVisualGrowthStatus.Active => "Active",
                CCS_SettlementVisualGrowthStatus.Inactive => "Inactive",
                _ => "Locked"
            };

            textMesh.text = $"{displayName}\n{statusLabel}";
            textMesh.color = status == CCS_SettlementVisualGrowthStatus.Active
                ? new Color(0.85f, 0.95f, 1f, 1f)
                : new Color(0.75f, 0.75f, 0.75f, 1f);
        }

        private void EnsureTextMesh()
        {
            textMesh = CCS_SettlementMarkerLabelUtility.EnsureTextMeshOnHost(this, ref textMesh);
            CCS_SettlementMarkerLabelUtility.ApplyStandardLayout(transform, textMesh, labelHeight);
        }
    }
}
