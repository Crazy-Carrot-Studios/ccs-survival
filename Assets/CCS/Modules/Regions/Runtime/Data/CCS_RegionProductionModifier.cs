using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RegionProductionModifier
// CATEGORY: Modules / Regions / Runtime / Data
// PURPOSE: Production and prosperity modifiers plus preferred contract categories.
// PLACEMENT: Embedded in CCS_RegionDefinition and world simulation profile entries.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.1.0 regional specialization foundation.
// =============================================================================

namespace CCS.Modules.Regions
{
    [Serializable]
    public sealed class CCS_RegionProductionModifier
    {
        [SerializeField] private float productionBonus = 1f;

        [SerializeField] private float prosperityModifier = 1f;

        [SerializeField] private CCS_RegionSpecializationType[] preferredContractCategories =
            Array.Empty<CCS_RegionSpecializationType>();

        public float ProductionBonus => productionBonus < 0f ? 0f : productionBonus;

        public float ProsperityModifier => prosperityModifier < 0f ? 0f : prosperityModifier;

        public CCS_RegionSpecializationType[] PreferredContractCategories =>
            preferredContractCategories ?? Array.Empty<CCS_RegionSpecializationType>();

        public bool PrefersContractCategory(CCS_RegionSpecializationType category)
        {
            if (category == CCS_RegionSpecializationType.Unknown)
            {
                return false;
            }

            CCS_RegionSpecializationType[] categories = PreferredContractCategories;
            if (categories.Length == 0)
            {
                return false;
            }

            for (int index = 0; index < categories.Length; index++)
            {
                if (categories[index] == category)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
