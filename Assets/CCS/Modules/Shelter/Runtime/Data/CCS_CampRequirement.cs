using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CampRequirement
// CATEGORY: Modules / Shelter / Runtime / Data
// PURPOSE: Single structure requirement entry for a camp tier definition.
// PLACEMENT: Serialized on CCS_CampTierDefinition assets.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Shelter
{
    [Serializable]
    public sealed class CCS_CampRequirement
    {
        [SerializeField] private CCS_CampStructureKind structureKind = CCS_CampStructureKind.None;
        [SerializeField] private string structureDefinitionId = string.Empty;

        public CCS_CampStructureKind StructureKind => structureKind;

        public string StructureDefinitionId => structureDefinitionId ?? string.Empty;

        public bool RequiresSpecificDefinition => !string.IsNullOrWhiteSpace(structureDefinitionId);
    }
}
