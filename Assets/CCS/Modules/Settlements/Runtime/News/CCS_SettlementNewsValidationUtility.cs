using System;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SettlementNewsValidationUtility
// CATEGORY: Modules / Settlements / Runtime / News
// PURPOSE: Profile validation, propagation helpers, and news snapshot builders.
// PLACEMENT: Used by CCS_SettlementNewsService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.2.0 — only active news types participate in event integration.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementNewsValidationUtility
    {
        private static readonly CCS_SettlementNewsType[] ActiveNewsTypes =
        {
            CCS_SettlementNewsType.MarketDay,
            CCS_SettlementNewsType.SupplyShipment,
            CCS_SettlementNewsType.HarvestFestival,
            CCS_SettlementNewsType.MiningShipment,
            CCS_SettlementNewsType.TimberDelivery
        };

        public static bool IsActiveNewsType(CCS_SettlementNewsType newsType)
        {
            for (int index = 0; index < ActiveNewsTypes.Length; index++)
            {
                if (ActiveNewsTypes[index] == newsType)
                {
                    return true;
                }
            }

            return false;
        }

        public static CCS_SettlementNewsType MapEventTypeToNewsType(CCS_SettlementEventType eventType)
        {
            switch (eventType)
            {
                case CCS_SettlementEventType.MarketDay:
                    return CCS_SettlementNewsType.MarketDay;
                case CCS_SettlementEventType.SupplyShipment:
                    return CCS_SettlementNewsType.SupplyShipment;
                case CCS_SettlementEventType.HarvestFestival:
                    return CCS_SettlementNewsType.HarvestFestival;
                case CCS_SettlementEventType.MiningShipment:
                    return CCS_SettlementNewsType.MiningShipment;
                case CCS_SettlementEventType.TimberDelivery:
                    return CCS_SettlementNewsType.TimberDelivery;
                default:
                    return CCS_SettlementNewsType.Unknown;
            }
        }

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_SettlementNewsProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement news profile is missing.");
            }

            if (string.IsNullOrWhiteSpace(profile.ProfileId))
            {
                return CCS_SurvivalValidationResult.Fail("Settlement news profile requires profileId.");
            }

            CCS_SettlementNewsDefinition[] definitions = profile.NewsDefinitions;
            if (definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement news profile requires news definitions.");
            }

            bool hasMarketDay = false;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementNewsDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.DefinitionId))
                {
                    return CCS_SurvivalValidationResult.Fail("Settlement news definition requires definitionId.");
                }

                if (!IsActiveNewsType(definition.NewsType))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Settlement news definition '{definition.DefinitionId}' uses non-active type {definition.NewsType}.");
                }

                if (string.IsNullOrWhiteSpace(definition.HeadlineTemplate))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Settlement news definition '{definition.DefinitionId}' requires headlineTemplate.");
                }

                if (string.IsNullOrWhiteSpace(definition.RumorLineTemplate))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Settlement news definition '{definition.DefinitionId}' requires rumorLineTemplate.");
                }

                if (definition.NewsType == CCS_SettlementNewsType.MarketDay)
                {
                    hasMarketDay = true;
                }
            }

            if (!hasMarketDay)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement news profile missing MarketDay definition.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Settlement news profile validated ({profile.ProfileId}). {definitions.Length} definitions.");
        }

        public static bool IsNewsExpired(CCS_SettlementNewsState state, int currentDayNumber)
        {
            if (state == null || !state.isActive)
            {
                return true;
            }

            int safeCurrentDay = currentDayNumber < 1 ? 1 : currentDayNumber;
            return safeCurrentDay > Math.Max(1, state.expirationDay);
        }

        public static bool IsSettlementAwareOfNews(CCS_SettlementNewsState state, string settlementId)
        {
            if (state == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            string[] knownSettlementIds = state.knownSettlementIds;
            for (int index = 0; index < knownSettlementIds.Length; index++)
            {
                if (string.Equals(knownSettlementIds[index], settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static CCS_SettlementNewsSnapshot BuildSnapshot(
            CCS_SettlementNewsState state,
            string viewingSettlementId)
        {
            if (state == null || !state.isActive)
            {
                return CCS_SettlementNewsSnapshot.Empty;
            }

            return new CCS_SettlementNewsSnapshot
            {
                NewsId = state.newsId ?? string.Empty,
                OriginSettlementId = state.originSettlementId ?? string.Empty,
                ViewingSettlementId = viewingSettlementId ?? string.Empty,
                NewsType = Enum.IsDefined(typeof(CCS_SettlementNewsType), state.eventType)
                    ? (CCS_SettlementNewsType)state.eventType
                    : CCS_SettlementNewsType.Unknown,
                Headline = state.headline ?? string.Empty,
                RumorLine = state.rumorLine ?? string.Empty,
                DayNumber = state.dayNumber,
                ExpirationDay = state.expirationDay,
                IsActive = true
            };
        }

        public static CCS_SettlementNewsState CloneState(CCS_SettlementNewsState state)
        {
            if (state == null)
            {
                return new CCS_SettlementNewsState();
            }

            return new CCS_SettlementNewsState
            {
                newsId = state.newsId ?? string.Empty,
                originSettlementId = state.originSettlementId ?? string.Empty,
                eventType = state.eventType,
                headline = state.headline ?? string.Empty,
                rumorLine = state.rumorLine ?? string.Empty,
                dayNumber = state.dayNumber,
                expirationDay = state.expirationDay,
                propagationReadyDay = state.propagationReadyDay,
                knownSettlementIds = CloneKnownSettlementIds(state.knownSettlementIds),
                isActive = state.isActive
            };
        }

        public static CCS_SettlementNewsState[] CloneStates(CCS_SettlementNewsState[] states)
        {
            if (states == null || states.Length == 0)
            {
                return Array.Empty<CCS_SettlementNewsState>();
            }

            CCS_SettlementNewsState[] clones = new CCS_SettlementNewsState[states.Length];
            for (int index = 0; index < states.Length; index++)
            {
                clones[index] = CloneState(states[index]);
            }

            return clones;
        }

        public static CCS_SettlementNewsState CreateNewsState(
            CCS_SettlementNewsDefinition definition,
            string originSettlementId,
            string originDisplayName,
            int currentDayNumber)
        {
            if (definition == null || string.IsNullOrWhiteSpace(originSettlementId))
            {
                return new CCS_SettlementNewsState { isActive = false };
            }

            int safeDay = currentDayNumber < 1 ? 1 : currentDayNumber;
            string displayName = string.IsNullOrWhiteSpace(originDisplayName)
                ? originSettlementId
                : originDisplayName;
            return new CCS_SettlementNewsState
            {
                newsId = BuildNewsId(originSettlementId, definition.NewsType, safeDay),
                originSettlementId = originSettlementId,
                eventType = (int)definition.NewsType,
                headline = ResolveTemplate(definition.HeadlineTemplate, displayName),
                rumorLine = ResolveTemplate(definition.RumorLineTemplate, displayName),
                dayNumber = safeDay,
                expirationDay = safeDay + definition.NewsDurationDays,
                propagationReadyDay = safeDay + definition.PropagationDelayDays,
                knownSettlementIds = new[] { originSettlementId },
                isActive = true
            };
        }

        public static string ResolveTemplate(string template, string settlementDisplayName)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return string.Empty;
            }

            return template.Replace("{settlement}", settlementDisplayName ?? string.Empty);
        }

        public static string[] ResolveConnectedSettlementIds(
            string originSettlementId,
            CCS_TradeRouteDefinition[] routeDefinitions)
        {
            if (string.IsNullOrWhiteSpace(originSettlementId) || routeDefinitions == null || routeDefinitions.Length == 0)
            {
                return Array.Empty<string>();
            }

            string[] buffer = new string[routeDefinitions.Length * 2];
            int writeIndex = 0;
            for (int index = 0; index < routeDefinitions.Length; index++)
            {
                CCS_TradeRouteDefinition definition = routeDefinitions[index];
                if (definition == null)
                {
                    continue;
                }

                if (string.Equals(definition.OriginSettlementId, originSettlementId, StringComparison.OrdinalIgnoreCase)
                    && !ContainsSettlementId(buffer, writeIndex, definition.DestinationSettlementId))
                {
                    buffer[writeIndex++] = definition.DestinationSettlementId;
                }

                if (string.Equals(
                        definition.DestinationSettlementId,
                        originSettlementId,
                        StringComparison.OrdinalIgnoreCase)
                    && !ContainsSettlementId(buffer, writeIndex, definition.OriginSettlementId))
                {
                    buffer[writeIndex++] = definition.OriginSettlementId;
                }
            }

            if (writeIndex == 0)
            {
                return Array.Empty<string>();
            }

            string[] connected = new string[writeIndex];
            Array.Copy(buffer, connected, writeIndex);
            return connected;
        }

        public static void AddKnownSettlement(CCS_SettlementNewsState state, string settlementId)
        {
            if (state == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return;
            }

            if (IsSettlementAwareOfNews(state, settlementId))
            {
                return;
            }

            string[] knownSettlementIds = state.knownSettlementIds ?? Array.Empty<string>();
            string[] expanded = new string[knownSettlementIds.Length + 1];
            Array.Copy(knownSettlementIds, expanded, knownSettlementIds.Length);
            expanded[knownSettlementIds.Length] = settlementId;
            state.knownSettlementIds = expanded;
        }

        private static string BuildNewsId(
            string originSettlementId,
            CCS_SettlementNewsType newsType,
            int dayNumber)
        {
            return $"ccs.survival.news.{originSettlementId}.{newsType}.{dayNumber}";
        }

        private static string[] CloneKnownSettlementIds(string[] knownSettlementIds)
        {
            if (knownSettlementIds == null || knownSettlementIds.Length == 0)
            {
                return Array.Empty<string>();
            }

            string[] clones = new string[knownSettlementIds.Length];
            Array.Copy(knownSettlementIds, clones, knownSettlementIds.Length);
            return clones;
        }

        private static bool ContainsSettlementId(string[] settlementIds, int length, string settlementId)
        {
            for (int index = 0; index < length; index++)
            {
                if (string.Equals(settlementIds[index], settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
