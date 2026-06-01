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
// NOTES: Cooldown, fullness checks, and hunger pacing in 0.9.5. No health restore.
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
        private float lastConsumeTime = float.NegativeInfinity;
        private bool isInitialized;

        #endregion

        #region Events

        public event FoodConsumedHandler FoodConsumed;
        public event FoodConsumeFailedHandler FoodConsumeFailed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_CookingProfile ActiveProfile => activeProfile;

        public float LastConsumeTime => lastConsumeTime;

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
            return ValidateConsumption(itemDefinition, raiseFailureEvent: false).IsSuccess;
        }

        public CCS_ConsumableFoodResult TryConsumeFood(CCS_ItemDefinition itemDefinition)
        {
            return TryConsumeFood(itemDefinition, raiseFailureEvent: true);
        }

        public CCS_ConsumableFoodResult TryConsumeFirstAvailableFood()
        {
            if (!EnsureInitialized() || activeProfile == null)
            {
                return FailConsume(null, "Consumable food service is not initialized.", raiseFailureEvent: true);
            }

            CCS_SurvivalValidationResult readinessValidation = ValidateConsumeReadiness(raiseFailureEvent: true);
            if (!readinessValidation.IsSuccess)
            {
                return CCS_ConsumableFoodResult.Failure(readinessValidation.Message);
            }

            List<CCS_ConsumableFoodDefinition> prioritizedDefinitions = BuildPrioritizedDefinitions();
            if (prioritizedDefinitions.Count == 0)
            {
                return FailConsume(null, "No consumable food available in inventory.", raiseFailureEvent: true);
            }

            for (int index = 0; index < prioritizedDefinitions.Count; index++)
            {
                CCS_ConsumableFoodDefinition consumableDefinition = prioritizedDefinitions[index];
                CCS_ItemDefinition itemDefinition = consumableDefinition?.ItemDefinition;
                if (itemDefinition == null)
                {
                    continue;
                }

                if (inventoryService.GetQuantity(itemDefinition) <= 0)
                {
                    continue;
                }

                CCS_ConsumableFoodResult result = TryConsumeFood(itemDefinition, raiseFailureEvent: false);
                if (result.IsSuccess)
                {
                    return result;
                }
            }

            return FailConsume(null, "No consumable food available in inventory.", raiseFailureEvent: true);
        }

        public float ResolveHungerRestoreAmount(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null || activeProfile == null)
            {
                return 0f;
            }

            if (TryGetConsumableDefinition(itemDefinition, out CCS_ConsumableFoodDefinition consumableDefinition))
            {
                return consumableDefinition.HungerRestoreAmount;
            }

            return 0f;
        }

        public string ResolveNotificationDisplayName(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition != null
                && TryGetConsumableDefinition(itemDefinition, out CCS_ConsumableFoodDefinition consumableDefinition))
            {
                return consumableDefinition.ResolveNotificationDisplayName();
            }

            return itemDefinition != null ? itemDefinition.DisplayName : "Food";
        }

        private CCS_ConsumableFoodResult TryConsumeFood(
            CCS_ItemDefinition itemDefinition,
            bool raiseFailureEvent)
        {
            if (!EnsureInitialized())
            {
                return FailConsume(null, "Consumable food service is not initialized.", raiseFailureEvent);
            }

            CCS_SurvivalValidationResult validation = ValidateConsumption(itemDefinition, raiseFailureEvent);
            if (!validation.IsSuccess)
            {
                return CCS_ConsumableFoodResult.Failure(validation.Message);
            }

            float hungerRestoreAmount = ResolveHungerRestoreAmount(itemDefinition);
            if (inventoryService.RemoveItem(itemDefinition, 1) < 1)
            {
                return FailConsume(itemDefinition, "Failed to remove food item from inventory.", raiseFailureEvent);
            }

            CCS_Result modifierResult = survivalCoreService.TryApplyModifier(
                CCS_SurvivalStatType.Hunger,
                CCS_SurvivalStatModifier.Add(hungerRestoreAmount));

            if (!modifierResult.IsSuccess)
            {
                inventoryService.AddItem(itemDefinition, 1);
                return FailConsume(itemDefinition, "Failed to restore hunger.", raiseFailureEvent);
            }

            lastConsumeTime = Time.time;
            RaiseFoodConsumed(itemDefinition, hungerRestoreAmount);
            return CCS_ConsumableFoodResult.Success(
                itemDefinition,
                hungerRestoreAmount,
                "Food consumed.");
        }

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            return isInitialized;
        }

        private CCS_SurvivalValidationResult ValidateConsumeReadiness(bool raiseFailureEvent)
        {
            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                return FailValidation(null, "Inventory service is not initialized.", raiseFailureEvent);
            }

            if (survivalCoreService == null || !survivalCoreService.IsInitialized)
            {
                return FailValidation(null, "Survival core service is not initialized.", raiseFailureEvent);
            }

            if (IsConsumeCooldownActive())
            {
                return FailValidation(null, "Food consume cooldown is active.", raiseFailureEvent);
            }

            if (!survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Hunger, out CCS_SurvivalStatSnapshot hungerSnapshot))
            {
                return FailValidation(null, "Hunger snapshot is unavailable.", raiseFailureEvent);
            }

            if (CCS_HungerStateUtility.IsHungerFull(hungerSnapshot))
            {
                return FailValidation(null, "Hunger Full", raiseFailureEvent);
            }

            return CCS_SurvivalValidationResult.Pass("Consume readiness validated.");
        }

        private CCS_SurvivalValidationResult ValidateConsumption(
            CCS_ItemDefinition itemDefinition,
            bool raiseFailureEvent)
        {
            if (itemDefinition == null)
            {
                return FailValidation(null, "Food item definition is null.", raiseFailureEvent);
            }

            CCS_SurvivalValidationResult readinessValidation = ValidateConsumeReadiness(raiseFailureEvent);
            if (!readinessValidation.IsSuccess)
            {
                return readinessValidation;
            }

            if (inventoryService.GetQuantity(itemDefinition) <= 0)
            {
                return FailValidation(itemDefinition, "Food item is not available in inventory.", raiseFailureEvent);
            }

            float hungerRestoreAmount = ResolveHungerRestoreAmount(itemDefinition);
            if (hungerRestoreAmount <= 0f)
            {
                return FailValidation(itemDefinition, "Food item is not configured as consumable.", raiseFailureEvent);
            }

            if (!survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Hunger, out CCS_SurvivalStatSnapshot hungerSnapshot))
            {
                return FailValidation(itemDefinition, "Hunger snapshot is unavailable.", raiseFailureEvent);
            }

            if (!CCS_HungerStateUtility.HasRoomForRestore(hungerSnapshot, hungerRestoreAmount))
            {
                return FailValidation(itemDefinition, "Hunger Full", raiseFailureEvent);
            }

            return CCS_SurvivalValidationResult.Pass("Food consumption validated.");
        }

        private List<CCS_ConsumableFoodDefinition> BuildPrioritizedDefinitions()
        {
            List<CCS_ConsumableFoodDefinition> prioritizedDefinitions = new List<CCS_ConsumableFoodDefinition>();
            IReadOnlyList<CCS_ConsumableFoodDefinition> consumableDefinitions = activeProfile.ConsumableFoodDefinitions;
            if (consumableDefinitions == null)
            {
                return prioritizedDefinitions;
            }

            for (int index = 0; index < consumableDefinitions.Count; index++)
            {
                CCS_ConsumableFoodDefinition consumableDefinition = consumableDefinitions[index];
                if (consumableDefinition?.ItemDefinition == null)
                {
                    continue;
                }

                prioritizedDefinitions.Add(consumableDefinition);
            }

            prioritizedDefinitions.Sort((left, right) =>
                right.HungerRestoreAmount.CompareTo(left.HungerRestoreAmount));

            return prioritizedDefinitions;
        }

        private bool TryGetConsumableDefinition(
            CCS_ItemDefinition itemDefinition,
            out CCS_ConsumableFoodDefinition consumableDefinition)
        {
            consumableDefinition = null;
            if (itemDefinition == null || activeProfile == null)
            {
                return false;
            }

            IReadOnlyList<CCS_ConsumableFoodDefinition> consumableDefinitions = activeProfile.ConsumableFoodDefinitions;
            if (consumableDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < consumableDefinitions.Count; index++)
            {
                CCS_ConsumableFoodDefinition candidate = consumableDefinitions[index];
                if (candidate?.ItemDefinition == itemDefinition)
                {
                    consumableDefinition = candidate;
                    return true;
                }
            }

            return false;
        }

        private bool IsConsumeCooldownActive()
        {
            float cooldownSeconds = ResolveConsumeCooldownSeconds(null);
            if (cooldownSeconds <= 0f)
            {
                return false;
            }

            return Time.time - lastConsumeTime < cooldownSeconds;
        }

        private float ResolveConsumeCooldownSeconds(CCS_ConsumableFoodDefinition consumableDefinition)
        {
            if (consumableDefinition != null && consumableDefinition.ConsumeCooldownSeconds > 0f)
            {
                return consumableDefinition.ConsumeCooldownSeconds;
            }

            if (survivalCoreService?.ActiveProfile != null)
            {
                return survivalCoreService.ActiveProfile.HungerConsumeCooldownSeconds;
            }

            return 1f;
        }

        private CCS_SurvivalValidationResult FailValidation(
            CCS_ItemDefinition itemDefinition,
            string message,
            bool raiseFailureEvent)
        {
            if (raiseFailureEvent)
            {
                RaiseFoodConsumeFailed(itemDefinition, message);
            }

            return CCS_SurvivalValidationResult.Fail(message);
        }

        private CCS_ConsumableFoodResult FailConsume(
            CCS_ItemDefinition itemDefinition,
            string message,
            bool raiseFailureEvent)
        {
            if (raiseFailureEvent)
            {
                RaiseFoodConsumeFailed(itemDefinition, message);
            }

            return CCS_ConsumableFoodResult.Failure(message);
        }

        private void RaiseFoodConsumed(CCS_ItemDefinition itemDefinition, float hungerRestored)
        {
            string displayName = ResolveNotificationDisplayName(itemDefinition);
            FoodConsumed?.Invoke(
                new CCS_CookingEventArgs(
                    itemDefinition: itemDefinition,
                    message: $"Ate {displayName} (+{hungerRestored:0} Hunger)."));
        }

        private void RaiseFoodConsumeFailed(CCS_ItemDefinition itemDefinition, string message)
        {
            FoodConsumeFailed?.Invoke(
                new CCS_CookingEventArgs(
                    itemDefinition: itemDefinition,
                    message: message ?? string.Empty));
        }

        #endregion
    }
}
