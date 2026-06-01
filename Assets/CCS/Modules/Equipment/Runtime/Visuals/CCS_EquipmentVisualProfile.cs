using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentVisualProfile
// CATEGORY: Modules / Equipment / Runtime / Visuals
// PURPOSE: Catalog of equipment visual definitions for runtime spawn/remove sync.
// PLACEMENT: Survival/Profiles/Equipment/CCS_DefaultEquipmentVisualProfile.asset
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Keeps visual data separate from gameplay equipment rules profile.
// =============================================================================

namespace CCS.Modules.Equipment
{
    [CreateAssetMenu(
        fileName = "CCS_EquipmentVisualProfile",
        menuName = "CCS/Survival/Equipment/Equipment Visual Profile")]
    public sealed class CCS_EquipmentVisualProfile : ScriptableObject
    {
        #region Variables

        [SerializeField] private string profileId = "ccs.survival.profile.equipment.visual.default";
        [SerializeField] private CCS_EquipmentVisualDefinition[] visualDefinitions = System.Array.Empty<CCS_EquipmentVisualDefinition>();

        #endregion

        #region Properties

        public string ProfileId => profileId;

        public IReadOnlyList<CCS_EquipmentVisualDefinition> VisualDefinitions => visualDefinitions;

        #endregion

        public CCS_EquipmentVisualDefinitionLookup BuildLookup()
        {
            return new CCS_EquipmentVisualDefinitionLookup(visualDefinitions);
        }
    }
}
