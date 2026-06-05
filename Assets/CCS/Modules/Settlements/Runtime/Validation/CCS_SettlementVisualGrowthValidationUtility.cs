using System;
using System.Collections.Generic;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthValidationUtility
// CATEGORY: Modules / Settlements / Runtime / Validation
// PURPOSE: Visual growth status resolution, snapshot building, and profile validation.
// PLACEMENT: Used by visual growth service, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 — derives visuals from CCS_SettlementGrowthSnapshot.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementVisualGrowthValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_SettlementVisualGrowthProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement visual growth profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_SettlementVisualGrowthDefinition[] definitions = profile.AnchorDefinitions;
            if (definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement visual growth profile has no anchor definitions.");
            }

            HashSet<string> anchorIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementVisualGrowthDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.AnchorId))
                {
                    return CCS_SurvivalValidationResult.Fail("Settlement visual growth definition missing anchor id.");
                }

                if (definition.requiredGrowthStage == CCS_SettlementGrowthStage.Unknown)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Visual growth anchor '{definition.AnchorId}' has unknown required stage.");
                }

                if (!anchorIds.Add(definition.AnchorId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate settlement visual growth anchor id '{definition.AnchorId}'.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Settlement visual growth profile validated.");
        }

        public static CCS_SettlementVisualGrowthStatus ResolveVisualStatus(
            CCS_SettlementGrowthSnapshot growthSnapshot,
            CCS_SettlementGrowthStage requiredStage,
            bool settlementDiscovered)
        {
            if (growthSnapshot == null || !growthSnapshot.IsValid)
            {
                return settlementDiscovered
                    ? CCS_SettlementVisualGrowthStatus.Inactive
                    : CCS_SettlementVisualGrowthStatus.Locked;
            }

            if (growthSnapshot.CurrentGrowthStage >= requiredStage)
            {
                return CCS_SettlementVisualGrowthStatus.Active;
            }

            return settlementDiscovered
                ? CCS_SettlementVisualGrowthStatus.Inactive
                : CCS_SettlementVisualGrowthStatus.Locked;
        }

        public static CCS_SettlementVisualGrowthSnapshot BuildSnapshot(
            string settlementId,
            CCS_SettlementGrowthSnapshot growthSnapshot,
            bool settlementDiscovered,
            CCS_SettlementVisualGrowthDefinition[] anchorDefinitions)
        {
            if (string.IsNullOrWhiteSpace(settlementId) || anchorDefinitions == null)
            {
                return CCS_SettlementVisualGrowthSnapshot.Empty;
            }

            List<CCS_SettlementVisualGrowthEntry> entries = new List<CCS_SettlementVisualGrowthEntry>();
            for (int index = 0; index < anchorDefinitions.Length; index++)
            {
                CCS_SettlementVisualGrowthDefinition definition = anchorDefinitions[index];
                if (definition == null
                    || !string.Equals(definition.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                entries.Add(new CCS_SettlementVisualGrowthEntry
                {
                    AnchorId = definition.AnchorId,
                    MarkerType = definition.markerType,
                    RequiredGrowthStage = definition.requiredGrowthStage,
                    DisplayName = definition.DisplayName,
                    Status = ResolveVisualStatus(
                        growthSnapshot,
                        definition.requiredGrowthStage,
                        settlementDiscovered)
                });
            }

            return new CCS_SettlementVisualGrowthSnapshot
            {
                SettlementId = settlementId,
                CurrentGrowthStage = growthSnapshot?.CurrentGrowthStage ?? CCS_SettlementGrowthStage.Unknown,
                Entries = entries.ToArray()
            };
        }
    }
}
