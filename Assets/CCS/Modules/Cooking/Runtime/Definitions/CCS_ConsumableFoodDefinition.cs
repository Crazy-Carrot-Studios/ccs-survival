using System;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ConsumableFoodDefinition
// CATEGORY: Modules / Cooking / Runtime / Definitions
// PURPOSE: Maps inventory food items to hunger restoration values.
// PLACEMENT: Serialized on CCS_CookingProfile assets.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No health restore or buffs in 0.9.4 foundation.
// =============================================================================

namespace CCS.Modules.Cooking
{
    [Serializable]
    public sealed class CCS_ConsumableFoodDefinition
    {
        #region Variables

        [Tooltip("Inventory item that may be consumed.")]
        [SerializeField] private CCS_ItemDefinition itemDefinition;

        [Tooltip("Hunger restored when one unit is consumed.")]
        [SerializeField] private float hungerRestoreAmount = 10f;

        #endregion

        #region Properties

        public CCS_ItemDefinition ItemDefinition => itemDefinition;

        public float HungerRestoreAmount => hungerRestoreAmount;

        #endregion
    }
}
