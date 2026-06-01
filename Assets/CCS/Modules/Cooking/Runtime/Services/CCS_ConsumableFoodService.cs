using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Inventory;
using CCS.Modules.SurvivalCore;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ConsumableFoodService
// CATEGORY: Modules / Cooking / Runtime / Services
// PURPOSE: Consumes inventory food items and restores hunger through Survival Core.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration from cooking profile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No health restore or buffs in 0.9.4 foundation.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_ConsumableFoodService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_ConsumableFoodService]";

        #region Variables

        private CCS_CookingProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_SurvivalCoreService survivalCoreService;
        private bool isInitialized;

        #endregion

        #region Events

        public event FoodConsumedHandler FoodConsumed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_CookingProfile ActiveProfile => activeProfile;

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

        public void InitializeFromProfile(CCS_CookingProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_CookingValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            isInitialized = true;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public void BindSurvivalCoreService(CCS_SurvivalCoreService service)
        {
            survivalCoreService = service;
        }

        public bool CanConsume(CCS_ItemDefinition itemDefinition)
        {
            return ValidateConsumption(itemDefinition).IsSuccess;
        }

        public CCS_ConsumableFoodResult TryConsumeFood(CCS_ItemDefinition itemDefinition)
        {
            if (!EnsureInitialized())
            {
                return CCS_ConsumableFoodResult.Failure("Consumable food service is not initialized.");
            }

            CCS_SurvivalValidationResult validation = ValidateConsumption(itemDefinition);
            if (!validation.IsSuccess)
            {
                return CCS_ConsumableFoodResult.Failure(validation.Message);
            }

            float hungerRestoreAmount = ResolveHungerRestoreAmount(itemDefinition);
            if (inventoryService.RemoveItem(itemDefinition, 1) < 1)
            {
                return CCS_ConsumableFoodResult.Failure("Failed to remove food item from inventory.");
            }

            CCS_Result modifierResult = survivalCoreService.TryApplyModifier(
                CCS_SurvivalStatType.Hunger,
                CCS_SurvivalStatModifier.Add(hungerRestoreAmount));

            if (!modifierResult.IsSuccess)
            {
                inventoryService.AddItem(itemDefinition, 1);
                return CCS_ConsumableFoodResult.Failure("Failed to restore hunger.");
            }

            RaiseFoodConsumed(itemDefinition, hungerRestoreAmount);
            return CCS_ConsumableFoodResult.Success(
                itemDefinition,
                hungerRestoreAmount,
                "Food consumed.");
        }

        public CCS_ConsumableFoodResult TryConsumeFirstAvailableFood()
        {
            if (!EnsureInitialized() || activeProfile == null)
            {
                return CCS_ConsumableFoodResult.Failure("Consumable food service is not initialized.");
            }

            IReadOnlyList<CCS_ConsumableFoodDefinition> consumableDefinitions = activeProfile.ConsumableFoodDefinitions;
            if (consumableDefinitions == null || consumableDefinitions.Count == 0)
            {
                return CCS_ConsumableFoodResult.Failure("No consumable food definitions configured.");
            }

            for (int index = 0; index < consumableDefinitions.Count; index++)
            {
                CCS_ConsumableFoodDefinition consumableDefinition = consumableDefinitions[index];
                CCS_ItemDefinition itemDefinition = consumableDefinition?.ItemDefinition;
                if (itemDefinition == null)
                {
                    continue;
                }

                if (inventoryService == null
                    || !inventoryService.IsInitialized
                    || inventoryService.GetQuantity(itemDefinition) <= 0)
                {
                    continue;
                }

                return TryConsumeFood(itemDefinition);
            }

            return CCS_ConsumableFoodResult.Failure("No consumable food available in inventory.");
        }

        public float ResolveHungerRestoreAmount(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null || activeProfile == null)
            {
                return 0f;
            }

            IReadOnlyList<CCS_ConsumableFoodDefinition> consumableDefinitions = activeProfile.ConsumableFoodDefinitions;
            if (consumableDefinitions == null)
            {
                return 0f;
            }

            for (int index = 0; index < consumableDefinitions.Count; index++)
            {
                CCS_ConsumableFoodDefinition consumableDefinition = consumableDefinitions[index];
                if (consumableDefinition?.ItemDefinition == itemDefinition)
                {
                    return consumableDefinition.HungerRestoreAmount;
                }
            }

            return 0f;
        }

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            return isInitialized;
        }

        private CCS_SurvivalValidationResult ValidateConsumption(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Food item definition is null.");
            }

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                return CCS_SurvivalValidationResult.Fail("Inventory service is not initialized.");
            }

            if (survivalCoreService == null || !survivalCoreService.IsInitialized)
            {
                return CCS_SurvivalValidationResult.Fail("Survival core service is not initialized.");
            }

            if (inventoryService.GetQuantity(itemDefinition) <= 0)
            {
                return CCS_SurvivalValidationResult.Fail("Food item is not available in inventory.");
            }

            float hungerRestoreAmount = ResolveHungerRestoreAmount(itemDefinition);
            if (hungerRestoreAmount <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Food item is not configured as consumable.");
            }

            return CCS_SurvivalValidationResult.Pass("Food consumption validated.");
        }

        private void RaiseFoodConsumed(CCS_ItemDefinition itemDefinition, float hungerRestored)
        {
            FoodConsumed?.Invoke(
                new CCS_CookingEventArgs(
                    itemDefinition: itemDefinition,
                    message: $"Ate {itemDefinition.DisplayName} (+{hungerRestored:0} hunger)."));
        }

        #endregion
    }
}
