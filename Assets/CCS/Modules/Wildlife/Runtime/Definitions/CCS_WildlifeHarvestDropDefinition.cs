using System;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeHarvestDropDefinition
// CATEGORY: Modules / Wildlife / Runtime / Definitions
// PURPOSE: Serializable drop entry for wildlife harvest definitions.
// PLACEMENT: Serialized on CCS_WildlifeDefinition assets.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No cooking or nutrition logic in 0.9.3 foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    [Serializable]
    public sealed class CCS_WildlifeHarvestDropDefinition
    {
        #region Variables

        [Tooltip("Inventory item granted when this wildlife drop rolls successfully.")]
        [SerializeField] private CCS_ItemDefinition itemDefinition;

        [Tooltip("Minimum quantity granted for this drop entry.")]
        [SerializeField] private int minQuantity = 1;

        [Tooltip("Maximum quantity granted for this drop entry.")]
        [SerializeField] private int maxQuantity = 1;

        #endregion

        #region Properties

        public CCS_ItemDefinition ItemDefinition => itemDefinition;

        public int MinQuantity => minQuantity;

        public int MaxQuantity => maxQuantity;

        #endregion
    }
}
