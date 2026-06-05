using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthAnchor
// CATEGORY: Modules / Settlements / Runtime / VisualGrowth
// PURPOSE: World anchor mapping settlement growth stage to marker visuals.
// PLACEMENT: Bootstrap scene children under settlement roots.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 — registers with CCS_SettlementVisualGrowthRuntimeBridge.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementVisualGrowthAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId = string.Empty;

        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private CCS_SettlementGrowthStage requiredGrowthStage = CCS_SettlementGrowthStage.Outpost;

        [SerializeField] private CCS_SettlementVisualGrowthMarkerType markerType =
            CCS_SettlementVisualGrowthMarkerType.Unknown;

        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private string businessId = string.Empty;

        [SerializeField] private string servicePointId = string.Empty;

        [SerializeField] private bool syncLinkedServicePointVisual;

        [SerializeField] private CCS_SettlementGrowthStage minimumServicePointGrowthStage =
            CCS_SettlementGrowthStage.Unknown;

        [SerializeField] private CCS_SettlementVisualGrowthMarker growthMarker;

        [SerializeField] private CCS_SettlementVisualGrowthLabel growthLabel;

        public string AnchorId => anchorId ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        public CCS_SettlementGrowthStage RequiredGrowthStage => requiredGrowthStage;

        public CCS_SettlementVisualGrowthMarkerType MarkerType => markerType;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName) ? markerType.ToString() : displayName;

        private void Awake()
        {
            EnsureComponents();
        }

        private void OnEnable()
        {
            CCS_SettlementVisualGrowthRuntimeBridge.RegisterAnchor(this);
            RefreshFromGrowthState();
        }

        private void OnDisable()
        {
            CCS_SettlementVisualGrowthRuntimeBridge.UnregisterAnchor(this);
        }

        public void RefreshFromGrowthState()
        {
            EnsureComponents();
            CCS_SettlementVisualGrowthStatus status = CCS_SettlementVisualGrowthRuntimeBridge.ResolveVisualStatus(
                SettlementId,
                RequiredGrowthStage);
            growthMarker?.ApplyStatus(status);
            growthLabel?.ApplyGrowthVisual(DisplayName, status);
            if (syncLinkedServicePointVisual)
            {
                ApplyLinkedServicePointVisual(status);
            }
        }

        private void EnsureComponents()
        {
            if (growthMarker == null)
            {
                growthMarker = GetComponentInChildren<CCS_SettlementVisualGrowthMarker>(true);
            }

            if (growthLabel == null)
            {
                growthLabel = GetComponentInChildren<CCS_SettlementVisualGrowthLabel>(true);
            }
        }

        private void ApplyLinkedServicePointVisual(CCS_SettlementVisualGrowthStatus status)
        {
            if (string.IsNullOrWhiteSpace(servicePointId))
            {
                return;
            }

            CCS_SettlementServicePoint[] servicePoints =
                GetComponentsInParent<CCS_SettlementServicePoint>(true);
            for (int index = 0; index < servicePoints.Length; index++)
            {
                CCS_SettlementServicePoint servicePoint = servicePoints[index];
                if (servicePoint != null
                    && string.Equals(servicePoint.ServicePointId, servicePointId, System.StringComparison.OrdinalIgnoreCase))
                {
                    servicePoint.ApplyVisualGrowthVisual(status, minimumServicePointGrowthStage);
                }
            }
        }
    }
}
