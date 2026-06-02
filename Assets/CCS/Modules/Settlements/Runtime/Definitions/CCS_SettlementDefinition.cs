using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementDefinition
// CATEGORY: Modules / Settlements / Runtime / Definitions
// PURPOSE: ScriptableObject definition for a frontier settlement location.
// PLACEMENT: Assets/CCS/Survival/Content/Settlements/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Western-specific settlement content lives under Survival assets.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_SettlementDefinition",
        menuName = "CCS/Survival/Settlements/Settlement Definition")]
    public sealed class CCS_SettlementDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS settlement id.")]
        [SerializeField] private string settlementId = string.Empty;

        [Tooltip("Player-facing settlement name.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Short description for debug and future UI.")]
        [SerializeField] private string description = string.Empty;

        [Tooltip("Generic settlement archetype.")]
        [SerializeField] private CCS_SettlementType settlementType = CCS_SettlementType.TradingPost;

        [Header("World")]
        [Tooltip("Default world position used for discovery map placeholder.")]
        [SerializeField] private Vector3 defaultWorldPosition;

        #endregion

        #region Properties

        public string SettlementId => settlementId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public string Description => description ?? string.Empty;

        public CCS_SettlementType SettlementType => settlementType;

        public Vector3 DefaultWorldPosition => defaultWorldPosition;

        #endregion
    }
}
