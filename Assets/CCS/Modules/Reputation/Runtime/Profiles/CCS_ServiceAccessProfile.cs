using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ServiceAccessProfile
// CATEGORY: Modules / Reputation / Runtime / Profiles
// PURPOSE: Catalog of service access rules for settlement service points.
// PLACEMENT: Assets/CCS/Survival/Profiles/Reputation/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 2.8.0 service access and price modifier foundation.
// =============================================================================

namespace CCS.Modules.Reputation
{
    [CreateAssetMenu(
        fileName = "CCS_ServiceAccessProfile",
        menuName = "CCS/Survival/Reputation/Service Access Profile")]
    public sealed class CCS_ServiceAccessProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_ServiceAccessRule[] serviceAccessRules = Array.Empty<CCS_ServiceAccessRule>();

        public CCS_ServiceAccessRule[] ServiceAccessRules =>
            serviceAccessRules ?? Array.Empty<CCS_ServiceAccessRule>();

        public bool TryResolveRule(
            string settlementId,
            string servicePointId,
            int servicePointTypeValue,
            out CCS_ServiceAccessRule rule)
        {
            rule = null;
            CCS_ServiceAccessRule[] rules = ServiceAccessRules;
            int bestScore = -1;

            for (int index = 0; index < rules.Length; index++)
            {
                CCS_ServiceAccessRule candidate = rules[index];
                if (candidate == null || !candidate.Matches(settlementId, servicePointId, servicePointTypeValue))
                {
                    continue;
                }

                int score = 0;
                if (!string.IsNullOrWhiteSpace(candidate.ServicePointId))
                {
                    score += 2;
                }

                if (candidate.ServicePointTypeFilter >= 0)
                {
                    score += 1;
                }

                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                rule = candidate;
            }

            return rule != null;
        }
    }
}
