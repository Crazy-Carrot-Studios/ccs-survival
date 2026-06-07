using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementSocialAnchor
// CATEGORY: Modules / Settlements / Runtime / Social
// PURPOSE: World anchor for settlement social gathering areas during leisure periods.
// PLACEMENT: Bootstrap scene children under settlement roots.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 — registers with CCS_SettlementSocialRuntimeBridge.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementSocialAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId = string.Empty;

        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private CCS_SettlementSocialMarker socialMarker;

        [SerializeField] private CCS_SettlementSocialLabel socialLabel;

        public string AnchorId => anchorId ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        private void Awake()
        {
            EnsureComponents();
        }

        private void OnEnable()
        {
            EnsureComponents();
            CCS_SettlementSocialRuntimeBridge.RegisterAnchor(this);
            RefreshPresentation();
        }

        private void OnDisable()
        {
            CCS_SettlementSocialRuntimeBridge.UnregisterAnchor(this);
        }

        public void RefreshPresentation()
        {
            EnsureComponents();
            socialMarker?.ApplyMarker();
            socialLabel?.ApplySocialArea(DisplayName);
        }

        private void EnsureComponents()
        {
            if (socialMarker == null)
            {
                socialMarker = GetComponentInChildren<CCS_SettlementSocialMarker>(true);
            }

            if (socialLabel == null)
            {
                socialLabel = GetComponentInChildren<CCS_SettlementSocialLabel>(true);
            }
        }
    }
}
