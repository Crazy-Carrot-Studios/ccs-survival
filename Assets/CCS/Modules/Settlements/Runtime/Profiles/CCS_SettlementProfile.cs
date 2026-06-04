using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementProfile
// CATEGORY: Modules / Settlements / Runtime / Profiles
// PURPOSE: Settlement module profile catalog and tuning.
// PLACEMENT: Assets/CCS/Survival/Profiles/Settlements/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Registered on CCS_SurvivalGameplayServiceHost.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_SettlementProfile",
        menuName = "CCS/Survival/Settlements/Settlement Profile")]
    public sealed class CCS_SettlementProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Catalog")]
        [Tooltip("Settlement definitions known to the settlement service.")]
        [SerializeField] private CCS_SettlementDefinition[] settlementDefinitions = new CCS_SettlementDefinition[0];

        [Header("Discovery")]
        [Tooltip("Default radius used when a location omits an explicit discover radius.")]
        [SerializeField] private float defaultDiscoverRadius = 12f;

        [Header("Trade Routes")]
        [Tooltip("Metadata-only trade routes between frontier settlements.")]
        [SerializeField] private CCS_TradeRouteProfile tradeRouteProfile;

        [Header("Diagnostics")]
        [Tooltip("Emit settlement service debug logs.")]
        [SerializeField] private bool enableDebugLogging = true;

        #endregion

        #region Properties

        public CCS_SettlementDefinition[] SettlementDefinitions =>
            settlementDefinitions ?? new CCS_SettlementDefinition[0];

        public CCS_TradeRouteProfile TradeRouteProfile => tradeRouteProfile;

        public float DefaultDiscoverRadius => defaultDiscoverRadius < 1f ? 12f : defaultDiscoverRadius;

        public bool EnableDebugLogging => enableDebugLogging;

        #endregion
    }
}
