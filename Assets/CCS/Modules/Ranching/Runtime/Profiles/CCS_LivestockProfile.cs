using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LivestockProfile
// CATEGORY: Modules / Ranching / Runtime / Profiles
// PURPOSE: Profile catalog for livestock and ranch structure definitions.
// PLACEMENT: Assets/CCS/Survival/Profiles/Ranching/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Registered on CCS_SurvivalGameplayServiceHost.
// =============================================================================

namespace CCS.Modules.Ranching
{
    [CreateAssetMenu(
        fileName = "CCS_LivestockProfile",
        menuName = "CCS/Survival/Ranching/Livestock Profile")]
    public sealed class CCS_LivestockProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_LivestockDefinition[] livestockDefinitions =
            Array.Empty<CCS_LivestockDefinition>();

        [SerializeField] private CCS_RanchStructureDefinition[] ranchStructureDefinitions =
            Array.Empty<CCS_RanchStructureDefinition>();

        [SerializeField] private bool enableDebugLogging = true;

        public CCS_LivestockDefinition[] LivestockDefinitions =>
            livestockDefinitions ?? Array.Empty<CCS_LivestockDefinition>();

        public CCS_RanchStructureDefinition[] RanchStructureDefinitions =>
            ranchStructureDefinitions ?? Array.Empty<CCS_RanchStructureDefinition>();

        public bool EnableDebugLogging => enableDebugLogging;

        public bool TryGetLivestockById(string livestockId, out CCS_LivestockDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(livestockId))
            {
                return false;
            }

            CCS_LivestockDefinition[] definitions = LivestockDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_LivestockDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.LivestockId, livestockId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetLivestockByPurchaseItemId(string itemId, out CCS_LivestockDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            CCS_LivestockDefinition[] definitions = LivestockDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_LivestockDefinition candidate = definitions[index];
                if (candidate?.PurchaseItem != null
                    && string.Equals(candidate.PurchaseItem.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetStructureById(string structureDefinitionId, out CCS_RanchStructureDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(structureDefinitionId))
            {
                return false;
            }

            CCS_RanchStructureDefinition[] definitions = RanchStructureDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_RanchStructureDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(
                        candidate.StructureDefinitionId,
                        structureDefinitionId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetStructureByKitItemId(string itemId, out CCS_RanchStructureDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            CCS_RanchStructureDefinition[] definitions = RanchStructureDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_RanchStructureDefinition candidate = definitions[index];
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
