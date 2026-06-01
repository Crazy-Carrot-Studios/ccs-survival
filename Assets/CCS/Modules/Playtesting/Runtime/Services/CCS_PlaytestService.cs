using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Building;
using CCS.Modules.Combat;
using CCS.Modules.Cooking;
using CCS.Modules.Equipment;
using CCS.Modules.Gathering;
using CCS.Modules.Inventory;
using CCS.Modules.PlayerDeath;
using CCS.Modules.SaveSystem;
using CCS.Modules.SurvivalCore;
using CCS.Modules.Wildlife;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlaytestService
// CATEGORY: Modules / Playtesting / Runtime / Services
// PURPOSE: Manual bootstrap playtest checklist state and event-driven step completion.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Development harness only. Subscribes to module events through explicit bind calls.
// =============================================================================

namespace CCS.Modules.Playtesting
{
    public sealed class CCS_PlaytestService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_PlaytestService]";

        private const string StickItemId = "ccs.survival.item.resource.stick";
        private const string WoodItemId = "ccs.survival.item.resource.wood";
        private const string SpearItemId = "ccs.survival.item.starter.spear";
        private const string CookedRabbitItemId = "ccs.survival.item.food.cookedrabbitmeat";
        private const string CookedVenisonItemId = "ccs.survival.item.food.cookedvenison";

        #region Variables

        private readonly List<CCS_PlaytestStepState> stepStates = new List<CCS_PlaytestStepState>();

        private CCS_PlaytestProfile activeProfile;
        private CCS_SurvivalCoreService survivalCoreService;
        private bool isInitialized;
        private bool harnessEnabled;
        private int activeStepIndex = -1;
        private bool eventsBound;
        private CCS_GatheringService boundGatheringService;
        private CCS_CombatService boundCombatService;
        private CCS_WildlifeHarvestService boundWildlifeHarvestService;
        private CCS_CookingService boundCookingService;
        private CCS_ConsumableFoodService boundConsumableFoodService;
        private CCS_SaveService boundSaveService;
        private CCS_PlayerDeathService boundPlayerDeathService;
        private CCS_BuildingPlacementService boundBuildingPlacementService;
        private CCS_PlayerEquipmentService boundEquipmentService;

        #endregion

        #region Events

        public event PlaytestStepChangedHandler PlaytestStepChanged;
        public event PlaytestStepPassedHandler PlaytestStepPassed;
        public event PlaytestStepFailedHandler PlaytestStepFailed;
        public event PlaytestResetHandler PlaytestReset;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public bool HarnessEnabled => harnessEnabled;

        public CCS_PlaytestProfile ActiveProfile => activeProfile;

        public int ActiveStepIndex => activeStepIndex;

        public IReadOnlyList<CCS_PlaytestStepState> StepStates => stepStates;

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

        public void InitializeFromProfile(CCS_PlaytestProfile profile)
        {
            activeProfile = profile;
            harnessEnabled = profile != null && profile.EnableHarness;
            RegisterStepDefinitions(profile);
            isInitialized = true;

            if (harnessEnabled && profile.ResetStepStateOnPlayStart)
            {
                ResetSteps();
            }
        }

        public void BindEventListeners(
            CCS_GatheringService gatheringService,
            CCS_CombatService combatService,
            CCS_WildlifeHarvestService wildlifeHarvestService,
            CCS_CookingService cookingService,
            CCS_ConsumableFoodService consumableFoodService,
            CCS_SaveService saveService,
            CCS_PlayerDeathService playerDeathService,
            CCS_BuildingPlacementService buildingPlacementService,
            CCS_PlayerEquipmentService equipmentService,
            CCS_SurvivalCoreService survivalCore)
        {
            UnbindEventListeners();
            survivalCoreService = survivalCore;

            if (!harnessEnabled)
            {
                return;
            }

            boundGatheringService = gatheringService;
            boundCombatService = combatService;
            boundWildlifeHarvestService = wildlifeHarvestService;
            boundCookingService = cookingService;
            boundConsumableFoodService = consumableFoodService;
            boundSaveService = saveService;
            boundPlayerDeathService = playerDeathService;
            boundBuildingPlacementService = buildingPlacementService;
            boundEquipmentService = equipmentService;

            if (boundGatheringService != null)
            {
                boundGatheringService.GatheringNodeGathered += HandleGatheringNodeGathered;
            }

            if (boundCombatService != null)
            {
                boundCombatService.WildlifeKilled += HandleWildlifeKilled;
            }

            if (boundWildlifeHarvestService != null)
            {
                boundWildlifeHarvestService.WildlifeHarvestCompleted += HandleWildlifeHarvestCompleted;
            }

            if (boundCookingService != null)
            {
                boundCookingService.CookingCompleted += HandleCookingCompleted;
            }

            if (boundConsumableFoodService != null)
            {
                boundConsumableFoodService.FoodConsumed += HandleFoodConsumed;
            }

            if (boundSaveService != null)
            {
                boundSaveService.SaveCompleted += HandleSaveCompleted;
                boundSaveService.LoadCompleted += HandleLoadCompleted;
            }

            if (boundPlayerDeathService != null)
            {
                boundPlayerDeathService.PlayerDied += HandlePlayerDied;
                boundPlayerDeathService.PlayerRespawned += HandlePlayerRespawned;
            }

            if (boundBuildingPlacementService != null)
            {
                boundBuildingPlacementService.BuildingPlaced += HandleBuildingPlaced;
            }

            if (boundEquipmentService != null)
            {
                boundEquipmentService.ItemEquipped += HandleItemEquipped;
            }

            eventsBound = true;
        }

        public void NotifyBootstrapSessionReady()
        {
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.Spawn,
                "Player spawned in bootstrap scene.");
        }

        public void UnbindEventListeners()
        {
            if (!eventsBound)
            {
                return;
            }

            if (boundGatheringService != null)
            {
                boundGatheringService.GatheringNodeGathered -= HandleGatheringNodeGathered;
            }

            if (boundCombatService != null)
            {
                boundCombatService.WildlifeKilled -= HandleWildlifeKilled;
            }

            if (boundWildlifeHarvestService != null)
            {
                boundWildlifeHarvestService.WildlifeHarvestCompleted -= HandleWildlifeHarvestCompleted;
            }

            if (boundCookingService != null)
            {
                boundCookingService.CookingCompleted -= HandleCookingCompleted;
            }

            if (boundConsumableFoodService != null)
            {
                boundConsumableFoodService.FoodConsumed -= HandleFoodConsumed;
            }

            if (boundSaveService != null)
            {
                boundSaveService.SaveCompleted -= HandleSaveCompleted;
                boundSaveService.LoadCompleted -= HandleLoadCompleted;
            }

            if (boundPlayerDeathService != null)
            {
                boundPlayerDeathService.PlayerDied -= HandlePlayerDied;
                boundPlayerDeathService.PlayerRespawned -= HandlePlayerRespawned;
            }

            if (boundBuildingPlacementService != null)
            {
                boundBuildingPlacementService.BuildingPlaced -= HandleBuildingPlaced;
            }

            if (boundEquipmentService != null)
            {
                boundEquipmentService.ItemEquipped -= HandleItemEquipped;
            }

            boundGatheringService = null;
            boundCombatService = null;
            boundWildlifeHarvestService = null;
            boundCookingService = null;
            boundConsumableFoodService = null;
            boundSaveService = null;
            boundPlayerDeathService = null;
            boundBuildingPlacementService = null;
            boundEquipmentService = null;
            eventsBound = false;
        }

        public void ResetSteps()
        {
            for (int index = 0; index < stepStates.Count; index++)
            {
                stepStates[index].Reset();
            }

            activeStepIndex = stepStates.Count > 0 ? 0 : -1;
            if (activeStepIndex >= 0)
            {
                stepStates[activeStepIndex].SetStatus(CCS_PlaytestStepStatus.Active);
                RaiseStepChanged(stepStates[activeStepIndex], "Checklist reset.");
            }

            PlaytestReset?.Invoke();
            LogDebug("Playtest checklist reset.");
        }

        public void SetStepActive(string stepId)
        {
            int index = FindStepIndex(stepId);
            if (index < 0)
            {
                return;
            }

            SetActiveStepIndex(index, "Step activated manually.");
        }

        public void MarkStepPassed(string stepId, string message = "")
        {
            int index = FindStepIndex(stepId);
            if (index < 0)
            {
                return;
            }

            CompleteStep(index, message, failed: false);
        }

        public void MarkStepFailed(string stepId, string message = "")
        {
            int index = FindStepIndex(stepId);
            if (index < 0)
            {
                return;
            }

            CCS_PlaytestStepState state = stepStates[index];
            state.SetStatus(CCS_PlaytestStepStatus.Failed);
            RaiseStepFailed(state, message);
            AdvanceToNextStep();
        }

        public void MarkStepSkipped(string stepId, string message = "")
        {
            int index = FindStepIndex(stepId);
            if (index < 0)
            {
                return;
            }

            CCS_PlaytestStepState state = stepStates[index];
            state.SetStatus(CCS_PlaytestStepStatus.Skipped);
            RaiseStepChanged(state, string.IsNullOrWhiteSpace(message) ? "Step skipped." : message);
            AdvanceToNextStep();
        }

        public void AdvanceActiveStep()
        {
            if (activeStepIndex < 0 || activeStepIndex >= stepStates.Count)
            {
                return;
            }

            CompleteStep(activeStepIndex, "Step advanced manually.", failed: false);
        }

        public void SkipActiveStep()
        {
            if (activeStepIndex < 0 || activeStepIndex >= stepStates.Count)
            {
                return;
            }

            MarkStepSkipped(stepStates[activeStepIndex].Definition.StepId, "Step skipped with F11.");
        }

        public void ForceTestDeathCondition()
        {
            if (!harnessEnabled || survivalCoreService == null || !survivalCoreService.IsInitialized)
            {
                return;
            }

            survivalCoreService.TryRestoreSavedNeeds(0f, 0f, 0f);
            LogDebug("Forced hunger and thirst to zero for death playtest (F7).");
        }

        public void Tick(float deltaTime)
        {
            if (!harnessEnabled || activeStepIndex < 0 || activeStepIndex >= stepStates.Count)
            {
                return;
            }

            CCS_PlaytestStepState activeState = stepStates[activeStepIndex];
            activeState.TickActive(deltaTime);
            if (activeState.HasTimedOut)
            {
                MarkStepFailed(activeState.Definition.StepId, "Step timed out.");
            }
        }

        #endregion

        #region Private Methods

        private void RegisterStepDefinitions(CCS_PlaytestProfile profile)
        {
            stepStates.Clear();
            if (profile?.StepDefinitions == null)
            {
                return;
            }

            for (int index = 0; index < profile.StepDefinitions.Count; index++)
            {
                CCS_PlaytestStepDefinition definition = profile.StepDefinitions[index];
                if (definition == null)
                {
                    continue;
                }

                stepStates.Add(new CCS_PlaytestStepState(definition));
            }
        }

        private void HandleGatheringNodeGathered(CCS_GatheringEventArgs eventArgs)
        {
            if (eventArgs?.Rewards == null)
            {
                return;
            }

            for (int index = 0; index < eventArgs.Rewards.Count; index++)
            {
                CCS_GatheringReward reward = eventArgs.Rewards[index];
                if (IsGatherResourceItem(reward.itemDefinitionId))
                {
                    IncrementActiveStepOfType(CCS_PlaytestStepType.GatherResource, reward.itemDefinitionId);
                    return;
                }
            }
        }

        private void HandleWildlifeKilled(CCS_CombatEventArgs eventArgs)
        {
            TryCompleteActiveStepOfType(CCS_PlaytestStepType.HuntWildlife, "Wildlife killed.");
        }

        private void HandleWildlifeHarvestCompleted(CCS_WildlifeEventArgs eventArgs)
        {
            TryCompleteActiveStepOfType(CCS_PlaytestStepType.HarvestCarcass, "Carcass harvested.");
        }

        private void HandleCookingCompleted(CCS_CookingEventArgs eventArgs)
        {
            TryCompleteActiveStepOfType(CCS_PlaytestStepType.CookFood, "Cooking completed.");
        }

        private void HandleFoodConsumed(CCS_CookingEventArgs eventArgs)
        {
            string itemId = eventArgs?.ItemDefinition != null
                ? eventArgs.ItemDefinition.ItemId
                : eventArgs?.CookedItemId ?? string.Empty;
            if (IsCookedFoodItem(itemId))
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.EatFood, "Cooked food consumed.");
            }
        }

        private void HandleSaveCompleted(CCS_SaveEventArgs eventArgs)
        {
            if (eventArgs != null && eventArgs.IsSuccess)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.SaveGame, "Save completed.");
            }
        }

        private void HandleLoadCompleted(CCS_SaveEventArgs eventArgs)
        {
            if (eventArgs != null && eventArgs.IsSuccess)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.LoadGame, "Load completed.");
            }
        }

        private void HandlePlayerDied(CCS_PlayerDeathEventArgs eventArgs)
        {
            TryCompleteActiveStepOfType(CCS_PlaytestStepType.TriggerDeath, eventArgs?.CauseMessage ?? "Player died.");
        }

        private void HandlePlayerRespawned(CCS_PlayerDeathEventArgs eventArgs)
        {
            TryCompleteActiveStepOfType(CCS_PlaytestStepType.Respawn, "Player respawned.");
        }

        private void HandleBuildingPlaced(CCS_BuildingPlacementEventArgs eventArgs)
        {
            TryCompleteActiveStepOfType(CCS_PlaytestStepType.PlaceBuilding, "Building piece placed.");
        }

        private void HandleItemEquipped(CCS_EquipmentEventArgs eventArgs)
        {
            string itemId = eventArgs?.EquipmentDefinition?.ItemDefinition != null
                ? eventArgs.EquipmentDefinition.ItemDefinition.ItemId
                : string.Empty;

            if (MatchesTargetItem(CCS_PlaytestStepType.EquipWeapon, itemId))
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.EquipWeapon, "Spear equipped.");
            }
        }

        private void IncrementActiveStepOfType(CCS_PlaytestStepType stepType, string itemId)
        {
            int index = FindActiveStepIndexOfType(stepType);
            if (index < 0)
            {
                return;
            }

            CCS_PlaytestStepState state = stepStates[index];
            if (!MatchesTargetItem(stepType, itemId))
            {
                return;
            }

            state.AddProgress();
            if (state.HasMetRequiredCount)
            {
                CompleteStep(index, $"Met required count for {state.Definition.DisplayName}.", failed: false);
            }
        }

        private void TryCompleteActiveStepOfType(CCS_PlaytestStepType stepType, string message)
        {
            int index = FindActiveStepIndexOfType(stepType);
            if (index < 0)
            {
                return;
            }

            CompleteStep(index, message, failed: false);
        }

        private void CompleteStep(int index, string message, bool failed)
        {
            if (index < 0 || index >= stepStates.Count)
            {
                return;
            }

            CCS_PlaytestStepState state = stepStates[index];
            if (state.Status == CCS_PlaytestStepStatus.Passed
                || state.Status == CCS_PlaytestStepStatus.Skipped
                || state.Status == CCS_PlaytestStepStatus.Failed)
            {
                return;
            }

            if (failed)
            {
                state.SetStatus(CCS_PlaytestStepStatus.Failed);
                RaiseStepFailed(state, message);
            }
            else
            {
                state.SetStatus(CCS_PlaytestStepStatus.Passed);
                RaiseStepPassed(state, message);
            }

            AdvanceToNextStep();
        }

        private void AdvanceToNextStep()
        {
            int nextIndex = FindNextIncompleteStepIndex();
            activeStepIndex = nextIndex;
            if (nextIndex < 0)
            {
                LogDebug("All playtest steps completed or skipped.");
                return;
            }

            stepStates[nextIndex].SetStatus(CCS_PlaytestStepStatus.Active);
            RaiseStepChanged(stepStates[nextIndex], "Next step activated.");
        }

        private int FindNextIncompleteStepIndex()
        {
            for (int index = 0; index < stepStates.Count; index++)
            {
                CCS_PlaytestStepStatus status = stepStates[index].Status;
                if (status == CCS_PlaytestStepStatus.NotStarted)
                {
                    return index;
                }
            }

            return -1;
        }

        private int FindActiveStepIndexOfType(CCS_PlaytestStepType stepType)
        {
            if (activeStepIndex < 0 || activeStepIndex >= stepStates.Count)
            {
                return -1;
            }

            return stepStates[activeStepIndex].Definition.StepType == stepType ? activeStepIndex : -1;
        }

        private int FindStepIndex(string stepId)
        {
            for (int index = 0; index < stepStates.Count; index++)
            {
                if (stepStates[index].Definition.StepId == stepId)
                {
                    return index;
                }
            }

            return -1;
        }

        private void SetActiveStepIndex(int index, string message)
        {
            if (activeStepIndex >= 0 && activeStepIndex < stepStates.Count)
            {
                CCS_PlaytestStepStatus previousStatus = stepStates[activeStepIndex].Status;
                if (previousStatus == CCS_PlaytestStepStatus.Active)
                {
                    stepStates[activeStepIndex].SetStatus(CCS_PlaytestStepStatus.NotStarted);
                }
            }

            activeStepIndex = index;
            stepStates[index].SetStatus(CCS_PlaytestStepStatus.Active);
            RaiseStepChanged(stepStates[index], message);
        }

        private bool MatchesTargetItem(CCS_PlaytestStepType stepType, string itemId)
        {
            CCS_PlaytestStepState state = activeStepIndex >= 0 && activeStepIndex < stepStates.Count
                ? stepStates[activeStepIndex]
                : null;
            if (state == null || state.Definition.StepType != stepType)
            {
                return false;
            }

            string targetItemId = state.Definition.TargetItemId;
            if (string.IsNullOrWhiteSpace(targetItemId))
            {
                if (stepType == CCS_PlaytestStepType.GatherResource)
                {
                    return IsGatherResourceItem(itemId);
                }

                if (stepType == CCS_PlaytestStepType.EquipWeapon)
                {
                    return itemId == SpearItemId;
                }

                if (stepType == CCS_PlaytestStepType.EatFood)
                {
                    return IsCookedFoodItem(itemId);
                }

                return true;
            }

            return targetItemId == itemId;
        }

        private static bool IsGatherResourceItem(string itemId)
        {
            return itemId == StickItemId || itemId == WoodItemId;
        }

        private static bool IsCookedFoodItem(string itemId)
        {
            return itemId == CookedRabbitItemId || itemId == CookedVenisonItemId;
        }

        private void RaiseStepChanged(CCS_PlaytestStepState state, string message)
        {
            PlaytestStepChanged?.Invoke(CreateEventArgs(state, message));
        }

        private void RaiseStepPassed(CCS_PlaytestStepState state, string message)
        {
            PlaytestStepPassed?.Invoke(CreateEventArgs(state, message));
            LogDebug($"Passed: {state.Definition.DisplayName} — {message}");
        }

        private void RaiseStepFailed(CCS_PlaytestStepState state, string message)
        {
            PlaytestStepFailed?.Invoke(CreateEventArgs(state, message));
            LogDebug($"Failed: {state.Definition.DisplayName} — {message}");
        }

        private CCS_PlaytestEventArgs CreateEventArgs(CCS_PlaytestStepState state, string message)
        {
            return new CCS_PlaytestEventArgs(
                state.Definition.StepId,
                state.Definition.StepType,
                state.Status,
                message);
        }

        private void LogDebug(string message)
        {
            if (activeProfile != null && activeProfile.ShowDebugLogs)
            {
                Debug.Log($"{LogPrefix} {message}");
            }
        }

        #endregion
    }
}
