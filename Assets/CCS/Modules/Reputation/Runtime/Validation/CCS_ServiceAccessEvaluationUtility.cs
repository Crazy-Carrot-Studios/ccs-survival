using System;

// =============================================================================
// SCRIPT: CCS_ServiceAccessEvaluationUtility
// CATEGORY: Modules / Reputation / Runtime / Validation
// PURPOSE: Evaluates service access rules against settlement context and reputation standing.
// PLACEMENT: Used by settlement routing and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 2.8.0 — safe fallback when reputation service or rule is missing.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public static class CCS_ServiceAccessEvaluationUtility
    {
        public static CCS_ServiceAccessResult EvaluateForServicePoint(
            CCS_ReputationService reputationService,
            string settlementId,
            string servicePointId,
            int servicePointTypeValue,
            bool isSettlementDiscovered)
        {
            CCS_ServiceAccessProfile accessProfile = reputationService?.ActiveProfile?.ServiceAccessProfile;
            if (accessProfile == null
                || !accessProfile.TryResolveRule(settlementId, servicePointId, servicePointTypeValue, out CCS_ServiceAccessRule rule)
                || rule == null)
            {
                return CCS_ServiceAccessResult.Allowed("No service access rule configured.");
            }

            return EvaluateRequirement(
                rule.Requirement,
                settlementId,
                reputationService,
                isSettlementDiscovered);
        }

        public static CCS_ServiceAccessResult EvaluateRequirement(
            CCS_ServiceAccessRequirement requirement,
            string settlementId,
            CCS_ReputationService reputationService,
            bool isSettlementDiscovered)
        {
            if (requirement == null || !requirement.Enabled)
            {
                return CCS_ServiceAccessResult.Denied(
                    CCS_ServiceAccessResultType.DeniedDisabled,
                    "Service access rule is disabled.");
            }

            if (requirement.RequireDiscoveredSettlement && !isSettlementDiscovered)
            {
                return CCS_ServiceAccessResult.Denied(
                    CCS_ServiceAccessResultType.DeniedUnavailable,
                    "Discover this settlement before using services.");
            }

            if (requirement.RequiredCampTier >= 0)
            {
                return CCS_ServiceAccessResult.Denied(
                    CCS_ServiceAccessResultType.MissingRequirement,
                    $"Requires camp tier {requirement.RequiredCampTier} (future requirement).",
                    requirement.FutureHookPlaceholder);
            }

            if (requirement.RequireLandClaim)
            {
                return CCS_ServiceAccessResult.Denied(
                    CCS_ServiceAccessResultType.MissingRequirement,
                    "Requires an owned land claim (future requirement).",
                    requirement.FutureHookPlaceholder);
            }

            if (requirement.MinimumGrowthStage >= 0)
            {
                return CCS_ServiceAccessResult.Denied(
                    CCS_ServiceAccessResultType.MissingRequirement,
                    $"Requires settlement growth stage {requirement.MinimumGrowthStage} (future requirement).",
                    requirement.FutureHookPlaceholder);
            }

            if (reputationService == null || !reputationService.IsInitialized)
            {
                return CCS_ServiceAccessResult.Allowed("Reputation service unavailable; access allowed by fallback.");
            }

            if (!reputationService.TryGetSettlementStanding(settlementId, out CCS_ReputationStanding standing)
                || standing == null)
            {
                standing = new CCS_ReputationStanding(
                    string.Empty,
                    CCS_ReputationScopeType.Settlement,
                    settlementId,
                    0,
                    -100,
                    100,
                    CCS_ReputationTier.Neutral);
            }

            if (standing.CurrentValue < requirement.MinimumReputationValue)
            {
                return CCS_ServiceAccessResult.Denied(
                    CCS_ServiceAccessResultType.DeniedReputation,
                    $"Requires reputation value {requirement.MinimumReputationValue} (current {standing.CurrentValue}).");
            }

            if ((int)standing.DisplayTier < (int)requirement.MinimumReputationTier)
            {
                return CCS_ServiceAccessResult.Denied(
                    CCS_ServiceAccessResultType.DeniedReputation,
                    $"Requires reputation tier {requirement.MinimumReputationTier} (current {standing.DisplayTier}).");
            }

            return CCS_ServiceAccessResult.Allowed("Service access requirements satisfied.");
        }
    }
}
