using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementHousingAnchor
// CATEGORY: Modules / Settlements / Runtime / Housing
// PURPOSE: World anchor mapping settlement housing activation to marker visuals.
// PLACEMENT: Bootstrap scene children under settlement roots.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — registers with CCS_SettlementHousingRuntimeBridge.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementHousingAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId = string.Empty;

        [SerializeField] private string housingId = string.Empty;

        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private int capacityContribution = 1;

        [SerializeField] private CCS_SettlementHousingMarker housingMarker;

        [SerializeField] private CCS_SettlementHousingLabel housingLabel;

        public string AnchorId => anchorId ?? string.Empty;

        public string HousingId => housingId ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public int CapacityContribution => capacityContribution;

        private void Awake()
        {
            EnsureComponents();
        }

        private void OnEnable()
        {
            CCS_SettlementHousingRuntimeBridge.RegisterAnchor(this);
            RefreshFromHousingState();
        }

        private void OnDisable()
        {
            CCS_SettlementHousingRuntimeBridge.UnregisterAnchor(this);
        }

        public void RefreshFromHousingState()
        {
            EnsureComponents();
            CCS_SettlementHousingStatus status = CCS_SettlementHousingRuntimeBridge.ResolveHousingStatus(
                SettlementId,
                HousingId);
            housingMarker?.ApplyStatus(status);
            housingLabel?.ApplyHousing(DisplayName, CapacityContribution, status);
        }

        private void EnsureComponents()
        {
            if (housingMarker == null)
            {
                housingMarker = GetComponentInChildren<CCS_SettlementHousingMarker>(true);
            }

            if (housingLabel == null)
            {
                housingLabel = GetComponentInChildren<CCS_SettlementHousingLabel>(true);
            }
        }
    }
}
