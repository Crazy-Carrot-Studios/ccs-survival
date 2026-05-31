using System;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StarterLoadoutEntry
// CATEGORY: Survival / Runtime / Player / Loadout
// PURPOSE: Serializable starter inventory grant entry for loadout profiles.
// PLACEMENT: Embedded on CCS_StarterLoadoutProfile.startingItems.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: No vendor or economy logic in 0.9.1.
// =============================================================================

namespace CCS.Survival.Player.Loadout
{
    [Serializable]
    public sealed class CCS_StarterLoadoutEntry
    {
        #region Variables

        [Tooltip("Item definition granted at runtime startup.")]
        [SerializeField] private CCS_ItemDefinition itemDefinition;

        [Tooltip("Quantity granted for the item definition.")]
        [SerializeField] private int quantity = 1;

        #endregion

        #region Properties

        public CCS_ItemDefinition ItemDefinition => itemDefinition;

        public int Quantity => quantity;

        #endregion
    }
}
