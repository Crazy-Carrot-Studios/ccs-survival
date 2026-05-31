using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_ItemDefinitionLookup
// CATEGORY: Modules / Inventory / Runtime / Definitions
// PURPOSE: Resolves stable item IDs to CCS_ItemDefinition assets for save restore.
// PLACEMENT: Built from profile catalogs during inventory service initialization.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Missing definitions fail safely during restore without corrupting other slots.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public sealed class CCS_ItemDefinitionLookup
    {
        #region Variables

        private readonly Dictionary<string, CCS_ItemDefinition> definitionsByItemId =
            new Dictionary<string, CCS_ItemDefinition>();

        #endregion

        #region Public Methods

        public CCS_ItemDefinitionLookup(IEnumerable<CCS_ItemDefinition> itemDefinitions)
        {
            if (itemDefinitions == null)
            {
                return;
            }

            foreach (CCS_ItemDefinition itemDefinition in itemDefinitions)
            {
                if (itemDefinition == null || string.IsNullOrWhiteSpace(itemDefinition.ItemId))
                {
                    continue;
                }

                definitionsByItemId[itemDefinition.ItemId] = itemDefinition;
            }
        }

        public bool TryGetDefinition(string itemId, out CCS_ItemDefinition itemDefinition)
        {
            itemDefinition = null;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            return definitionsByItemId.TryGetValue(itemId, out itemDefinition) && itemDefinition != null;
        }

        #endregion

        #region Properties

        public int KnownDefinitionCount => definitionsByItemId.Count;

        #endregion
    }
}
