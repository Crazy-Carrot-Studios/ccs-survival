using CCS.Modules.Economy;
using CCS.Modules.Interaction;
using CCS.Survival;
using CCS.Survival.Player;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementLocation
// CATEGORY: Modules / Settlements / Runtime / Components
// PURPOSE: World settlement root that discovers the location when the player enters range.
// PLACEMENT: CCS_TestTradingPost bootstrap object.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: No final town art; primitive bootstrap placement only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementLocation : MonoBehaviour
    {
        #region Variables

        [Header("Settlement")]
        [SerializeField] private CCS_SettlementDefinition settlementDefinition;

        [Header("Discovery")]
        [SerializeField] private float discoverRadius = 12f;

        [SerializeField] private bool autoDiscoverOnProximity = true;

        private bool hasDiscoveredThisSession;

        #endregion

        #region Properties

        public CCS_SettlementDefinition SettlementDefinition => settlementDefinition;

        public float DiscoverRadius => discoverRadius < 1f ? 12f : discoverRadius;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!autoDiscoverOnProximity || hasDiscoveredThisSession || settlementDefinition == null)
            {
                return;
            }

            if (!TryGetPlayerPosition(out Vector3 playerPosition))
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, playerPosition);
            if (distance > DiscoverRadius)
            {
                return;
            }

            if (TryDiscover(playerPosition))
            {
                hasDiscoveredThisSession = true;
            }
        }

        #endregion

        #region Public Methods

        public bool TryDiscover(Vector3 worldPosition)
        {
            if (settlementDefinition == null
                || !CCS_SettlementRuntimeBridge.TryGetSettlementService(out CCS_SettlementService settlementService)
                || !settlementService.IsInitialized)
            {
                return false;
            }

            Vector3 resolvedPosition = worldPosition == Vector3.zero ? transform.position : worldPosition;
            return settlementService.DiscoverSettlement(settlementDefinition, resolvedPosition);
        }

        public void NotifyServicePointUsed(CCS_SettlementServicePoint servicePoint)
        {
            if (settlementDefinition == null || servicePoint == null)
            {
                return;
            }

            TryDiscover(transform.position);

            if (!CCS_SettlementRuntimeBridge.TryGetSettlementService(out CCS_SettlementService settlementService)
                || !settlementService.IsInitialized)
            {
                return;
            }

            settlementService.NotifyServicePointActivated(new CCS_SettlementServicePointActivationArgs
            {
                SettlementId = settlementDefinition.SettlementId,
                ServicePointId = servicePoint.ServicePointId,
                ServicePointType = servicePoint.ServicePointType,
                VendorId = servicePoint.VendorDefinition != null ? servicePoint.VendorDefinition.VendorId : string.Empty,
                HasVendor = servicePoint.VendorDefinition != null
            });
        }

        #endregion

        #region Private Methods

        private static bool TryGetPlayerPosition(out Vector3 playerPosition)
        {
            playerPosition = Vector3.zero;
            CCS_PlayerGameplayController[] controllers =
                CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_PlayerGameplayController>();
            if (controllers == null || controllers.Length == 0)
            {
                return false;
            }

            playerPosition = controllers[0].transform.position;
            return true;
        }

        #endregion
    }
}
