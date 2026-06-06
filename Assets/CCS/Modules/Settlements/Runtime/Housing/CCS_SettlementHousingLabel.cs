using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementHousingLabel
// CATEGORY: Modules / Settlements / Runtime / Housing
// PURPOSE: World-space label for housing name, capacity, and activation status.
// PLACEMENT: Child of CCS_SettlementHousingAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — Active / Inactive / Locked dev-readable text.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementHousingLabel : MonoBehaviour
    {
        [SerializeField] private float labelHeight = 2.4f;

        [SerializeField] private Color activeColor = new Color(0.35f, 0.95f, 1f, 1f);

        [SerializeField] private Color inactiveColor = new Color(1f, 0.9f, 0.35f, 1f);

        [SerializeField] private Color lockedColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        private TextMesh textMesh;

        private void Awake()
        {
            EnsureLabel();
        }

        public void ApplyHousing(string displayName, int capacityContribution, CCS_SettlementHousingStatus status)
        {
            EnsureLabel();
            if (textMesh == null)
            {
                return;
            }

            string statusText = status switch
            {
                CCS_SettlementHousingStatus.Active => "Active",
                CCS_SettlementHousingStatus.Inactive => "Inactive",
                _ => "Locked"
            };

            textMesh.text = $"{displayName}\nCap +{capacityContribution} — {statusText}";
            textMesh.color = status switch
            {
                CCS_SettlementHousingStatus.Active => activeColor,
                CCS_SettlementHousingStatus.Inactive => inactiveColor,
                _ => lockedColor
            };
        }

        private void EnsureLabel()
        {
            textMesh = CCS_SettlementMarkerLabelUtility.EnsureTextMeshOnHost(this, ref textMesh);
            CCS_SettlementMarkerLabelUtility.ApplyStandardLayout(transform, textMesh, labelHeight);
        }
    }
}
