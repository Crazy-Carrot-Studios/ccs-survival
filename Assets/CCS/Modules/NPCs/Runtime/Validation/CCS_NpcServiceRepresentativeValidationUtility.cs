using System;
using System.Collections.Generic;
using CCS.Modules.Settlements;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeValidationUtility
// CATEGORY: Modules / NPCs / Runtime / Validation
// PURPOSE: Profile, persistence, and routing validation for service representatives.
// PLACEMENT: Used by representative service, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 — service point fallback remains valid when representative missing.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcServiceRepresentativeValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_NpcServiceRepresentativeProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("NPC service representative profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_NpcServiceRepresentativeDefinition[] definitions = profile.RepresentativeDefinitions;
            if (definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("NPC service representative profile has no definitions.");
            }

            HashSet<string> representativeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcServiceRepresentativeDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.RepresentativeId))
                {
                    return CCS_SurvivalValidationResult.Fail("Representative definition missing representative id.");
                }

                if (string.IsNullOrWhiteSpace(definition.BusinessId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Representative '{definition.RepresentativeId}' missing business id.");
                }

                if (string.IsNullOrWhiteSpace(definition.ServicePointId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Representative '{definition.RepresentativeId}' missing service point id.");
                }

                if (definition.RequiredRole == CCS_NpcRoleType.Unknown)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Representative '{definition.RepresentativeId}' missing required role.");
                }

                if (CCS_NpcIdentityValidationUtility.IsPlaceholderOnlyRole(definition.RequiredRole))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Representative '{definition.RepresentativeId}' uses placeholder-only role.");
                }

                if (CCS_NpcServiceRepresentativeUtility.ResolveRouteType(definition.RequiredRole)
                    == CCS_SettlementServiceRouteType.Unknown)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Representative '{definition.RepresentativeId}' has unsupported route role.");
                }

                if (!representativeIds.Add(definition.RepresentativeId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate representative id '{definition.RepresentativeId}'.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("NPC service representative profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidatePersistedStates(
            CCS_NpcServiceRepresentativeState[] states)
        {
            if (states == null || states.Length == 0)
            {
                return CCS_SurvivalValidationResult.Pass("No persisted service representatives.");
            }

            HashSet<string> representativeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < states.Length; index++)
            {
                CCS_NpcServiceRepresentativeState state = states[index];
                if (state == null || string.IsNullOrWhiteSpace(state.representativeId))
                {
                    return CCS_SurvivalValidationResult.Fail("Persisted service representative missing id.");
                }

                if (!representativeIds.Add(state.representativeId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate persisted representative id '{state.representativeId}'.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Persisted service representative states validated.");
        }

        public static bool ValidateServicePointRouteReference(string servicePointId, CCS_NpcRoleType role)
        {
            if (string.IsNullOrWhiteSpace(servicePointId) || role == CCS_NpcRoleType.Unknown)
            {
                return false;
            }

            if (!CCS_SettlementServicePointRuntimeBridge.TryGetServicePoint(servicePointId, out CCS_SettlementServicePoint servicePoint)
                || servicePoint == null)
            {
                return true;
            }

            CCS_SettlementServiceRouteType expectedRoute = CCS_NpcServiceRepresentativeUtility.ResolveRouteType(role);
            CCS_SettlementServiceRouteType actualRoute = CCS_SettlementServiceRouteResolver.ResolveRouteType(servicePoint);
            return expectedRoute == actualRoute || actualRoute != CCS_SettlementServiceRouteType.Unknown;
        }
    }
}
