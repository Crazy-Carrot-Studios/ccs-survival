using UnityEngine;

// =============================================================================
// SCRIPT: CCS_UpkeepDefinition
// CATEGORY: Modules / Upkeep / Runtime / Definitions
// PURPOSE: ScriptableObject definition for a recurring upkeep or tax cost.
// PLACEMENT: Assets/CCS/Survival/Content/Upkeep/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Generic template; western naming lives in Survival content assets.
// =============================================================================

namespace CCS.Modules.Upkeep
{
    [CreateAssetMenu(
        fileName = "CCS_UpkeepDefinition",
        menuName = "CCS/Survival/Upkeep/Upkeep Definition")]
    public sealed class CCS_UpkeepDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable reverse-DNS upkeep definition id.")]
        [SerializeField] private string upkeepDefinitionId = string.Empty;

        [Tooltip("Player-facing upkeep label.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Target archetype this upkeep applies to.")]
        [SerializeField] private CCS_UpkeepTargetType targetType = CCS_UpkeepTargetType.LandClaim;

        [Header("Cost")]
        [Tooltip("Currency id charged for this upkeep.")]
        [SerializeField] private string currencyId = string.Empty;

        [Tooltip("Recurring cost amount when due.")]
        [SerializeField] private int amount = 25;

        [Tooltip("Interval in in-game days placeholder (manual/simple timer in 2.5.0).")]
        [SerializeField] private int intervalDaysPlaceholder = 7;

        [Tooltip("Grace period in days placeholder before failed status escalation.")]
        [SerializeField] private int gracePeriodDaysPlaceholder = 3;

        [Header("Payment")]
        [Tooltip("Attempt bank debit before wallet when paying upkeep.")]
        [SerializeField] private bool autoPayFromBank = true;

        [Tooltip("Attempt wallet debit when bank payment is unavailable.")]
        [SerializeField] private bool autoPayFromWallet = true;

        [Tooltip("Whether this upkeep definition is active.")]
        [SerializeField] private bool enabled = true;

        public string UpkeepDefinitionId => upkeepDefinitionId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public CCS_UpkeepTargetType TargetType => targetType;

        public string CurrencyId => currencyId ?? string.Empty;

        public int Amount => amount;

        public int IntervalDaysPlaceholder => intervalDaysPlaceholder;

        public int GracePeriodDaysPlaceholder => gracePeriodDaysPlaceholder;

        public bool AutoPayFromBank => autoPayFromBank;

        public bool AutoPayFromWallet => autoPayFromWallet;

        public bool Enabled => enabled;
    }
}
