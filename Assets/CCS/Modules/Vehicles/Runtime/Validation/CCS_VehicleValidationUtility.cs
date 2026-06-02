using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VehicleValidationUtility
// CATEGORY: Modules / Vehicles / Runtime / Validation
// PURPOSE: Profile and content validation for the generic vehicle module.
// PLACEMENT: Used by CCS_VehicleService and editor validators.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Vehicles
{
    public static class CCS_VehicleValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_VehicleProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Vehicle profile is missing.");
            }

            HashSet<string> vehicleIds = new HashSet<string>();
            IReadOnlyList<CCS_VehicleDefinition> definitions = profile.VehicleDefinitions;
            for (int index = 0; index < definitions.Count; index++)
            {
                CCS_VehicleDefinition definition = definitions[index];
                if (definition == null)
                {
                    continue;
                }

                string vehicleId = definition.VehicleId;
                if (string.IsNullOrWhiteSpace(vehicleId))
                {
                    return CCS_SurvivalValidationResult.Fail("Vehicle definition has an empty vehicleId.");
                }

                if (!vehicleIds.Add(vehicleId))
                {
                    return CCS_SurvivalValidationResult.Fail($"Duplicate vehicleId detected: {vehicleId}");
                }

                if (definition.WorldPrefab == null)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Vehicle '{vehicleId}' is missing a world prefab reference.");
                }
            }

            if (profile.DefaultFrontierWagonDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Default frontier wagon definition is missing.");
            }

            return CCS_SurvivalValidationResult.Pass("Vehicle profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateWagonPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                return CCS_SurvivalValidationResult.Fail("Frontier wagon prefab is missing.");
            }

            if (prefab.GetComponent<CCS_VehicleWorldActor>() == null)
            {
                return CCS_SurvivalValidationResult.Fail("Frontier wagon prefab is missing CCS_VehicleWorldActor.");
            }

            if (prefab.GetComponentInChildren<CCS_WagonCargoContainer>() == null)
            {
                return CCS_SurvivalValidationResult.Fail("Frontier wagon prefab is missing CCS_WagonCargoContainer.");
            }

            return CCS_SurvivalValidationResult.Pass("Frontier wagon prefab validated.");
        }
    }
}
