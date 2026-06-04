using System;
using System.Collections.Generic;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_BusinessPresenceValidationUtility
// CATEGORY: Modules / Settlements / Runtime / Validation
// PURPOSE: Presence status resolution, snapshot building, and profile validation.
// PLACEMENT: Used by CCS_BusinessPresenceService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — derives visuals from CCS_BusinessSnapshot.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_BusinessPresenceValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_BusinessPresenceProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Business presence profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_BusinessPresenceDefinition[] definitions = profile.AnchorDefinitions;
            if (definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Business presence profile has no anchor definitions.");
            }

            HashSet<string> anchorIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_BusinessPresenceDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.AnchorId))
                {
                    return CCS_SurvivalValidationResult.Fail("Business presence definition missing anchor id.");
                }

                if (!anchorIds.Add(definition.AnchorId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate business presence anchor id '{definition.AnchorId}'.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Business presence profile validated.");
        }

        public static CCS_BusinessPresenceStatus ResolvePresenceStatus(
            CCS_BusinessSnapshot businessSnapshot,
            CCS_BusinessType businessType)
        {
            if (businessSnapshot == null || !businessSnapshot.IsValid)
            {
                return CCS_BusinessPresenceStatus.Locked;
            }

            if (ContainsBusinessType(businessSnapshot.ActiveBusinesses, businessType))
            {
                return CCS_BusinessPresenceStatus.Active;
            }

            if (ContainsBusinessType(businessSnapshot.AvailableBusinesses, businessType))
            {
                return CCS_BusinessPresenceStatus.Inactive;
            }

            return CCS_BusinessPresenceStatus.Locked;
        }

        public static CCS_BusinessPresenceSnapshot BuildSnapshot(
            string settlementId,
            CCS_BusinessSnapshot businessSnapshot,
            CCS_BusinessPresenceDefinition[] anchorDefinitions)
        {
            if (string.IsNullOrWhiteSpace(settlementId) || anchorDefinitions == null)
            {
                return CCS_BusinessPresenceSnapshot.Empty;
            }

            List<CCS_BusinessPresenceEntry> entries = new List<CCS_BusinessPresenceEntry>();
            for (int index = 0; index < anchorDefinitions.Length; index++)
            {
                CCS_BusinessPresenceDefinition definition = anchorDefinitions[index];
                if (definition == null
                    || !string.Equals(definition.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                entries.Add(new CCS_BusinessPresenceEntry
                {
                    AnchorId = definition.AnchorId,
                    BusinessId = definition.BusinessId,
                    BusinessType = definition.businessType,
                    DisplayName = definition.DisplayName,
                    Status = ResolvePresenceStatus(businessSnapshot, definition.businessType)
                });
            }

            return new CCS_BusinessPresenceSnapshot
            {
                SettlementId = settlementId,
                Entries = entries.ToArray()
            };
        }

        private static bool ContainsBusinessType(CCS_BusinessInstance[] instances, CCS_BusinessType businessType)
        {
            if (instances == null)
            {
                return false;
            }

            for (int index = 0; index < instances.Length; index++)
            {
                CCS_BusinessInstance instance = instances[index];
                if (instance != null && instance.BusinessType == businessType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
