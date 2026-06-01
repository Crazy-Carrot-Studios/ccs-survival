using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Building;
using CCS.Modules.Crafting;
using CCS.Modules.Combat;
using CCS.Modules.Cooking;
using CCS.Modules.Equipment;
using CCS.Modules.CharacterController;
using CCS.Modules.Gathering;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.PlayerDeath;
using CCS.Modules.SaveSystem;
using CCS.Modules.Sleep;
using CCS.Modules.Storage;
using CCS.Modules.SurvivalCore;
using CCS.Modules.Wildlife;
using CCS.Survival;
using CCS.Survival.Player;
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
        private const string StoneItemId = "ccs.survival.item.resource.stone";
        private const string FiberItemId = "ccs.survival.item.resource.fiber";
        private const string SpearItemId = "ccs.survival.item.starter.spear";
        private const string StorageCrateRecipeId = "ccs.survival.recipe.progression.storagecrate";
        private const string CookedRabbitItemId = "ccs.survival.item.food.cookedrabbitmeat";
        private const string CookedVenisonItemId = "ccs.survival.item.food.cookedvenison";
        private const string FoundationPieceId = "ccs.survival.building.primitive.foundation";
        private const string LegacyFoundationPieceId = "ccs.survival.building.test.foundation";

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
        private CCS_CraftingRecipeService boundCraftingRecipeService;
        private CCS_StorageService boundStorageService;
        private CCS_SleepService boundSleepService;
        private CCS_PlayerEquipmentService boundEquipmentService;
        private bool playtestStorageCrateExists;
        private bool playtestStorageCrateOpened;
        private bool playtestStorageItemDeposited;
        private bool playtestStorageRestoredAfterLoad;
        private bool playtestBedrollExists;
        private bool playtestSleepCompleted;
        private bool playtestBedrollRespawnAssigned;
        private bool playtestBedrollRestoredAfterLoad;
        private bool controllerPolishWalkDone;
        private bool controllerPolishSprintDone;
        private bool controllerPolishCameraRotateDone;
        private bool controllerPolishInteractDone;
        private bool controllerPolishGatherDone;
        private bool controllerPolishBuildingPreviewDone;
        private bool controllerPolishStorageDone;
        private bool controllerPolishBedrollDone;
        private float controllerPolishPreviousYaw;
        private bool controllerPolishYawInitialized;
        private CCS_InteractionService boundInteractionService;

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
            CCS_CraftingRecipeService craftingRecipeService,
            CCS_StorageService storageService,
            CCS_SleepService sleepService,
            CCS_SurvivalCoreService survivalCore,
            CCS_InteractionService interactionService = null)
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
            boundCraftingRecipeService = craftingRecipeService;
            boundStorageService = storageService;
            boundSleepService = sleepService;
            boundEquipmentService = equipmentService;
            boundInteractionService = interactionService;

            if (boundInteractionService != null)
            {
                boundInteractionService.InteractionSucceeded += HandleInteractionSucceeded;
            }

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

            if (boundCraftingRecipeService != null)
            {
                boundCraftingRecipeService.CraftingCompleted += HandleCraftingProgressionCompleted;
            }

            if (boundEquipmentService != null)
            {
                boundEquipmentService.ItemEquipped += HandleItemEquipped;
            }

            CCS_EquipmentVisualRuntimeBridge.VisualSpawned += HandleEquipmentVisualSpawned;
            CCS_EquipmentVisualRuntimeBridge.VisualRemoved += HandleEquipmentVisualRemoved;

            if (boundStorageService != null)
            {
                boundStorageService.StorageContainerOpened += HandleStorageContainerOpened;
                boundStorageService.StorageItemAdded += HandleStorageItemAdded;
                boundStorageService.StorageStateRestored += HandleStorageStateRestored;
            }

            if (boundSleepService != null)
            {
                boundSleepService.SleepCompleted += HandleSleepCompleted;
                boundSleepService.SleepRespawnPointAssigned += HandleSleepRespawnPointAssigned;
                boundSleepService.SleepStateRestored += HandleSleepStateRestored;
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

            if (boundCraftingRecipeService != null)
            {
                boundCraftingRecipeService.CraftingCompleted -= HandleCraftingProgressionCompleted;
            }

            if (boundEquipmentService != null)
            {
                boundEquipmentService.ItemEquipped -= HandleItemEquipped;
            }

            CCS_EquipmentVisualRuntimeBridge.VisualSpawned -= HandleEquipmentVisualSpawned;
            CCS_EquipmentVisualRuntimeBridge.VisualRemoved -= HandleEquipmentVisualRemoved;

            if (boundStorageService != null)
            {
                boundStorageService.StorageContainerOpened -= HandleStorageContainerOpened;
                boundStorageService.StorageItemAdded -= HandleStorageItemAdded;
                boundStorageService.StorageStateRestored -= HandleStorageStateRestored;
            }

            if (boundSleepService != null)
            {
                boundSleepService.SleepCompleted -= HandleSleepCompleted;
                boundSleepService.SleepRespawnPointAssigned -= HandleSleepRespawnPointAssigned;
                boundSleepService.SleepStateRestored -= HandleSleepStateRestored;
            }

            boundGatheringService = null;
            boundCombatService = null;
            boundWildlifeHarvestService = null;
            boundCookingService = null;
            boundConsumableFoodService = null;
            boundSaveService = null;
            boundPlayerDeathService = null;
            boundBuildingPlacementService = null;
            boundCraftingRecipeService = null;
            boundStorageService = null;
            boundSleepService = null;
            boundEquipmentService = null;

            if (boundInteractionService != null)
            {
                boundInteractionService.InteractionSucceeded -= HandleInteractionSucceeded;
            }

            boundInteractionService = null;
            eventsBound = false;
        }

        public void ResetSteps()
        {
            ResetStoragePlaytestFlags();
            ResetBedrollPlaytestFlags();
            ResetControllerPolishFlags();

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

            if (boundPlayerDeathService != null && boundPlayerDeathService.IsInitialized)
            {
                boundPlayerDeathService.TriggerTestDeath("Playtest forced death (F7).");
            }

            LogDebug("Forced hunger and thirst to zero for death playtest (F7).");
        }

        public bool TryEquipStarterSpear()
        {
            if (!harnessEnabled || boundEquipmentService == null || !boundEquipmentService.IsInitialized)
            {
                return false;
            }

            CCS_EquippedItem equippedMainHand =
                boundEquipmentService.GetEquippedItem(CCS_EquipmentSlotType.MainHand);
            if (equippedMainHand?.ItemDefinition?.ItemId == SpearItemId)
            {
                int visualStepIndex = FindActiveStepIndexOfType(CCS_PlaytestStepType.ConfirmEquipmentVisual);
                if (visualStepIndex >= 0 && stepStates[visualStepIndex].ProgressCount >= 1)
                {
                    boundEquipmentService.UnequipItem(CCS_EquipmentSlotType.MainHand);
                    LogDebug("Unequipped starter spear for equipment visual playtest (F6).");
                    return true;
                }

                TryCompleteActiveStepOfType(CCS_PlaytestStepType.EquipWeapon, "Spear already equipped.");
                return true;
            }

            CCS_EquipmentItemDefinition spearDefinition = FindEquipmentDefinitionForItemId(SpearItemId);
            if (spearDefinition == null)
            {
                LogDebug("Starter spear equipment definition was not found in the active equipment profile.");
                return false;
            }

            if (boundEquipmentService.IsSlotOccupied(CCS_EquipmentSlotType.MainHand))
            {
                boundEquipmentService.UnequipItem(CCS_EquipmentSlotType.MainHand);
            }

            if (!boundEquipmentService.EquipItem(spearDefinition))
            {
                LogDebug("Failed to equip starter spear for playtest.");
                return false;
            }

            LogDebug("Equipped starter spear for playtest (F6).");
            return true;
        }

        public bool TryPlacePlaytestFoundation()
        {
            if (!harnessEnabled)
            {
                return false;
            }

            if (!CCS_BuildingRuntimeBridge.TryGetBuildingPlacementService(out CCS_BuildingPlacementService placementService)
                || placementService == null
                || !placementService.IsInitialized)
            {
                return false;
            }

            if (!CCS_BuildingRuntimeBridge.TryGetBuildingService(out CCS_BuildingService buildingService)
                || buildingService == null
                || !buildingService.IsInitialized)
            {
                return false;
            }

            if (!buildingService.TryGetDefinition(FoundationPieceId, out CCS_BuildingPieceDefinition definition))
            {
                return false;
            }

            if (!TrySeedBuildingPlacementResources(definition))
            {
                LogDebug("Could not seed inventory resources for playtest foundation placement.");
                return false;
            }

            if (!placementService.SetActiveDefinition(definition))
            {
                return false;
            }

            Vector3 placementPosition = ResolvePlaytestFoundationPosition();
            if (!placementService.UpdatePreviewWithSnap(placementPosition, Quaternion.identity))
            {
                LogDebug($"Foundation preview invalid at {placementPosition}.");
                return false;
            }

            CCS_BuildingPlacementValidationResult result = placementService.PlaceCurrentPieceUsingSnap();
            if (!result.Success)
            {
                LogDebug($"Foundation placement failed: {result.FailureReason}");
                return false;
            }

            LogDebug("Placed playtest foundation (B).");
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!harnessEnabled || activeStepIndex < 0 || activeStepIndex >= stepStates.Count)
            {
                return;
            }

            CCS_PlaytestStepState activeState = stepStates[activeStepIndex];
            activeState.TickActive(deltaTime);
            if (activeState.Definition.StepType == CCS_PlaytestStepType.VerifyControllerPolish)
            {
                UpdateControllerPolishTracking(deltaTime);
            }

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

        private void HandleInteractionSucceeded(CCS_InteractionEventArgs eventArgs)
        {
            controllerPolishInteractDone = true;
            TryEvaluateControllerPolishCompletion();
        }

        private void HandleGatheringNodeGathered(CCS_GatheringEventArgs eventArgs)
        {
            controllerPolishGatherDone = true;
            TryEvaluateControllerPolishCompletion();

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
                EvaluateStorageCrateStepAfterLoad();
                EvaluateBedrollStepAfterLoad();
            }
        }

        private void HandleStorageContainerOpened(CCS_StorageEventArgs eventArgs)
        {
            if (eventArgs == null || !eventArgs.IsSuccess)
            {
                return;
            }

            playtestStorageCrateExists = true;
            playtestStorageCrateOpened = true;
            controllerPolishStorageDone = true;
            TryEvaluateControllerPolishCompletion();
            TryEvaluateStorageCrateStepCompletion();
        }

        private void HandleStorageItemAdded(CCS_StorageEventArgs eventArgs)
        {
            if (eventArgs == null || !eventArgs.IsSuccess)
            {
                return;
            }

            playtestStorageCrateExists = true;
            playtestStorageItemDeposited = true;
            TryEvaluateStorageCrateStepCompletion();
        }

        private void HandleStorageStateRestored(CCS_StorageEventArgs eventArgs)
        {
            if (eventArgs == null || !eventArgs.IsSuccess)
            {
                return;
            }

            playtestStorageCrateExists = boundStorageService != null && boundStorageService.RegisteredContainerCount > 0;
            playtestStorageRestoredAfterLoad = true;
            TryEvaluateStorageCrateStepCompletion();
        }

        private void HandleSleepCompleted(CCS_SleepEventArgs eventArgs)
        {
            if (eventArgs == null || !eventArgs.Success)
            {
                return;
            }

            playtestBedrollExists = true;
            playtestSleepCompleted = true;
            controllerPolishBedrollDone = true;
            TryEvaluateControllerPolishCompletion();
            TryEvaluateBedrollStepCompletion();
        }

        private void HandleSleepRespawnPointAssigned(CCS_SleepEventArgs eventArgs)
        {
            if (eventArgs == null || !eventArgs.Success)
            {
                return;
            }

            playtestBedrollRespawnAssigned = true;
            TryEvaluateBedrollStepCompletion();
        }

        private void HandleSleepStateRestored(CCS_SleepEventArgs eventArgs)
        {
            if (eventArgs == null || !eventArgs.Success)
            {
                return;
            }

            playtestBedrollExists = boundSleepService != null && boundSleepService.RegisteredSleepSpotCount > 0;
            playtestBedrollRestoredAfterLoad = true;
            TryEvaluateBedrollStepCompletion();
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
            if (MatchesTargetBuildingPiece(eventArgs))
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.PlaceBuilding, "Building piece placed.");
            }

            TryCompleteShelterStepIfReady();
        }

        private void TryCompleteShelterStepIfReady()
        {
            if (!CCS_BuildingRuntimeBridge.TryGetBuildingRecipeService(out CCS_BuildingRecipeService recipeService)
                || recipeService == null
                || !recipeService.IsInitialized)
            {
                return;
            }

            if (!recipeService.MeetsMinimumShelter())
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.BuildShelter,
                "Minimum shelter reached (foundation, wall, roof).");
        }

        private void HandleCraftingProgressionCompleted(CCS_CraftingProgressionEventArgs eventArgs)
        {
            if (eventArgs == null || boundCraftingRecipeService == null)
            {
                return;
            }

            if (eventArgs.StationType != CCS_CraftingStationType.Workbench)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.CraftAtWorkbench,
                "Workbench crafting completed.");
        }

        public bool TrySeedWorkbenchCraftingResources()
        {
            if (!CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            bool seeded = false;
            seeded |= TryAddInventoryItemById(inventoryService, WoodItemId, 12);
            seeded |= TryAddInventoryItemById(inventoryService, StickItemId, 6);
            seeded |= TryAddInventoryItemById(inventoryService, StoneItemId, 6);
            seeded |= TryAddInventoryItemById(inventoryService, FiberItemId, 8);
            seeded |= TryAddInventoryItemById(inventoryService, SpearItemId, 1);

            if (seeded)
            {
                LogDebug("Seeded workbench crafting resources (F4).");
            }

            return seeded;
        }

        public bool TryCraftWorkbenchPlaytestItem()
        {
            if (!harnessEnabled)
            {
                return false;
            }

            CCS_CraftingRecipeService recipeService = boundCraftingRecipeService;
            if (recipeService == null || !recipeService.IsInitialized)
            {
                if (!CCS_CraftingRuntimeBridge.TryGetCraftingRecipeService(out recipeService)
                    || recipeService == null
                    || !recipeService.IsInitialized)
                {
                    return false;
                }
            }

            if (!recipeService.TryGetRecipeById(StorageCrateRecipeId, out CCS_CraftingRecipeDefinition recipe))
            {
                LogDebug("Storage crate progression recipe was not found.");
                return false;
            }

            CCS_CraftingStationContext workbenchContext = new CCS_CraftingStationContext(
                CCS_CraftingStationType.Workbench,
                "Test Workbench",
                "ccs.survival.station.test.workbench");

            recipeService.ApplyActiveStationContext(workbenchContext);
            CCS_CraftingResult result = recipeService.TryCraftProgressionRecipe(recipe, workbenchContext, 1);
            if (result.IsSuccess)
            {
                LogDebug("Crafted storage crate at workbench for playtest (F3).");
            }
            else
            {
                LogDebug($"Workbench craft failed: {result.Message}");
            }

            return result.IsSuccess;
        }

        public bool TryPlaceOrOpenStorageCrateNearPlayer()
        {
            if (!harnessEnabled)
            {
                return false;
            }

            CCS_StorageService storageService = ResolveStorageService();
            if (storageService == null)
            {
                return false;
            }

            if (storageService.ActiveContainer != null)
            {
                storageService.CloseContainer();
                return true;
            }

            if (storageService.RegisteredContainerCount > 0 && storageService.TryOpenNearestContainer())
            {
                playtestStorageCrateExists = true;
                return true;
            }

            CCS_StorageContainer placedContainer = storageService.TryPlaceDefaultContainerNearPlayer();
            if (placedContainer != null)
            {
                playtestStorageCrateExists = true;
                LogDebug("Placed primitive storage crate near player (F2).");
                return true;
            }

            return false;
        }

        public bool TryPlaceOrSleepBedrollNearPlayer()
        {
            if (!harnessEnabled)
            {
                return false;
            }

            CCS_SleepService sleepService = ResolveSleepService();
            if (sleepService == null)
            {
                return false;
            }

            if (sleepService.RegisteredSleepSpotCount > 0 && sleepService.TrySleepAtNearestSpot())
            {
                playtestBedrollExists = true;
                LogDebug("Sleep attempted at nearest bedroll (Shift+F2).");
                return true;
            }

            CCS_SleepSpot placedSpot = sleepService.TryPlaceDefaultSleepSpotNearPlayer();
            if (placedSpot != null)
            {
                playtestBedrollExists = true;
                LogDebug("Placed primitive bedroll near player (Shift+F2).");
                return true;
            }

            return false;
        }

        public bool TryMoveFirstPlayerItemToActiveStorageCrate()
        {
            if (!harnessEnabled)
            {
                return false;
            }

            CCS_StorageService storageService = ResolveStorageService();
            if (storageService == null)
            {
                return false;
            }

            if (storageService.ActiveContainer == null && !storageService.TryOpenNearestContainer())
            {
                LogDebug("No active storage crate for F1 transfer.");
                return false;
            }

            bool moved = storageService.TryMovePlayerItemToContainer();
            if (moved)
            {
                LogDebug("Moved first player item into active storage crate (F1).");
            }

            return moved;
        }

        public bool TryMoveFirstStorageItemToPlayer()
        {
            if (!harnessEnabled)
            {
                return false;
            }

            CCS_StorageService storageService = ResolveStorageService();
            if (storageService == null)
            {
                return false;
            }

            if (storageService.ActiveContainer == null && !storageService.TryOpenNearestContainer())
            {
                LogDebug("No active storage crate for Shift+F1 transfer.");
                return false;
            }

            bool moved = storageService.TryMoveContainerItemToPlayer();
            if (moved)
            {
                LogDebug("Moved first storage crate item to player (Shift+F1).");
            }

            return moved;
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

        private void HandleEquipmentVisualSpawned(string itemId)
        {
            IncrementActiveStepOfType(CCS_PlaytestStepType.ConfirmEquipmentVisual, itemId);
        }

        private void HandleEquipmentVisualRemoved(string itemId)
        {
            IncrementActiveStepOfType(CCS_PlaytestStepType.ConfirmEquipmentVisual, itemId);
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

            if (stepType == CCS_PlaytestStepType.GatherResource && IsGatherResourceItem(targetItemId))
            {
                return IsGatherResourceItem(itemId);
            }

            if (stepType == CCS_PlaytestStepType.EatFood && IsCookedFoodItem(targetItemId))
            {
                return IsCookedFoodItem(itemId);
            }

            return targetItemId == itemId;
        }

        private bool MatchesTargetBuildingPiece(CCS_BuildingPlacementEventArgs eventArgs)
        {
            CCS_PlaytestStepState state = activeStepIndex >= 0 && activeStepIndex < stepStates.Count
                ? stepStates[activeStepIndex]
                : null;
            if (state == null || state.Definition.StepType != CCS_PlaytestStepType.PlaceBuilding)
            {
                return false;
            }

            string targetObjectId = state.Definition.TargetObjectId;
            if (string.IsNullOrWhiteSpace(targetObjectId))
            {
                return true;
            }

            string placedPieceId = eventArgs?.PlacedInstance != null ? eventArgs.PlacedInstance.PieceId : string.Empty;
            return placedPieceId == targetObjectId
                || (targetObjectId == LegacyFoundationPieceId && placedPieceId == FoundationPieceId);
        }

        private CCS_EquipmentItemDefinition FindEquipmentDefinitionForItemId(string itemId)
        {
            if (boundEquipmentService?.ActiveProfile == null
                || string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            CCS_EquipmentItemDefinition[] definitions =
                boundEquipmentService.ActiveProfile.SaveRestoreEquipmentDefinitions;
            if (definitions == null)
            {
                return null;
            }

            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_EquipmentItemDefinition definition = definitions[index];
                if (definition?.ItemDefinition != null && definition.ItemDefinition.ItemId == itemId)
                {
                    return definition;
                }
            }

            return null;
        }

        private void EvaluateStorageCrateStepAfterLoad()
        {
            if (boundStorageService != null && boundStorageService.RegisteredContainerCount > 0)
            {
                playtestStorageCrateExists = true;
            }

            for (int index = 0; index < stepStates.Count; index++)
            {
                if (stepStates[index].Definition.StepType != CCS_PlaytestStepType.UseStorageCrate)
                {
                    continue;
                }

                if (stepStates[index].Status == CCS_PlaytestStepStatus.Passed)
                {
                    playtestStorageRestoredAfterLoad = true;
                }

                break;
            }

            TryEvaluateStorageCrateStepCompletion();
        }

        private void TryEvaluateStorageCrateStepCompletion()
        {
            if (!playtestStorageCrateExists
                || !playtestStorageCrateOpened
                || !playtestStorageItemDeposited
                || !playtestStorageRestoredAfterLoad)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.UseStorageCrate,
                "Storage crate placed, opened, item deposited, and restored after save/load.");
        }

        private void ResetStoragePlaytestFlags()
        {
            playtestStorageCrateExists = false;
            playtestStorageCrateOpened = false;
            playtestStorageItemDeposited = false;
            playtestStorageRestoredAfterLoad = false;
        }

        private void ResetControllerPolishFlags()
        {
            controllerPolishWalkDone = false;
            controllerPolishSprintDone = false;
            controllerPolishCameraRotateDone = false;
            controllerPolishInteractDone = false;
            controllerPolishGatherDone = false;
            controllerPolishBuildingPreviewDone = false;
            controllerPolishStorageDone = false;
            controllerPolishBedrollDone = false;
            controllerPolishPreviousYaw = 0f;
            controllerPolishYawInitialized = false;
        }

        private void UpdateControllerPolishTracking(float deltaTime)
        {
            if (!CCS_CharacterMovementRuntimeBridge.TryGetCharacterMovementService(
                    out CCS_CharacterMovementService movementService)
                || movementService == null
                || !movementService.IsInitialized)
            {
                return;
            }

            CCS_CharacterMovementSnapshot snapshot = movementService.CurrentSnapshot;
            if (snapshot.MovementState == CCS_CharacterMovementState.Walking
                || snapshot.MovementState == CCS_CharacterMovementState.Running)
            {
                controllerPolishWalkDone = true;
            }

            if (snapshot.IsSprinting)
            {
                controllerPolishSprintDone = true;
            }

            float yaw = movementService.LookState.YawDegrees;
            if (!controllerPolishYawInitialized)
            {
                controllerPolishPreviousYaw = yaw;
                controllerPolishYawInitialized = true;
            }
            else if (Mathf.Abs(Mathf.DeltaAngle(controllerPolishPreviousYaw, yaw)) > 2f
                || movementService.LastLookInputMagnitude > 0.05f)
            {
                controllerPolishCameraRotateDone = true;
            }

            controllerPolishPreviousYaw = yaw;

            if (boundBuildingPlacementService != null
                && boundBuildingPlacementService.IsPlacementModeActive)
            {
                controllerPolishBuildingPreviewDone = true;
            }

            TryEvaluateControllerPolishCompletion();
        }

        private void TryEvaluateControllerPolishCompletion()
        {
            if (!controllerPolishWalkDone
                || !controllerPolishSprintDone
                || !controllerPolishCameraRotateDone
                || !controllerPolishInteractDone
                || !controllerPolishGatherDone
                || !controllerPolishBuildingPreviewDone
                || !controllerPolishStorageDone
                || !controllerPolishBedrollDone)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.VerifyControllerPolish,
                "Third-person controller polish checklist completed.");
        }

        private void EvaluateBedrollStepAfterLoad()
        {
            if (boundSleepService != null && boundSleepService.RegisteredSleepSpotCount > 0)
            {
                playtestBedrollExists = true;
            }

            for (int index = 0; index < stepStates.Count; index++)
            {
                if (stepStates[index].Definition.StepType != CCS_PlaytestStepType.PlaceAndSleepAtBedroll)
                {
                    continue;
                }

                if (stepStates[index].Status == CCS_PlaytestStepStatus.Passed)
                {
                    playtestBedrollRestoredAfterLoad = true;
                }

                break;
            }

            TryEvaluateBedrollStepCompletion();
        }

        private void TryEvaluateBedrollStepCompletion()
        {
            if (!playtestBedrollExists
                || !playtestSleepCompleted
                || !playtestBedrollRespawnAssigned
                || !playtestBedrollRestoredAfterLoad)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.PlaceAndSleepAtBedroll,
                "Bedroll placed, sleep completed, respawn assigned, and restored after save/load.");
        }

        private void ResetBedrollPlaytestFlags()
        {
            playtestBedrollExists = false;
            playtestSleepCompleted = false;
            playtestBedrollRespawnAssigned = false;
            playtestBedrollRestoredAfterLoad = false;
        }

        private CCS_StorageService ResolveStorageService()
        {
            if (boundStorageService != null && boundStorageService.IsInitialized)
            {
                return boundStorageService;
            }

            if (CCS_StorageRuntimeBridge.TryGetStorageService(out CCS_StorageService storageService)
                && storageService != null
                && storageService.IsInitialized)
            {
                return storageService;
            }

            return null;
        }

        private CCS_SleepService ResolveSleepService()
        {
            if (boundSleepService != null && boundSleepService.IsInitialized)
            {
                return boundSleepService;
            }

            if (CCS_SleepRuntimeBridge.TryGetSleepService(out CCS_SleepService sleepService)
                && sleepService != null
                && sleepService.IsInitialized)
            {
                return sleepService;
            }

            return null;
        }

        private static bool TryAddInventoryItemById(
            CCS_PlayerInventoryService inventoryService,
            string itemId,
            int quantity)
        {
            if (inventoryService == null
                || !inventoryService.IsInitialized
                || string.IsNullOrWhiteSpace(itemId)
                || quantity <= 0)
            {
                return false;
            }

            CCS_ItemDefinition[] definitions = inventoryService.ActiveProfile?.SaveRestoreItemDefinitions;
            if (definitions == null)
            {
                return false;
            }

            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_ItemDefinition definition = definitions[index];
                if (definition != null && definition.ItemId == itemId)
                {
                    return inventoryService.AddItem(definition, quantity) > 0;
                }
            }

            return false;
        }

        private bool TrySeedBuildingPlacementResources(CCS_BuildingPieceDefinition definition)
        {
            if (!CCS_PlaytestRuntimeBridge.TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost.ServiceRegistry == null
                || !runtimeHost.ServiceRegistry.TryGetService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            if (definition?.BuildCostEntries != null)
            {
                for (int index = 0; index < definition.BuildCostEntries.Count; index++)
                {
                    CCS_BuildingCostEntry costEntry = definition.BuildCostEntries[index];
                    if (costEntry.ItemDefinition == null || costEntry.Quantity <= 0)
                    {
                        continue;
                    }

                    inventoryService.AddItem(costEntry.ItemDefinition, costEntry.Quantity * 5);
                }
            }

            return true;
        }

        private static Vector3 ResolvePlaytestFoundationPosition()
        {
            CCS_PlayerGameplayController[] players =
                Object.FindObjectsByType<CCS_PlayerGameplayController>(FindObjectsSortMode.None);
            if (players != null && players.Length > 0 && players[0] != null)
            {
                return players[0].transform.position + players[0].transform.forward * 3f + Vector3.up * 0.5f;
            }

            return new Vector3(0f, 0.5f, 3f);
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
