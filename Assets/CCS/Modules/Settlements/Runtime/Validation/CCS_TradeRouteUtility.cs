using System.Collections.Generic;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_TradeRouteUtility
// CATEGORY: Modules / Settlements / Runtime / Validation
// PURPOSE: Validates trade route profiles and builds runtime snapshots.
// PLACEMENT: Used by validators, save capture, and future trade route services.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.5.0 — discovery, active, usage, and risk snapshot fields for freight.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_TradeRouteUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_TradeRouteProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Trade route profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_TradeRouteDefinition[] definitions = profile.TradeRouteDefinitions;
            if (definitions == null || definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Trade route profile requires at least one route definition.");
            }

            HashSet<string> routeIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_TradeRouteDefinition definition = definitions[index];
                if (definition == null)
                {
                    return CCS_SurvivalValidationResult.Fail($"Trade route definition at index {index} is null.");
                }

                if (string.IsNullOrWhiteSpace(definition.RouteId))
                {
                    return CCS_SurvivalValidationResult.Fail("Trade route definition routeId is required.");
                }

                if (!routeIds.Add(definition.RouteId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate trade route id '{definition.RouteId}'.");
                }

                if (string.IsNullOrWhiteSpace(definition.OriginSettlementId)
                    || string.IsNullOrWhiteSpace(definition.DestinationSettlementId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Trade route '{definition.RouteId}' requires origin and destination settlement ids.");
                }

                if (definition.Distance <= 0f)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Trade route '{definition.RouteId}' distance must be greater than zero.");
                }

                CCS_SurvivalValidationResult rewardValidation =
                    CCS_TradeRouteRewardModifierUtility.ValidateRouteRewardFields(definition);
                if (!rewardValidation.IsSuccess)
                {
                    return rewardValidation;
                }
            }

            return CCS_SurvivalValidationResult.Pass("Trade route profile validated.");
        }

        public static CCS_TradeRouteSnapshot BuildSnapshot(CCS_TradeRouteDefinition definition)
        {
            if (definition == null)
            {
                return new CCS_TradeRouteSnapshot();
            }

            return BuildSnapshot(
                definition,
                definition.StartsDiscovered,
                definition.StartsActive && definition.StartsDiscovered,
                0);
        }

        public static CCS_TradeRouteSnapshot BuildSnapshot(
            CCS_TradeRouteDefinition definition,
            bool isDiscovered,
            bool isActive,
            int usageCount)
        {
            if (definition == null)
            {
                return new CCS_TradeRouteSnapshot();
            }

            return new CCS_TradeRouteSnapshot
            {
                routeId = definition.RouteId,
                displayName = definition.DisplayName,
                originSettlementId = definition.OriginSettlementId,
                destinationSettlementId = definition.DestinationSettlementId,
                preferredGoods = definition.PreferredGoods,
                distance = definition.Distance,
                routeDifficulty = (int)definition.RouteDifficulty,
                riskRating = (int)definition.RiskRating,
                baseFreightMultiplier = definition.BaseFreightMultiplier,
                distanceMultiplier = definition.DistanceMultiplier,
                isDiscovered = isDiscovered,
                isActive = isActive,
                usageCount = usageCount < 0 ? 0 : usageCount
            };
        }

        public static CCS_TradeRouteSnapshot[] BuildSnapshots(CCS_TradeRouteProfile profile)
        {
            if (profile == null)
            {
                return new CCS_TradeRouteSnapshot[0];
            }

            CCS_TradeRouteDefinition[] definitions = profile.TradeRouteDefinitions;
            CCS_TradeRouteSnapshot[] snapshots = new CCS_TradeRouteSnapshot[definitions.Length];
            for (int index = 0; index < definitions.Length; index++)
            {
                snapshots[index] = BuildSnapshot(definitions[index]);
            }

            return snapshots;
        }
    }
}
