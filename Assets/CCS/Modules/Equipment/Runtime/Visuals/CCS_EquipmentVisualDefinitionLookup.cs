using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_EquipmentVisualDefinitionLookup
// CATEGORY: Modules / Equipment / Runtime / Visuals
// PURPOSE: Fast item ID lookup for equipment visual definitions.
// PLACEMENT: Built from CCS_EquipmentVisualProfile at runtime bind.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Last duplicate item ID wins during build; validation should catch duplicates.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public sealed class CCS_EquipmentVisualDefinitionLookup
    {
        #region Variables

        private readonly Dictionary<string, CCS_EquipmentVisualDefinition> definitionsByItemId =
            new Dictionary<string, CCS_EquipmentVisualDefinition>();

        #endregion

        public CCS_EquipmentVisualDefinitionLookup(IReadOnlyList<CCS_EquipmentVisualDefinition> definitions)
        {
            if (definitions == null)
            {
                return;
            }

            for (int index = 0; index < definitions.Count; index++)
            {
                CCS_EquipmentVisualDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.ItemId))
                {
                    continue;
                }

                definitionsByItemId[definition.ItemId] = definition;
            }
        }

        public bool TryGetDefinition(string itemId, out CCS_EquipmentVisualDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            return definitionsByItemId.TryGetValue(itemId, out definition);
        }

        public IReadOnlyCollection<CCS_EquipmentVisualDefinition> AllDefinitions => definitionsByItemId.Values;
    }
}
