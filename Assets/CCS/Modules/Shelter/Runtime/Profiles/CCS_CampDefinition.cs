using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CampDefinition
// CATEGORY: Modules / Shelter / Runtime / Profiles
// PURPOSE: Catalog of frontier shelter definitions and camp detection tuning.
// PLACEMENT: Assets/CCS/Survival/Profiles/Camp/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Shelter
{
    [CreateAssetMenu(
        fileName = "CCS_CampDefinition",
        menuName = "CCS/Survival/Shelter/Camp Definition")]
    public sealed class CCS_CampDefinition : CCS_SurvivalProfileBase
    {
        [SerializeField] private bool enableCampTracking = true;
        [SerializeField] private float campDetectionRadius = 12f;
        [SerializeField] private string campfirePieceId = "ccs.survival.building.campfire.test";
        [SerializeField] private CCS_ShelterDefinition[] shelterDefinitions = System.Array.Empty<CCS_ShelterDefinition>();

        public bool EnableCampTracking => enableCampTracking;

        public float CampDetectionRadius => campDetectionRadius < 1f ? 1f : campDetectionRadius;

        public string CampfirePieceId => campfirePieceId ?? string.Empty;

        public IReadOnlyList<CCS_ShelterDefinition> ShelterDefinitions => shelterDefinitions;

        public bool TryGetShelterById(string shelterDefinitionId, out CCS_ShelterDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(shelterDefinitionId) || shelterDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < shelterDefinitions.Length; index++)
            {
                CCS_ShelterDefinition candidate = shelterDefinitions[index];
                if (candidate != null
                    && string.Equals(candidate.ShelterDefinitionId, shelterDefinitionId, System.StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetByPlaceableItemId(string itemId, out CCS_ShelterDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(itemId) || shelterDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < shelterDefinitions.Length; index++)
            {
                CCS_ShelterDefinition candidate = shelterDefinitions[index];
                if (candidate?.PlaceableKitItem != null
                    && string.Equals(candidate.PlaceableKitItem.ItemId, itemId, System.StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
