using System;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SettlementEventValidationUtility
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: Profile validation, eligibility checks, and event snapshot helpers.
// PLACEMENT: Used by CCS_SettlementEventService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 — only active event types participate in generation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementEventValidationUtility
    {
        private static readonly CCS_SettlementEventType[] ActiveEventTypes =
        {
            CCS_SettlementEventType.MarketDay,
            CCS_SettlementEventType.SupplyShipment,
            CCS_SettlementEventType.HarvestFestival,
            CCS_SettlementEventType.MiningShipment,
            CCS_SettlementEventType.TimberDelivery
        };

        public static bool IsActiveEventType(CCS_SettlementEventType eventType)
        {
            for (int index = 0; index < ActiveEventTypes.Length; index++)
            {
                if (ActiveEventTypes[index] == eventType)
                {
                    return true;
                }
            }

            return false;
        }

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_SettlementEventProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement event profile is missing.");
            }

            if (string.IsNullOrWhiteSpace(profile.ProfileId))
            {
                return CCS_SurvivalValidationResult.Fail("Settlement event profile requires profileId.");
            }

            CCS_SettlementEventDefinition[] definitions = profile.EventDefinitions;
            if (definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement event profile requires event definitions.");
            }

            bool hasMarketDay = false;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementEventDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.EventId))
                {
                    return CCS_SurvivalValidationResult.Fail("Settlement event definition requires eventId.");
                }

                if (!IsActiveEventType(definition.EventType))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Settlement event definition '{definition.EventId}' uses non-active type {definition.EventType}.");
                }

                if (string.IsNullOrWhiteSpace(definition.DisplayName))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Settlement event definition '{definition.EventId}' requires displayName.");
                }

                if (string.IsNullOrWhiteSpace(definition.EventMarkerAnchorId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Settlement event definition '{definition.EventId}' requires eventMarkerAnchorId.");
                }

                if (definition.ContractRewardMultiplier > 2f || definition.ReputationGainMultiplier > 2f)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Settlement event definition '{definition.EventId}' modifier exceeds dev cap.");
                }

                if (definition.EventType == CCS_SettlementEventType.MarketDay)
                {
                    hasMarketDay = true;
                }
            }

            if (!hasMarketDay)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement event profile missing MarketDay definition.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Settlement event profile validated ({profile.ProfileId}). {definitions.Length} definitions.");
        }

        public static bool IsSettlementEligible(CCS_SettlementEventDefinition definition, string settlementId)
        {
            if (definition == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            string[] settlementIds = definition.EligibleSettlementIds;
            for (int index = 0; index < settlementIds.Length; index++)
            {
                if (string.Equals(settlementIds[index], settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            if (settlementIds.Length > 0)
            {
                return false;
            }

            return true;
        }

        public static bool TryResolveEligibleDefinition(
            CCS_SettlementEventProfile profile,
            string settlementId,
            CCS_SettlementType settlementType,
            int population,
            float prosperity,
            int activeBusinessCount,
            int tradeRouteUsageCount,
            out CCS_SettlementEventDefinition definition)
        {
            definition = null;
            if (profile == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            CCS_SettlementEventDefinition bestDefinition = null;
            int bestScore = int.MinValue;
            CCS_SettlementEventDefinition[] definitions = profile.EventDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementEventDefinition candidate = definitions[index];
                if (candidate == null || !IsActiveEventType(candidate.EventType))
                {
                    continue;
                }

                if (!IsSettlementEligible(candidate, settlementId))
                {
                    continue;
                }

                if (!MatchesSettlementType(candidate, settlementType))
                {
                    continue;
                }

                if (population < candidate.MinimumPopulation
                    || prosperity < candidate.MinimumProsperity
                    || activeBusinessCount < candidate.MinimumActiveBusinesses
                    || tradeRouteUsageCount < candidate.MinimumTradeRouteUsage)
                {
                    continue;
                }

                int score = ScoreDefinition(candidate, settlementType);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDefinition = candidate;
                }
            }

            definition = bestDefinition;
            return definition != null;
        }

        public static bool IsEventExpired(CCS_SettlementEventState state, int currentDayNumber, int currentHour)
        {
            if (state == null || !state.isActive)
            {
                return true;
            }

            int elapsedHours = CalculateElapsedHours(
                state.startDayNumber,
                state.startHour,
                currentDayNumber,
                currentHour);
            return elapsedHours >= Math.Max(1, state.durationHours);
        }

        public static CCS_SettlementEventSnapshot BuildSnapshot(
            CCS_SettlementEventState state,
            CCS_SettlementEventDefinition definition)
        {
            if (state == null || definition == null || !state.isActive)
            {
                return CCS_SettlementEventSnapshot.Empty;
            }

            return new CCS_SettlementEventSnapshot
            {
                ActiveEventId = state.activeEventId ?? string.Empty,
                EventType = definition.EventType,
                SettlementId = state.settlementId ?? string.Empty,
                DisplayName = definition.DisplayName,
                EventMarkerAnchorId = definition.EventMarkerAnchorId,
                PreferredSocialAnchorId = definition.PreferredSocialAnchorId,
                DialogueAppendLine = definition.DialogueAppendLine,
                StartDayNumber = state.startDayNumber,
                StartHour = state.startHour,
                DurationHours = state.durationHours,
                ProsperityBonus = definition.ProsperityBonus,
                SupplyBonus = definition.SupplyBonus,
                ContractRewardMultiplier = definition.ContractRewardMultiplier,
                ReputationGainMultiplier = definition.ReputationGainMultiplier,
                IsActive = true
            };
        }

        public static CCS_SettlementEventState CloneState(CCS_SettlementEventState state)
        {
            if (state == null)
            {
                return new CCS_SettlementEventState();
            }

            return new CCS_SettlementEventState
            {
                activeEventId = state.activeEventId ?? string.Empty,
                eventType = state.eventType,
                startDayNumber = state.startDayNumber,
                startHour = state.startHour,
                durationHours = state.durationHours,
                settlementId = state.settlementId ?? string.Empty,
                isActive = state.isActive
            };
        }

        public static CCS_SettlementEventState CreateInactiveState(string settlementId)
        {
            return new CCS_SettlementEventState
            {
                settlementId = settlementId ?? string.Empty,
                isActive = false
            };
        }

        public static CCS_SettlementEventState CreateActiveState(
            CCS_SettlementEventDefinition definition,
            string settlementId,
            int startDayNumber,
            int startHour)
        {
            if (definition == null)
            {
                return CreateInactiveState(settlementId);
            }

            return new CCS_SettlementEventState
            {
                activeEventId = definition.EventId,
                eventType = (int)definition.EventType,
                startDayNumber = startDayNumber < 1 ? 1 : startDayNumber,
                startHour = ClampHour(startHour),
                durationHours = definition.DurationHours,
                settlementId = settlementId ?? string.Empty,
                isActive = true
            };
        }

        private static bool MatchesSettlementType(
            CCS_SettlementEventDefinition definition,
            CCS_SettlementType settlementType)
        {
            int[] settlementTypes = definition.EligibleSettlementTypes;
            if (settlementTypes == null || settlementTypes.Length == 0)
            {
                return true;
            }

            for (int index = 0; index < settlementTypes.Length; index++)
            {
                if (Enum.IsDefined(typeof(CCS_SettlementType), settlementTypes[index])
                    && (CCS_SettlementType)settlementTypes[index] == settlementType)
                {
                    return true;
                }
            }

            return false;
        }

        private static int ScoreDefinition(
            CCS_SettlementEventDefinition definition,
            CCS_SettlementType settlementType)
        {
            int score = (int)definition.EventType;
            switch (settlementType)
            {
                case CCS_SettlementType.TradingPost:
                    if (definition.EventType == CCS_SettlementEventType.MarketDay)
                    {
                        score += 20;
                    }
                    else if (definition.EventType == CCS_SettlementEventType.SupplyShipment)
                    {
                        score += 10;
                    }

                    break;
                case CCS_SettlementType.Homestead:
                    if (definition.EventType == CCS_SettlementEventType.HarvestFestival)
                    {
                        score += 20;
                    }

                    break;
                case CCS_SettlementType.MiningCamp:
                    if (definition.EventType == CCS_SettlementEventType.MiningShipment)
                    {
                        score += 20;
                    }

                    break;
                case CCS_SettlementType.Other:
                    if (definition.EventType == CCS_SettlementEventType.TimberDelivery)
                    {
                        score += 20;
                    }

                    break;
            }

            return score;
        }

        private static int CalculateElapsedHours(
            int startDayNumber,
            int startHour,
            int currentDayNumber,
            int currentHour)
        {
            int safeStartDay = startDayNumber < 1 ? 1 : startDayNumber;
            int safeCurrentDay = currentDayNumber < 1 ? 1 : currentDayNumber;
            int dayDelta = safeCurrentDay - safeStartDay;
            if (dayDelta < 0)
            {
                return 0;
            }

            return dayDelta * 24 + (ClampHour(currentHour) - ClampHour(startHour));
        }

        private static int ClampHour(int hour)
        {
            if (hour < 0)
            {
                return 0;
            }

            if (hour > 23)
            {
                return 23;
            }

            return hour;
        }
    }
}
