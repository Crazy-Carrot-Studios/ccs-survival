using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BusinessPresenceAnchor
// CATEGORY: Modules / Settlements / Runtime / BusinessPresence
// PURPOSE: World anchor mapping settlement business activation to marker visuals.
// PLACEMENT: Bootstrap scene children under settlement roots.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — registers with CCS_BusinessPresenceService at runtime.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_BusinessPresenceAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId = string.Empty;

        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private string businessId = string.Empty;

        [SerializeField] private CCS_BusinessType businessType = CCS_BusinessType.Unknown;

        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private bool syncLinkedServicePointVisual = true;

        [SerializeField] private CCS_BusinessPresenceMarker presenceMarker;

        [SerializeField] private CCS_BusinessPresenceLabel presenceLabel;

        public string AnchorId => anchorId ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        public string BusinessId => businessId ?? string.Empty;

        public CCS_BusinessType BusinessType => businessType;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName) ? businessType.ToString() : displayName;

        private void Awake()
        {
            EnsureComponents();
        }

        private void OnEnable()
        {
            CCS_BusinessPresenceRuntimeBridge.RegisterAnchor(this);
            RefreshFromBusinessState();
        }

        private void OnDisable()
        {
            CCS_BusinessPresenceRuntimeBridge.UnregisterAnchor(this);
        }

        public void RefreshFromBusinessState()
        {
            EnsureComponents();
            CCS_BusinessPresenceStatus status = CCS_BusinessPresenceRuntimeBridge.ResolvePresenceStatus(
                SettlementId,
                BusinessType);
            presenceMarker?.ApplyStatus(status);
            presenceLabel?.ApplyPresence(DisplayName, status);
            if (syncLinkedServicePointVisual)
            {
                ApplyLinkedServicePointVisual(status);
            }
        }

        private void EnsureComponents()
        {
            if (presenceMarker == null)
            {
                presenceMarker = GetComponentInChildren<CCS_BusinessPresenceMarker>(true);
            }

            if (presenceLabel == null)
            {
                presenceLabel = GetComponentInChildren<CCS_BusinessPresenceLabel>(true);
            }
        }

        private void ApplyLinkedServicePointVisual(CCS_BusinessPresenceStatus status)
        {
            CCS_SettlementServicePointType servicePointType = MapBusinessTypeToServicePointType(BusinessType);
            if (servicePointType == CCS_SettlementServicePointType.Other)
            {
                return;
            }

            CCS_SettlementServicePoint[] servicePoints =
                GetComponentsInParent<CCS_SettlementServicePoint>(true);
            for (int index = 0; index < servicePoints.Length; index++)
            {
                CCS_SettlementServicePoint servicePoint = servicePoints[index];
                if (servicePoint != null && servicePoint.ServicePointType == servicePointType)
                {
                    servicePoint.ApplyBusinessPresenceVisual(status);
                }
            }
        }

        private static CCS_SettlementServicePointType MapBusinessTypeToServicePointType(CCS_BusinessType type)
        {
            return type switch
            {
                CCS_BusinessType.GeneralStore => CCS_SettlementServicePointType.GeneralStore,
                CCS_BusinessType.Stable => CCS_SettlementServicePointType.Stable,
                CCS_BusinessType.Gunsmith => CCS_SettlementServicePointType.Gunsmith,
                CCS_BusinessType.Bank => CCS_SettlementServicePointType.Bank,
                CCS_BusinessType.ContractOffice => CCS_SettlementServicePointType.ContractBoard,
                _ => CCS_SettlementServicePointType.Other
            };
        }
    }
}
