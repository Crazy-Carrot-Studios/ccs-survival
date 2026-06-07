using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementEventAnchor
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: World anchor for settlement dynamic event presentation.
// PLACEMENT: Bootstrap scene children under settlement roots.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 — registers with CCS_SettlementEventAnchorRuntimeBridge.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementEventAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId = string.Empty;

        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private string idleDisplayName = "Event Area";

        [SerializeField] private CCS_SettlementEventMarker eventMarker;

        [SerializeField] private CCS_SettlementEventLabel eventLabel;

        public string AnchorId => anchorId ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        private void Awake()
        {
            EnsureComponents();
        }

        private void OnEnable()
        {
            EnsureComponents();
            CCS_SettlementEventAnchorRuntimeBridge.RegisterAnchor(this);
            RefreshPresentation();
        }

        private void OnDisable()
        {
            CCS_SettlementEventAnchorRuntimeBridge.UnregisterAnchor(this);
        }

        public void RefreshPresentation()
        {
            EnsureComponents();
            eventMarker?.ApplyMarker();

            if (CCS_SettlementEventRuntimeBridge.TryGetActiveEvent(
                    SettlementId,
                    out CCS_SettlementEventSnapshot snapshot)
                && snapshot != null
                && snapshot.IsValid
                && string.Equals(snapshot.EventMarkerAnchorId, AnchorId, System.StringComparison.OrdinalIgnoreCase))
            {
                eventLabel?.ApplyActiveEvent(snapshot.DisplayName);
                return;
            }

            eventLabel?.ApplyIdlePresentation(idleDisplayName);
        }

        private void EnsureComponents()
        {
            if (eventMarker == null)
            {
                eventMarker = GetComponentInChildren<CCS_SettlementEventMarker>(true);
            }

            if (eventLabel == null)
            {
                eventLabel = GetComponentInChildren<CCS_SettlementEventLabel>(true);
            }
        }
    }
}
