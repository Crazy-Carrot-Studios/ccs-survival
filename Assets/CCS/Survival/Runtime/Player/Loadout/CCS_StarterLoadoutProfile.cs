using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StarterLoadoutProfile
// CATEGORY: Survival / Runtime / Player / Loadout
// PURPOSE: Defines starter inventory grants and primitive recipe registration for new games.
// PLACEMENT: Assets/CCS/Survival/Profiles/StarterLoadout/
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Apply only when inventory is empty unless overridden by profile flag.
// =============================================================================

namespace CCS.Survival.Player.Loadout
{
    [CreateAssetMenu(
        fileName = "CCS_StarterLoadoutProfile",
        menuName = "CCS/Survival/Player/Starter Loadout Profile")]
    public sealed class CCS_StarterLoadoutProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Starter Grants")]
        [Tooltip("Items granted on a fresh runtime start when loadout rules allow application.")]
        [SerializeField] private CCS_StarterLoadoutEntry[] startingItems = System.Array.Empty<CCS_StarterLoadoutEntry>();

        [Tooltip("Currency placeholder item used for starting coin grants.")]
        [SerializeField] private CCS_ItemDefinition currencyItemDefinition;

        [Tooltip("Starting currency amount granted using the currency item definition.")]
        [SerializeField] private int startingCurrencyAmount = 10;

        [Header("Application Rules")]
        [Tooltip("When enabled, starter loadout applies only if inventory has no occupied slots.")]
        [SerializeField] private bool applyWhenInventoryEmpty = true;

        [Header("Primitive Progression")]
        [Tooltip("Hand recipes unlocked at startup for early-game crafting progression.")]
        [SerializeField] private CCS_CraftingRecipeDefinition[] primitiveRecipes =
            System.Array.Empty<CCS_CraftingRecipeDefinition>();

        [Tooltip("Bone tool recipes unlocked at startup for primitive equipment progression.")]
        [SerializeField] private CCS_CraftingRecipeDefinition[] boneToolRecipes =
            System.Array.Empty<CCS_CraftingRecipeDefinition>();

        #endregion

        #region Properties

        public CCS_StarterLoadoutEntry[] StartingItems => startingItems;

        public CCS_ItemDefinition CurrencyItemDefinition => currencyItemDefinition;

        public int StartingCurrencyAmount => startingCurrencyAmount;

        public bool ApplyWhenInventoryEmpty => applyWhenInventoryEmpty;

        public CCS_CraftingRecipeDefinition[] PrimitiveRecipes => primitiveRecipes;

        public CCS_CraftingRecipeDefinition[] BoneToolRecipes => boneToolRecipes;

        #endregion
    }
}
