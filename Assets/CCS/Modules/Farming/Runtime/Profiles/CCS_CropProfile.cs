using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CropProfile
// CATEGORY: Modules / Farming / Runtime / Profiles
// PURPOSE: Profile catalog for crop and farm plot definitions.
// PLACEMENT: Assets/CCS/Survival/Profiles/Farming/
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Registered on CCS_SurvivalGameplayServiceHost. Milestone 2.2.0.
// =============================================================================

namespace CCS.Modules.Farming
{
    [CreateAssetMenu(
        fileName = "CCS_CropProfile",
        menuName = "CCS/Survival/Farming/Crop Profile")]
    public sealed class CCS_CropProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_CropDefinition[] cropDefinitions = Array.Empty<CCS_CropDefinition>();
        [SerializeField] private CCS_FarmPlotDefinition[] farmPlotDefinitions = Array.Empty<CCS_FarmPlotDefinition>();
        [SerializeField] private bool enableDebugLogging = true;

        public CCS_CropDefinition[] CropDefinitions => cropDefinitions ?? Array.Empty<CCS_CropDefinition>();

        public CCS_FarmPlotDefinition[] FarmPlotDefinitions => farmPlotDefinitions ?? Array.Empty<CCS_FarmPlotDefinition>();

        public bool EnableDebugLogging => enableDebugLogging;

        public bool TryGetCropById(string cropId, out CCS_CropDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(cropId))
            {
                return false;
            }

            CCS_CropDefinition[] definitions = CropDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_CropDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.CropId, cropId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetCropBySeedItemId(string itemId, out CCS_CropDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            CCS_CropDefinition[] definitions = CropDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_CropDefinition candidate = definitions[index];
                if (candidate?.SeedItem != null
                    && string.Equals(candidate.SeedItem.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetPlotById(string plotDefinitionId, out CCS_FarmPlotDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(plotDefinitionId))
            {
                return false;
            }

            CCS_FarmPlotDefinition[] definitions = FarmPlotDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_FarmPlotDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.PlotDefinitionId, plotDefinitionId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetPlotByKitItemId(string itemId, out CCS_FarmPlotDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            CCS_FarmPlotDefinition[] definitions = FarmPlotDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_FarmPlotDefinition candidate = definitions[index];
                if (candidate?.PlaceableKitItem != null
                    && string.Equals(candidate.PlaceableKitItem.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
