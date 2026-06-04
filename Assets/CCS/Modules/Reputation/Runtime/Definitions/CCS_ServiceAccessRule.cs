using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ServiceAccessRule
// CATEGORY: Modules / Reputation / Runtime / Definitions
// PURPOSE: ScriptableObject rule mapping settlement service points to access requirements.
// PLACEMENT: Assets/CCS/Survival/Content/Reputation/Access/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 2.8.0 service access and price modifier foundation.
// =============================================================================

namespace CCS.Modules.Reputation
{
    [CreateAssetMenu(
        fileName = "CCS_ServiceAccessRule",
        menuName = "CCS/Survival/Reputation/Service Access Rule")]
    public sealed class CCS_ServiceAccessRule : ScriptableObject
    {
        [SerializeField] private string ruleId = string.Empty;

        [SerializeField] private string settlementId = CCS_ReputationContentIds.DefaultTradingPostSettlementId;

        [SerializeField] private string servicePointId = string.Empty;

        [Tooltip("-1 matches any service point type when servicePointId is empty.")]
        [SerializeField] private int servicePointTypeFilter = -1;

        [SerializeField] private CCS_ServiceAccessRequirement requirement = new CCS_ServiceAccessRequirement();

        public string RuleId => ruleId ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        public string ServicePointId => servicePointId ?? string.Empty;

        public int ServicePointTypeFilter => servicePointTypeFilter;

        public CCS_ServiceAccessRequirement Requirement => requirement;

        public bool Matches(string resolvedSettlementId, string resolvedServicePointId, int servicePointTypeValue)
        {
            if (!string.IsNullOrWhiteSpace(settlementId)
                && !string.Equals(settlementId, resolvedSettlementId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(servicePointId))
            {
                return string.Equals(servicePointId, resolvedServicePointId, StringComparison.OrdinalIgnoreCase);
            }

            return servicePointTypeFilter < 0 || servicePointTypeFilter == servicePointTypeValue;
        }
    }
}
