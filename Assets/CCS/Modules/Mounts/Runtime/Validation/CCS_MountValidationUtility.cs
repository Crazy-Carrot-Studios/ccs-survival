using System;
using System.Collections.Generic;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_MountValidationUtility
// CATEGORY: Modules / Mounts / Runtime / Validation
// PURPOSE: Profile and content validation for the generic mount module.
// PLACEMENT: Used by editor validators and runtime profile initialization.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    public static class CCS_MountValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_MountProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Mount profile is missing.");
            }

            if (!profile.TryGetMountById(CCS_MountContentIds.HorseMountId, out CCS_MountDefinition horse))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Mount profile missing horse definition '{CCS_MountContentIds.HorseMountId}'.");
            }

            CCS_SurvivalValidationResult horseResult = ValidateDefinition(horse);
            if (!horseResult.IsSuccess)
            {
                return horseResult;
            }

            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            IReadOnlyList<CCS_MountDefinition> definitions = profile.MountDefinitions;
            for (int index = 0; index < definitions.Count; index++)
            {
                CCS_MountDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.MountId))
                {
                    continue;
                }

                if (!ids.Add(definition.MountId))
                {
                    return CCS_SurvivalValidationResult.Fail($"Duplicate mount id '{definition.MountId}'.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Mount profile is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateDefinition(CCS_MountDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Mount definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.MountId))
            {
                return CCS_SurvivalValidationResult.Fail("Mount definition has empty mountId.");
            }

            if (definition.WorldPrefab == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Mount '{definition.MountId}' is missing world prefab reference.");
            }

            if (definition.MovementSpeed <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Mount '{definition.MountId}' movement speed must be positive.");
            }

            if (definition.PurchaseValue <= 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Mount '{definition.MountId}' purchase value must be positive.");
            }

            return CCS_SurvivalValidationResult.Pass($"Mount definition '{definition.MountId}' is valid.");
        }
    }
}
