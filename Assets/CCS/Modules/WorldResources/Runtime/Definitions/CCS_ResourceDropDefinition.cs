using System;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ResourceDropDefinition
// CATEGORY: Modules / WorldResources / Runtime / Definitions
// PURPOSE: Serializable drop entry referencing inventory item definitions.
// PLACEMENT: Serialized on CCS_ResourceDefinition assets.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI references. Quantities validated by world resource validation utility.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    [Serializable]
    public sealed class CCS_ResourceDropDefinition
    {
        #region Variables

        [Tooltip("Inventory item granted when this drop rolls successfully.")]
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
