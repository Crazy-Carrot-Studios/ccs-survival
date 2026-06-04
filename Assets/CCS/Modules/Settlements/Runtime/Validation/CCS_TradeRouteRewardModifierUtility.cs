using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TradeRouteRewardModifierUtility
// CATEGORY: Modules / Settlements / Runtime / Validation
// PURPOSE: Resolves route and risk multipliers for freight contract trade dollar rewards.
// PLACEMENT: Used by CCS_ContractService, validators, and debug HUD.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.5.0 — no encounters; Safe/Low/Moderate active risk bands.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_TradeRouteRewardModifierUtility
    {
        public const float SafeRiskMultiplier = 1f;
        public const float LowRiskMultiplier = 1.08f;
        public const float ModerateRiskMultiplier = 1.18f;
        public const float DangerousRiskMultiplierPlaceholder = 1.28f;
        public const float SevereRiskMultiplierPlaceholder = 1.4f;

        public static bool IsActiveRiskLevel(CCS_TradeRouteRiskLevel riskLevel)
        {
            return riskLevel == CCS_TradeRouteRiskLevel.Safe
                || riskLevel == CCS_TradeRouteRiskLevel.Low
                || riskLevel == CCS_TradeRouteRiskLevel.Moderate;
        }

        public static bool IsPlaceholderRiskLevel(CCS_TradeRouteRiskLevel riskLevel)
        {
            return riskLevel == CCS_TradeRouteRiskLevel.Dangerous
                || riskLevel == CCS_TradeRouteRiskLevel.Severe;
        }

        public static float ResolveRiskMultiplier(CCS_TradeRouteRiskLevel riskLevel)
        {
            switch (riskLevel)
            {
                case CCS_TradeRouteRiskLevel.Safe:
                    return SafeRiskMultiplier;
                case CCS_TradeRouteRiskLevel.Low:
                    return LowRiskMultiplier;
                case CCS_TradeRouteRiskLevel.Moderate:
                    return ModerateRiskMultiplier;
                case CCS_TradeRouteRiskLevel.Dangerous:
                    return DangerousRiskMultiplierPlaceholder;
                case CCS_TradeRouteRiskLevel.Severe:
                    return SevereRiskMultiplierPlaceholder;
                default:
                    return 1f;
            }
        }

        public static int ResolveBonusReputation(CCS_TradeRouteRiskLevel riskLevel)
        {
            switch (riskLevel)
            {
                case CCS_TradeRouteRiskLevel.Moderate:
                    return 1;
                case CCS_TradeRouteRiskLevel.Dangerous:
                case CCS_TradeRouteRiskLevel.Severe:
                    return 2;
                default:
                    return 0;
            }
        }

        public static float ResolveRouteMultiplier(CCS_TradeRouteDefinition route)
        {
            if (route == null)
            {
                return 1f;
            }

            float baseMultiplier = route.BaseFreightMultiplier < 0f ? 0f : route.BaseFreightMultiplier;
            float distanceMultiplier = route.DistanceMultiplier < 0f ? 0f : route.DistanceMultiplier;
            if (baseMultiplier <= 0f)
            {
                baseMultiplier = 1f;
            }

            if (distanceMultiplier <= 0f)
            {
                distanceMultiplier = 1f;
            }

            return baseMultiplier * distanceMultiplier;
        }

        public static CCS_TradeRouteFreightRewardBreakdown CalculateFreightTradeDollars(
            int baseTradeDollars,
            CCS_TradeRouteDefinition route)
        {
            CCS_TradeRouteFreightRewardBreakdown breakdown = new CCS_TradeRouteFreightRewardBreakdown
            {
                BaseTradeDollars = Mathf.Max(0, baseTradeDollars),
                RouteMultiplier = 1f,
                RiskMultiplier = 1f,
                FinalTradeDollars = Mathf.Max(0, baseTradeDollars)
            };

            if (route == null || baseTradeDollars <= 0)
            {
                return breakdown;
            }

            breakdown.LinkedRouteId = route.RouteId;
            breakdown.RiskLevel = route.RiskRating;
            breakdown.RouteMultiplier = ResolveRouteMultiplier(route);
            breakdown.RiskMultiplier = IsActiveRiskLevel(route.RiskRating)
                ? ResolveRiskMultiplier(route.RiskRating)
                : 1f;
            breakdown.BonusReputation = IsActiveRiskLevel(route.RiskRating)
                ? ResolveBonusReputation(route.RiskRating)
                : 0;
            breakdown.UsedRouteModifiers = true;

            float scaled = breakdown.BaseTradeDollars
                * breakdown.RouteMultiplier
                * breakdown.RiskMultiplier;
            breakdown.FinalTradeDollars = Mathf.Max(0, Mathf.RoundToInt(scaled));
            return breakdown;
        }

        public static CCS_TradeRouteFreightRewardBreakdown TryCalculateForLinkedRoute(
            int baseTradeDollars,
            string linkedRouteId,
            CCS_TradeRouteService tradeRouteService)
        {
            if (baseTradeDollars <= 0 || string.IsNullOrWhiteSpace(linkedRouteId))
            {
                return new CCS_TradeRouteFreightRewardBreakdown
                {
                    BaseTradeDollars = Mathf.Max(0, baseTradeDollars),
                    FinalTradeDollars = Mathf.Max(0, baseTradeDollars),
                    LinkedRouteId = linkedRouteId ?? string.Empty
                };
            }

            if (tradeRouteService == null
                || !tradeRouteService.IsInitialized
                || !tradeRouteService.TryGetRoute(linkedRouteId, out CCS_TradeRouteDefinition route)
                || route == null)
            {
                return new CCS_TradeRouteFreightRewardBreakdown
                {
                    BaseTradeDollars = Mathf.Max(0, baseTradeDollars),
                    FinalTradeDollars = Mathf.Max(0, baseTradeDollars),
                    LinkedRouteId = linkedRouteId
                };
            }

            return CalculateFreightTradeDollars(baseTradeDollars, route);
        }

        public static CCS_SurvivalValidationResult ValidateRouteRewardFields(CCS_TradeRouteDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Trade route definition is null.");
            }

            if (definition.BaseFreightMultiplier < 0f)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Trade route '{definition.RouteId}' baseFreightMultiplier cannot be negative.");
            }

            if (definition.DistanceMultiplier < 0f)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Trade route '{definition.RouteId}' distanceMultiplier cannot be negative.");
            }

            if (definition.RiskRating == CCS_TradeRouteRiskLevel.Unknown)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Trade route '{definition.RouteId}' requires a risk rating.");
            }

            if (IsPlaceholderRiskLevel(definition.RiskRating))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Trade route '{definition.RouteId}' uses placeholder risk '{definition.RiskRating}' in 3.5.0.");
            }

            if (!IsActiveRiskLevel(definition.RiskRating))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Trade route '{definition.RouteId}' risk '{definition.RiskRating}' is not an active 3.5.0 band.");
            }

            return CCS_SurvivalValidationResult.Pass($"Trade route '{definition.RouteId}' reward fields validated.");
        }
    }
}
