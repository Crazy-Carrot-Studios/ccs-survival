using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StarterLoadoutService
// CATEGORY: Survival / Runtime / Player / Loadout
// PURPOSE: Applies starter inventory grants once on fresh runtime starts.
// PLACEMENT: Registered by survival gameplay composition after inventory service exists.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Skips application when inventory already contains items (save restore path).
// =============================================================================

namespace CCS.Survival.Player.Loadout
{
    public sealed class CCS_StarterLoadoutService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_StarterLoadoutService]";

        #region Variables

        private CCS_StarterLoadoutProfile activeProfile;
        private bool isInitialized;
        private bool loadoutApplied;

        #endregion

        #region Events

        public event StarterLoadoutAppliedHandler LoadoutApplied;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public bool HasAppliedStarterLoadout => loadoutApplied;

        public CCS_StarterLoadoutProfile ActiveProfile => activeProfile;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_StarterLoadoutProfile profile)
        {
            activeProfile = profile;
            isInitialized = true;
        }

        public void RegisterPrimitiveRecipes(CCS_CraftingService craftingService)
        {
            if (craftingService == null || !craftingService.IsInitialized || activeProfile == null)
            {
                return;
            }

            RegisterRecipeList(craftingService, activeProfile.PrimitiveRecipes);
            RegisterRecipeList(craftingService, activeProfile.BoneToolRecipes);
        }

        private static void RegisterRecipeList(
            CCS_CraftingService craftingService,
            CCS_CraftingRecipeDefinition[] recipes)
        {
            if (recipes == null)
            {
                return;
            }

            for (int index = 0; index < recipes.Length; index++)
            {
                craftingService.RegisterDefaultUnlockedRecipe(recipes[index]);
            }
        }

        public bool TryApplyStarterLoadout(CCS_PlayerInventoryService inventoryService)
        {
            if (loadoutApplied || activeProfile == null || inventoryService == null || !inventoryService.IsInitialized)
            {
                return false;
            }

            if (activeProfile.ApplyWhenInventoryEmpty)
            {
                CCS_InventorySnapshot snapshot = inventoryService.CreateSnapshot();
                if (snapshot.UsedSlotCount > 0)
                {
                    return false;
                }
            }

            int grantedItemCount = 0;

            CCS_StarterLoadoutEntry[] startingItems = activeProfile.StartingItems;
            for (int index = 0; index < startingItems.Length; index++)
            {
                CCS_StarterLoadoutEntry entry = startingItems[index];
                if (entry == null || entry.ItemDefinition == null || entry.Quantity <= 0)
                {
                    continue;
                }

                int added = inventoryService.AddItem(entry.ItemDefinition, entry.Quantity);
                if (added > 0)
                {
                    grantedItemCount += added;
                }
            }

            int grantedCurrencyAmount = 0;
            if (activeProfile.CurrencyItemDefinition != null && activeProfile.StartingCurrencyAmount > 0)
            {
                grantedCurrencyAmount = inventoryService.AddItem(
                    activeProfile.CurrencyItemDefinition,
                    activeProfile.StartingCurrencyAmount);
            }

            loadoutApplied = grantedItemCount > 0 || grantedCurrencyAmount > 0;
            if (loadoutApplied)
            {
                LoadoutApplied?.Invoke(new CCS_StarterLoadoutAppliedEventArgs(grantedItemCount, grantedCurrencyAmount));
                Debug.Log(
                    $"{LogPrefix} Applied starter loadout — items={grantedItemCount}, currency={grantedCurrencyAmount}.");
            }

            return loadoutApplied;
        }

        #endregion
    }
}
