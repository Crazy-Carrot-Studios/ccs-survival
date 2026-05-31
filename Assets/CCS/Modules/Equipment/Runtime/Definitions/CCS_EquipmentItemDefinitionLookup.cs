using System.Collections.Generic;
using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_EquipmentItemDefinitionLookup
// CATEGORY: Modules / Equipment / Runtime / Definitions
// PURPOSE: Resolves item IDs to equipment definitions for save restore.
// PLACEMENT: Built from profile catalogs during equipment service initialization.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Invalid slot mappings and missing definitions fail safely during restore.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public sealed class CCS_EquipmentItemDefinitionLookup
    {
        #region Variables

        private readonly Dictionary<string, CCS_EquipmentItemDefinition> definitionsByItemId =
            new Dictionary<string, CCS_EquipmentItemDefinition>();

        #endregion

        #region Public Methods

        public CCS_EquipmentItemDefinitionLookup(IEnumerable<CCS_EquipmentItemDefinition> equipmentDefinitions)
        {
            if (equipmentDefinitions == null)
            {
                return;
            }

            foreach (CCS_EquipmentItemDefinition equipmentDefinition in equipmentDefinitions)
            {
                if (equipmentDefinition == null)
                {
                    continue;
                }

                CCS_ItemDefinition itemDefinition = equipmentDefinition.ItemDefinition;
                if (itemDefinition == null || string.IsNullOrWhiteSpace(itemDefinition.ItemId))
                {
                    continue;
                }

                definitionsByItemId[itemDefinition.ItemId] = equipmentDefinition;
            }
        }

        public bool TryGetDefinitionByItemId(string itemId, out CCS_EquipmentItemDefinition equipmentDefinition)
        {
            equipmentDefinition = null;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            return definitionsByItemId.TryGetValue(itemId, out equipmentDefinition)
                && equipmentDefinition != null;
        }

        #endregion

        #region Properties

        public int KnownDefinitionCount => definitionsByItemId.Count;

        #endregion
    }
}
