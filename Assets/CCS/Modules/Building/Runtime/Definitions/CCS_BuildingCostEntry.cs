using System;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingCostEntry
// CATEGORY: Modules / Building / Runtime / Definitions
// PURPOSE: Serializable build cost entry referencing inventory item definitions.
// PLACEMENT: Serialized on CCS_BuildingPieceDefinition assets.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Consumed by placement validation and inventory integration in 0.8.2.
// =============================================================================

namespace CCS.Modules.Building
{
    [Serializable]
    public sealed class CCS_BuildingCostEntry
    {
        #region Variables

        [Tooltip("Inventory item consumed when placing this building piece.")]
        [SerializeField] private CCS_ItemDefinition itemDefinition;

        [Tooltip("Quantity consumed per placement.")]
        [SerializeField] private int quantity = 1;

        #endregion

        #region Properties

        public CCS_ItemDefinition ItemDefinition => itemDefinition;

        public int Quantity => quantity < 0 ? 0 : quantity;

        #endregion
    }
}
