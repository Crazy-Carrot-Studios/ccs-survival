using System;
using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VehicleProfile
// CATEGORY: Modules / Vehicles / Runtime / Profiles
// PURPOSE: Catalog of vehicle definitions for the generic vehicle service.
// PLACEMENT: Assets/CCS/Survival/Profiles/Vehicles/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Vehicles
{
    [CreateAssetMenu(
        fileName = "CCS_VehicleProfile",
        menuName = "CCS/Survival/Vehicles/Vehicle Profile")]
    public sealed class CCS_VehicleProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_VehicleDefinition[] vehicleDefinitions = Array.Empty<CCS_VehicleDefinition>();
        [SerializeField] private CCS_VehicleDefinition defaultFrontierWagonDefinition;

        public IReadOnlyList<CCS_VehicleDefinition> VehicleDefinitions => vehicleDefinitions;

        public CCS_VehicleDefinition DefaultFrontierWagonDefinition => defaultFrontierWagonDefinition;

        public bool TryGetVehicleById(string vehicleId, out CCS_VehicleDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(vehicleId) || vehicleDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < vehicleDefinitions.Length; index++)
            {
                CCS_VehicleDefinition candidate = vehicleDefinitions[index];
                if (candidate != null
                    && string.Equals(candidate.VehicleId, vehicleId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
