using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementSocialLabel
// CATEGORY: Modules / Settlements / Runtime / Social
// PURPOSE: World-space label for settlement social gathering areas.
// PLACEMENT: Child of CCS_SettlementSocialAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 — dev-readable social area names only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementSocialLabel : MonoBehaviour
    {
        [SerializeField] private float labelHeight = 2.2f;

        [SerializeField] private Color labelColor = new Color(1f, 0.75f, 0.35f, 1f);

        private TextMesh textMesh;

        private void Awake()
        {
            EnsureLabel();
        }

        public void ApplySocialArea(string displayName)
        {
            EnsureLabel();
            if (textMesh == null)
            {
                return;
            }

            textMesh.text = $"{displayName}\nSocial Area";
            textMesh.color = labelColor;
        }

        private void EnsureLabel()
        {
            textMesh = CCS_SettlementMarkerLabelUtility.EnsureTextMeshOnHost(this, ref textMesh);
            CCS_SettlementMarkerLabelUtility.ApplyStandardLayout(transform, textMesh, labelHeight);
        }
    }
}
