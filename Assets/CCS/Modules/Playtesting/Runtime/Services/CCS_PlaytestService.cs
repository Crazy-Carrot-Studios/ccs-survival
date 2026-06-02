using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Building;
using CCS.Modules.Crafting;
using CCS.Modules.Economy;
using CCS.Modules.Combat;
using CCS.Modules.Cooking;
using CCS.Modules.Equipment;
using CCS.Modules.CharacterController;
using CCS.Modules.Gathering;
using CCS.Modules.Hotbar;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.PlayerDeath;
using CCS.Modules.SaveSystem;
using CCS.Modules.Sleep;
using CCS.Modules.Storage;
using CCS.Modules.SurvivalCore;
using CCS.Modules.Trapping;
using CCS.Modules.Industry;
using CCS.Modules.Mounts;
using CCS.Modules.Vehicles;
using CCS.Modules.Firearms;
using CCS.Modules.Prospecting;
using CCS.Modules.Shelter;
using CCS.Modules.Settlements;
using CCS.Modules.Regions;
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
        private const string SaplingItemId = "ccs.survival.item.resource.sapling";
        private const string ScrapIronItemId = "ccs.survival.item.resource.scrapiron";
        private const string PocketKnifeItemId = "ccs.survival.item.starter.knife";
        private const string SpearItemId = "ccs.survival.item.starter.spear";
        private const string FrontierFishingPoleRecipeId = "ccs.survival.recipe.frontier.fishingpole";
        private const string FrontierBowRecipeId = "ccs.survival.recipe.frontier.bow";
        private const string BoneHatchetItemId = "ccs.survival.item.tool.hatchet.bone";
        private const string BonePickItemId = "ccs.survival.item.tool.pick.bone";
        private const string FishingPoleItemId = "ccs.survival.item.tool.fishingpole";
        private const string BowItemId = "ccs.survival.item.frontier.bow";
        private const string SimpleTrapItemId = "ccs.survival.item.frontier.simpletrap";
        private const string FrontierSimpleTrapRecipeId = "ccs.survival.recipe.frontier.simpletrap";
        private const string HideItemId = "ccs.survival.item.resource.hide";
        private const string RawFishItemId = "ccs.survival.item.resource.rawfish";
        private const string CordageItemId = "ccs.survival.item.frontier.cordage";
        private const string TradeDollarsCurrencyId = "ccs.survival.currency.tradedollars";
        private const string GeneralStoreVendorId = "ccs.survival.vendor.frontier.generalstore";
        private const string FrontierStableVendorId = CCS_MountContentIds.FrontierStableVendorId;
        private const string FrontierHorseItemId = CCS_MountContentIds.FrontierHorseItemId;
        private const string FrontierWagonItemId = CCS_VehicleContentIds.FrontierWagonItemId;
        private const string FrontierGunsmithVendorId = CCS_FirearmContentIds.FrontierGunsmithVendorId;
        private const string FrontierRevolverItemId = CCS_FirearmContentIds.FrontierRevolverItemId;
        private const string RevolverCartridgeItemId = CCS_FirearmContentIds.RevolverCartridgeItemId;
        private const string StorageCrateRecipeId = "ccs.survival.recipe.progression.storagecrate";
        private const string CookedRabbitItemId = "ccs.survival.item.food.cookedrabbitmeat";
        private const string CookedVenisonItemId = "ccs.survival.item.food.cookedvenison";
        private const string RawMeatItemId = "ccs.survival.item.resource.rawmeat";
        private const string CookedFishItemId = "ccs.survival.item.food.cookedfish";
        private const string CookedMeatItemId = "ccs.survival.item.food.cookedmeat";
        private const string CookedTurkeyItemId = "ccs.survival.item.food.cookedturkey";
        private const string JerkyItemId = "ccs.survival.item.food.jerky";
        private const string DriedFishItemId = "ccs.survival.item.food.driedfish";
        private const string LeanToKitItemId = "ccs.survival.item.frontier.leantokit";
        private const string SupplyCrateKitItemId = "ccs.survival.item.frontier.supplycratekit";
        private const string WorkbenchKitItemId = "ccs.survival.item.frontier.workbenchkit";
        private const string SawTableKitItemId = "ccs.survival.item.frontier.sawtablekit";
        private const string CharcoalKilnKitItemId = "ccs.survival.item.frontier.charcoalkilnkit";
        private const string PrimitiveForgeKitItemId = "ccs.survival.item.frontier.primitiveforgekit";
        private const string IronOreItemId = "ccs.survival.item.resource.ironore";
        private const string IronHatchetItemId = "ccs.survival.item.tool.hatchet.iron";
        private const string IndustryProcessWoodLumberId = "ccs.survival.industry.process.wood.lumber";
        private const string IndustryProcessWoodCharcoalId = "ccs.survival.industry.process.wood.charcoal";
        private const string IndustryProcessIronRefineId = "ccs.survival.industry.process.ironore.refinediron";
        private const string BlacksmithIronHatchetHeadId = "ccs.survival.industry.blacksmith.ironhatchethead";
        private const string CampfirePieceId = "ccs.survival.building.campfire.test";
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
        private CCS_TrapService boundTrapService;
        private CCS_CookingService boundCookingService;
        private CCS_ConsumableFoodService boundConsumableFoodService;
        private CCS_SaveService boundSaveService;
        private CCS_PlayerDeathService boundPlayerDeathService;
        private CCS_BuildingPlacementService boundBuildingPlacementService;
        private CCS_CraftingRecipeService boundCraftingRecipeService;
        private CCS_CurrencyService boundCurrencyService;
        private CCS_VendorService boundVendorService;
        private int playtestCurrencyBaseline;
        private CCS_StorageService boundStorageService;
        private CCS_SleepService boundSleepService;
        private CCS_PlayerEquipmentService boundEquipmentService;
        private CCS_ActiveItemService boundActiveItemService;
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
        private CCS_CampService boundCampService;
        private CCS_MountService boundMountService;
        private CCS_VehicleService boundVehicleService;
        private CCS_FirearmService boundFirearmService;
        private CCS_SettlementService boundSettlementService;
        private CCS_RegionService boundRegionService;

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
            CCS_TrapService trapService,
            CCS_CookingService cookingService,
            CCS_ConsumableFoodService consumableFoodService,
            CCS_SaveService saveService,
            CCS_PlayerDeathService playerDeathService,
            CCS_BuildingPlacementService buildingPlacementService,
            CCS_PlayerEquipmentService equipmentService,
            CCS_ActiveItemService activeItemService,
            CCS_CraftingRecipeService craftingRecipeService,
            CCS_StorageService storageService,
            CCS_SleepService sleepService,
            CCS_SurvivalCoreService survivalCore,
            CCS_InteractionService interactionService = null,
            CCS_CurrencyService currencyService = null,
            CCS_VendorService vendorService = null,
            CCS_CampService campService = null,
            CCS_MountService mountService = null,
            CCS_VehicleService vehicleService = null,
            CCS_FirearmService firearmService = null,
            CCS_SettlementService settlementService = null,
            CCS_RegionService regionService = null)
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
            boundTrapService = trapService;
            boundCookingService = cookingService;
            boundConsumableFoodService = consumableFoodService;
            boundSaveService = saveService;
            boundPlayerDeathService = playerDeathService;
            boundBuildingPlacementService = buildingPlacementService;
            boundCraftingRecipeService = craftingRecipeService;
            boundStorageService = storageService;
            boundSleepService = sleepService;
            boundEquipmentService = equipmentService;
            boundActiveItemService = activeItemService;
            boundInteractionService = interactionService;
            boundCurrencyService = currencyService;
            boundVendorService = vendorService;
            boundCampService = campService;
            boundMountService = mountService;
            boundVehicleService = vehicleService;
            boundFirearmService = firearmService;
            boundSettlementService = settlementService;
            boundRegionService = regionService;

            if (boundSettlementService != null)
            {
                boundSettlementService.SettlementDiscovered += HandleSettlementDiscovered;
                boundSettlementService.ServicePointActivated += HandleSettlementServicePointActivated;
            }

            if (boundRegionService != null)
            {
                boundRegionService.RegionDiscovered += HandleRegionDiscovered;
            }

            if (boundMountService != null)
            {
                boundMountService.MountStateChanged += HandleMountStateChanged;
                boundMountService.HorseOwnershipChanged += HandleHorseOwnershipChanged;
            }

            if (boundVehicleService != null)
            {
                boundVehicleService.VehicleStateChanged += HandleVehicleStateChanged;
                boundVehicleService.WagonOwnershipChanged += HandleWagonOwnershipChanged;
            }

            if (boundVendorService != null)
            {
                boundVendorService.VendorTransactionCompleted += HandleVendorTransactionCompleted;
            }

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

            if (boundTrapService != null)
            {
                boundTrapService.TrapPlaced += HandleTrapPlaced;
                boundTrapService.TrapTriggered += HandleTrapTriggered;
                boundTrapService.TrapHarvested += HandleTrapHarvested;
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

            if (boundActiveItemService != null)
            {
                boundActiveItemService.ActiveItemChanged += HandleActiveItemChanged;
                boundActiveItemService.ActiveItemUsed += HandleActiveItemUsed;
            }

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

            if (boundTrapService != null)
            {
                boundTrapService.TrapPlaced -= HandleTrapPlaced;
                boundTrapService.TrapTriggered -= HandleTrapTriggered;
                boundTrapService.TrapHarvested -= HandleTrapHarvested;
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

            if (boundActiveItemService != null)
            {
                boundActiveItemService.ActiveItemChanged -= HandleActiveItemChanged;
                boundActiveItemService.ActiveItemUsed -= HandleActiveItemUsed;
            }

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

            if (boundVendorService != null)
            {
                boundVendorService.VendorTransactionCompleted -= HandleVendorTransactionCompleted;
            }

            if (boundSettlementService != null)
            {
                boundSettlementService.SettlementDiscovered -= HandleSettlementDiscovered;
                boundSettlementService.ServicePointActivated -= HandleSettlementServicePointActivated;
            }

            if (boundRegionService != null)
            {
                boundRegionService.RegionDiscovered -= HandleRegionDiscovered;
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
            boundActiveItemService = null;
            boundCurrencyService = null;
            boundVendorService = null;
            if (boundMountService != null)
            {
                boundMountService.MountStateChanged -= HandleMountStateChanged;
                boundMountService.HorseOwnershipChanged -= HandleHorseOwnershipChanged;
            }

            if (boundVehicleService != null)
            {
                boundVehicleService.VehicleStateChanged -= HandleVehicleStateChanged;
                boundVehicleService.WagonOwnershipChanged -= HandleWagonOwnershipChanged;
            }

            boundMountService = null;
            boundVehicleService = null;
            boundFirearmService = null;
            boundSettlementService = null;
            boundRegionService = null;

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

        public bool TryEquipPocketKnife()
        {
            return TryEquipItemForPlaytest(PocketKnifeItemId, "pocket knife", CCS_PlaytestStepType.EquipWeapon);
        }

        public bool TryEquipStarterSpear()
        {
            return TryEquipItemForPlaytest(SpearItemId, "starter spear", CCS_PlaytestStepType.EquipSpearRegression);
        }

        private bool TryEquipItemForPlaytest(string itemId, string displayLabel, CCS_PlaytestStepType equipStepType)
        {
            if (!harnessEnabled || boundEquipmentService == null || !boundEquipmentService.IsInitialized)
            {
                return false;
            }

            CCS_EquippedItem equippedMainHand =
                boundEquipmentService.GetEquippedItem(CCS_EquipmentSlotType.MainHand);
            if (equippedMainHand?.ItemDefinition?.ItemId == itemId)
            {
                int visualStepIndex = FindActiveStepIndexOfType(CCS_PlaytestStepType.ConfirmEquipmentVisual);
                if (visualStepIndex >= 0 && stepStates[visualStepIndex].ProgressCount >= 1)
                {
                    boundEquipmentService.UnequipItem(CCS_EquipmentSlotType.MainHand);
                    LogDebug($"Unequipped {displayLabel} for equipment visual playtest (F6).");
                    return true;
                }

                TryCompleteActiveStepOfType(equipStepType, $"{displayLabel} already equipped.");
                return true;
            }

            CCS_EquipmentItemDefinition equipmentDefinition = FindEquipmentDefinitionForItemId(itemId);
            if (equipmentDefinition == null)
            {
                LogDebug($"{displayLabel} equipment definition was not found in the active equipment profile.");
                return false;
            }

            if (boundEquipmentService.IsSlotOccupied(CCS_EquipmentSlotType.MainHand))
            {
                boundEquipmentService.UnequipItem(CCS_EquipmentSlotType.MainHand);
            }

            if (!boundEquipmentService.EquipItem(equipmentDefinition))
            {
                LogDebug($"Failed to equip {displayLabel} for playtest.");
                return false;
            }

            LogDebug($"Equipped {displayLabel} for playtest (F6).");
            return true;
        }

        public bool TrySelectActiveFromMainHand()
        {
            if (!harnessEnabled || boundActiveItemService == null || !boundActiveItemService.IsInitialized)
            {
                return false;
            }

            if (!boundActiveItemService.SelectActiveFromEquipped(CCS_EquipmentSlotType.MainHand))
            {
                LogDebug("Failed to select active item from main hand.");
                return false;
            }

            LogDebug("Selected active item from main hand (Alpha1).");
            return true;
        }

        public bool TrySelectActiveFromToolSlot()
        {
            if (!harnessEnabled || boundActiveItemService == null || !boundActiveItemService.IsInitialized)
            {
                return false;
            }

            if (!boundActiveItemService.SelectActiveFromEquipped(CCS_EquipmentSlotType.Tool))
            {
                LogDebug("Failed to select active item from tool slot.");
                return false;
            }

            LogDebug("Selected active item from tool slot (Alpha2).");
            return true;
        }

        public bool TryEquipBoneHatchet()
        {
            return TryEquipToolForPlaytest(BoneHatchetItemId, "bone hatchet");
        }

        public bool TryEquipBonePick()
        {
            return TryEquipToolForPlaytest(BonePickItemId, "bone pick");
        }

        public bool TryEquipFishingPole()
        {
            return TryEquipToolForPlaytest(FishingPoleItemId, "fishing pole");
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

            TryEvaluateShelterCampPlaytestSteps();
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

            if (eventArgs != null)
            {
                switch (eventArgs.NodeType)
                {
                    case CCS_GatheringNodeType.StoneOutcrop:
                        TryCompleteActiveStepOfType(
                            CCS_PlaytestStepType.MineStoneOutcrop,
                            "Stone outcrop mined.");
                        break;
                    case CCS_GatheringNodeType.OreVein:
                        TryCompleteActiveStepOfType(
                            CCS_PlaytestStepType.MineIronVein,
                            "Iron vein mined.");
                        break;
                    case CCS_GatheringNodeType.CoalVein:
                        TryCompleteActiveStepOfType(
                            CCS_PlaytestStepType.MineCoalVein,
                            "Coal vein mined.");
                        break;
                }
            }

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
                }

                if (string.Equals(reward.itemDefinitionId, WoodItemId, System.StringComparison.OrdinalIgnoreCase))
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.GatherWoodForShelter,
                        "Wood gathered for frontier shelter.");
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
            TryEvaluateHuntingPlaytestSteps();
        }

        private void HandleTrapPlaced(CCS_TrapEventArgs eventArgs)
        {
            if (eventArgs != null && eventArgs.IsSuccess)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.PlaceTrapForTrapping, eventArgs.Message);
            }
        }

        private void HandleTrapTriggered(CCS_TrapEventArgs eventArgs)
        {
            if (eventArgs != null && eventArgs.IsSuccess)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.ForceTrapTrigger, eventArgs.Message);
            }
        }

        private void HandleTrapHarvested(CCS_TrapEventArgs eventArgs)
        {
            if (eventArgs != null && eventArgs.IsSuccess)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.HarvestTriggeredTrap, eventArgs.Message);
                TryEvaluateTrappingPlaytestSteps();
            }
        }

        private void HandleCookingCompleted(CCS_CookingEventArgs eventArgs)
        {
            TryCompleteActiveStepOfType(CCS_PlaytestStepType.CookFood, "Cooking completed.");

            string cookedItemId = eventArgs?.CookedItemId ?? string.Empty;
            if (IsCookedFoodItem(cookedItemId) || IsPreservedFoodItem(cookedItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyCookedFoodInInventory,
                    "Cooked food added to inventory.");
            }

            if (IsPreservedFoodItem(cookedItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.PreserveFoodAtCampfire,
                    "Preserved food created at campfire.");
            }
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
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SaveHomesteadCampState,
                    "Homestead camp state saved.");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SaveHorseState,
                    "Horse ownership and saddlebag state saved.");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SaveWagonState,
                    "Wagon ownership, hitch, and cargo state saved.");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SaveFirearmState,
                    "Firearm loaded rounds and equipment state saved.");
                if (boundSettlementService != null
                    && boundSettlementService.IsInitialized
                    && boundSettlementService.IsDiscovered(CCS_SettlementContentIds.TestTradingPostSettlementId))
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveSettlementDiscovery,
                        "Settlement discovery saved.");
                }

                if (boundRegionService != null
                    && boundRegionService.IsInitialized
                    && AreAllBootstrapRegionsDiscovered())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveRegionDiscovery,
                        "Region discovery saved.");
                }
            }
        }

        private void HandleLoadCompleted(CCS_SaveEventArgs eventArgs)
        {
            if (eventArgs != null && eventArgs.IsSuccess)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.LoadGame, "Load completed.");
                EvaluateStorageCrateStepAfterLoad();
                EvaluateBedrollStepAfterLoad();
                EvaluateCampPersistenceAfterLoad();
                EvaluateHomesteadCampPersistenceAfterLoad();
                EvaluateHorsePersistenceAfterLoad();
                EvaluateWagonPersistenceAfterLoad();
                EvaluateFirearmPersistenceAfterLoad();
                EvaluateSettlementDiscoveryAfterLoad();
                EvaluateRegionDiscoveryAfterLoad();
            }
        }

        private void EvaluateRegionDiscoveryAfterLoad()
        {
            if (boundRegionService == null || !boundRegionService.IsInitialized)
            {
                return;
            }

            if (AreAllBootstrapRegionsDiscovered())
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyRegionDiscoveryAfterLoad,
                    "All bootstrap region discoveries persisted after load.");
            }
        }

        private bool AreAllBootstrapRegionsDiscovered()
        {
            if (boundRegionService == null || !boundRegionService.IsInitialized)
            {
                return false;
            }

            return boundRegionService.IsDiscovered(CCS_RegionContentIds.PineRidgeForestRegionId)
                && boundRegionService.IsDiscovered(CCS_RegionContentIds.BrokenCreekRegionId)
                && boundRegionService.IsDiscovered(CCS_RegionContentIds.IronRidgeMineRegionId)
                && boundRegionService.IsDiscovered(CCS_RegionContentIds.FrontierTradingPostRegionId);
        }

        private void EvaluateAllRegionsDiscoveredStep()
        {
            if (!AreAllBootstrapRegionsDiscovered())
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.VerifyAllRegionsDiscovered,
                "All bootstrap frontier regions discovered.");
        }

        private void HandleRegionDiscovered(CCS_RegionSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            if (string.Equals(
                    snapshot.RegionId,
                    CCS_RegionContentIds.PineRidgeForestRegionId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DiscoverPineRidgeForestRegion,
                    "Pine Ridge Forest region discovered.");
            }

            if (string.Equals(
                    snapshot.RegionId,
                    CCS_RegionContentIds.BrokenCreekRegionId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DiscoverBrokenCreekRegion,
                    "Broken Creek region discovered.");
            }

            if (string.Equals(
                    snapshot.RegionId,
                    CCS_RegionContentIds.IronRidgeMineRegionId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DiscoverIronRidgeMineRegion,
                    "Iron Ridge Mine region discovered.");
            }

            if (string.Equals(
                    snapshot.RegionId,
                    CCS_RegionContentIds.FrontierTradingPostRegionId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DiscoverFrontierTradingPostRegion,
                    "Frontier Trading Post region discovered.");
            }

            EvaluateAllRegionsDiscoveredStep();
        }

        private void EvaluateSettlementDiscoveryAfterLoad()
        {
            if (boundSettlementService == null || !boundSettlementService.IsInitialized)
            {
                return;
            }

            if (boundSettlementService.IsDiscovered(CCS_SettlementContentIds.TestTradingPostSettlementId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifySettlementDiscoveryAfterLoad,
                    "Trading post discovery persisted after load.");
            }
        }

        private void HandleSettlementDiscovered(CCS_SettlementSnapshot snapshot)
        {
            if (snapshot == null
                || !string.Equals(
                    snapshot.SettlementId,
                    CCS_SettlementContentIds.TestTradingPostSettlementId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverTradingPost,
                "Frontier trading post discovered.");
        }

        private void HandleSettlementServicePointActivated(CCS_SettlementServicePointActivationArgs activationArgs)
        {
            if (activationArgs == null)
            {
                return;
            }

            switch (activationArgs.ServicePointType)
            {
                case CCS_SettlementServicePointType.GeneralStore:
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.InteractGeneralStoreServicePoint,
                        "General Store service point activated.");
                    break;
                case CCS_SettlementServicePointType.Stable:
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.InteractStableServicePoint,
                        "Stable service point activated.");
                    break;
                case CCS_SettlementServicePointType.Gunsmith:
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.InteractGunsmithServicePoint,
                        "Gunsmith service point activated.");
                    break;
                case CCS_SettlementServicePointType.Blacksmith:
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.InteractBlacksmithServicePoint,
                        "Blacksmith service point activated.");
                    if (activationArgs.IsSuccess
                        && activationArgs.RouteType == CCS_SettlementServiceRouteType.Industry)
                    {
                        TryCompleteActiveStepOfType(
                            CCS_PlaytestStepType.VerifySettlementBlacksmithRouting,
                            "Blacksmith routed to industry service summary.");
                    }
                    break;
            }
        }

        private void HandleMountStateChanged(CCS_MountInstance instance, CCS_MountState previousState)
        {
            if (instance == null)
            {
                return;
            }

            if (instance.State == CCS_MountState.Mounted)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.MountHorse, "Horse mounted.");
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.RideHorse, "Horse riding active.");
                if (boundVehicleService != null
                    && boundVehicleService.IsInitialized
                    && boundVehicleService.IsHitched)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.RideHorseWithWagon,
                        "Horse riding with hitched wagon.");
                }
            }
        }

        private void HandleHorseOwnershipChanged(bool ownsHorse)
        {
            if (!ownsHorse)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.BuyHorseFromStable,
                "Frontier horse ownership granted.");
        }

        private void HandleVehicleStateChanged(CCS_VehicleInstance instance, CCS_VehicleState previousState)
        {
            if (instance == null)
            {
                return;
            }

            if (instance.State == CCS_VehicleState.Hitched || instance.State == CCS_VehicleState.Moving)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.HitchWagonToHorse,
                    "Wagon hitched to owned horse.");
            }
        }

        private void HandleWagonOwnershipChanged(bool ownsWagon)
        {
            if (!ownsWagon)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.BuyWagonFromStable,
                "Frontier wagon ownership granted.");
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

            if (!string.IsNullOrWhiteSpace(eventArgs.ContainerId)
                && eventArgs.ContainerId.Contains("saddlebag", System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.OpenHorseSaddlebag,
                    "Horse saddlebag opened.");
            }

            if (!string.IsNullOrWhiteSpace(eventArgs.ContainerId)
                && eventArgs.ContainerId.Contains(".cargo", System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.OpenWagonCargo,
                    "Wagon cargo opened.");
                if (HasInventoryItem(CCS_ProspectingContentIds.IronOreItemId)
                    || HasInventoryItem(CCS_ProspectingContentIds.CoalItemId)
                    || HasInventoryItem(CCS_ProspectingContentIds.RefinedIronItemId))
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.LoadMiningGoodsIntoWagonCargo,
                        "Mining bulk goods ready for wagon haul.");
                }
            }
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

            if (boundCampService != null
                && boundCampService.IsInitialized
                && boundCampService.CurrentSnapshot.CampTier >= CCS_CampTier.TemporaryCamp)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SleepInFrontierCamp,
                    "Slept in temporary frontier camp.");
            }
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

            if (eventArgs?.PlacedInstance != null
                && string.Equals(eventArgs.PlacedInstance.PieceId, CampfirePieceId, System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.PlaceCampfireForCamp,
                    "Campfire placed for frontier camp.");
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
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.EquipWeapon,
                    $"Equipped {itemId} for playtest step.");
            }

            if (MatchesTargetItem(CCS_PlaytestStepType.EquipSpearRegression, itemId))
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.EquipSpearRegression, "Starter spear equipped (regression).");
            }

            if (MatchesTargetItem(CCS_PlaytestStepType.EquipFishingPole, itemId))
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.EquipFishingPole, "Fishing pole equipped.");
            }

            if (MatchesTargetItem(CCS_PlaytestStepType.EquipBowForHunt, itemId))
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.EquipBowForHunt, "Frontier bow equipped for hunt.");
            }

            if (MatchesTargetItem(CCS_PlaytestStepType.EquipTrapForTrapping, itemId))
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.EquipTrapForTrapping, "Simple trap equipped for placement.");
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

        private void HandleActiveItemChanged(CCS_ActiveItemState previousState, CCS_ActiveItemState newState)
        {
            if (!newState.HasActiveItem)
            {
                return;
            }

            if (MatchesTargetItem(CCS_PlaytestStepType.SelectActiveItem, newState.ActiveItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SelectActiveItem,
                    $"Active item selected: {newState.ActiveItemId}.");
            }

            if (MatchesTargetItem(CCS_PlaytestStepType.AcquirePickForMining, newState.ActiveItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.AcquirePickForMining,
                    $"Mining pick active: {newState.ActiveItemId}.");
            }
        }

        private void HandleActiveItemUsed(CCS_ActiveItemUseResult useResult)
        {
            if (useResult.ResultType == CCS_ActiveItemUseResultType.NoActiveItem)
            {
                return;
            }

            if (useResult.ResultType == CCS_ActiveItemUseResultType.FirearmReloaded)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.ReloadFirearm,
                    useResult.Message ?? "Firearm reloaded.");
            }

            if (useResult.ResultType == CCS_ActiveItemUseResultType.CombatHit
                && boundActiveItemService != null
                && boundActiveItemService.ActiveState.BehaviorType == CCS_ActiveItemBehaviorType.Firearm)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.ShootWildlifeWithFirearm,
                    useResult.Message ?? "Firearm hit wildlife.");
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.HuntWildlife, "Wildlife killed.");
            }

            if (MatchesTargetItem(CCS_PlaytestStepType.UseActiveItem, useResult.ActiveItemId)
                && (useResult.ResultType == CCS_ActiveItemUseResultType.CombatHit
                    || useResult.ResultType == CCS_ActiveItemUseResultType.NoTarget
                    || useResult.ResultType == CCS_ActiveItemUseResultType.NoBehaviorRegistered
                    || useResult.ResultType == CCS_ActiveItemUseResultType.WeaponNotEquipped
                    || useResult.ResultType == CCS_ActiveItemUseResultType.OnCooldown))
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.UseActiveItem, useResult.Message);
            }

            if (MatchesTargetItem(CCS_PlaytestStepType.UseHatchetOnTree, useResult.ActiveItemId)
                && useResult.ResultType == CCS_ActiveItemUseResultType.GatheringSuccess)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.UseHatchetOnTree, useResult.Message);
            }

            if (MatchesTargetItem(CCS_PlaytestStepType.UsePickOnRock, useResult.ActiveItemId)
                && useResult.ResultType == CCS_ActiveItemUseResultType.GatheringSuccess)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.UsePickOnRock, useResult.Message);
            }

            if (useResult.ResultType == CCS_ActiveItemUseResultType.WrongTool)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.UseWrongToolOnGatherTarget,
                    useResult.Message);
            }

            if (MatchesTargetItem(CCS_PlaytestStepType.UseFishingPoleOnSpot, useResult.ActiveItemId)
                && (useResult.ResultType == CCS_ActiveItemUseResultType.FishingSuccess
                    || useResult.ResultType == CCS_ActiveItemUseResultType.FishingFailed))
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.UseFishingPoleOnSpot, useResult.Message);
            }

            if (useResult.ResultType == CCS_ActiveItemUseResultType.HarvestSuccess)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.HarvestCarcass, useResult.Message);
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.HarvestWithKnifeAfterFirearm,
                    "Carcass harvested with knife after firearm hunt.");
                TryEvaluateHuntingPlaytestSteps();
            }

            if (MatchesTargetItem(CCS_PlaytestStepType.EquipTrapForTrapping, useResult.ActiveItemId)
                && (useResult.ResultType == CCS_ActiveItemUseResultType.TrapPlacementPreview
                    || useResult.ResultType == CCS_ActiveItemUseResultType.TrapPlaced))
            {
                if (useResult.ResultType == CCS_ActiveItemUseResultType.TrapPlaced)
                {
                    TryCompleteActiveStepOfType(CCS_PlaytestStepType.PlaceTrapForTrapping, useResult.Message);
                }
            }

            if (useResult.ResultType == CCS_ActiveItemUseResultType.TrapPlaced)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.PlaceTrapForTrapping, useResult.Message);
            }

            if (useResult.ResultType == CCS_ActiveItemUseResultType.ShelterPlaced)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.PlaceLeanToShelter, useResult.Message);
            }

            if (useResult.ResultType == CCS_ActiveItemUseResultType.HomesteadStructurePlaced)
            {
                if (MatchesTargetItem(CCS_PlaytestStepType.PlaceSupplyCrateForFrontierCamp, useResult.ActiveItemId))
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.PlaceSupplyCrateForFrontierCamp,
                        useResult.Message);
                }

                if (MatchesTargetItem(CCS_PlaytestStepType.PlaceWorkbenchForHomestead, useResult.ActiveItemId))
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.PlaceWorkbenchForHomestead,
                        useResult.Message);
                }

                TryEvaluateShelterCampPlaytestSteps();
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
            TryEvaluateFrontierRecipeValidationStep();
            TryEvaluateEconomyPlaytestSteps();
            TryEvaluateHuntingPlaytestSteps();
            TryEvaluateTrappingPlaytestSteps();
            TryEvaluateCookingPlaytestSteps();
        }

        public bool TryGrantPlaytestRawFish()
        {
            if (!CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || !TryAddInventoryItemById(inventoryService, RawFishItemId, 1))
            {
                return false;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.ObtainFishForTrade,
                "Raw fish available for vendor trade.");
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.ObtainRawFoodForCooking,
                "Raw fish available for cooking playtest.");
            return true;
        }

        public bool TryGrantPlaytestRawMeat()
        {
            if (!CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || !TryAddInventoryItemById(inventoryService, RawMeatItemId, 1))
            {
                return false;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.ObtainRawFoodForCooking,
                "Raw meat available for cooking playtest.");
            return true;
        }

        public bool TryPlaytestSellPreservedFood()
        {
            if (!EnsureVendorReadyForPlaytest())
            {
                return false;
            }

            CCS_ItemDefinition jerky = FindItemDefinitionById(JerkyItemId);
            if (jerky == null)
            {
                return false;
            }

            playtestCurrencyBaseline = GetTradeDollarsBalance();
            CCS_VendorTransactionResult result = boundVendorService.TrySellActiveVendorItem(jerky, 1);
            return result.IsSuccess;
        }

        public bool TryPlaytestSellRawFish()
        {
            if (!EnsureVendorReadyForPlaytest())
            {
                return false;
            }

            CCS_ItemDefinition rawFish = FindItemDefinitionById(RawFishItemId);
            if (rawFish == null)
            {
                return false;
            }

            playtestCurrencyBaseline = GetTradeDollarsBalance();
            CCS_VendorTransactionResult result = boundVendorService.TrySellActiveVendorItem(rawFish, 1);
            return result.IsSuccess;
        }

        public bool TryGrantPlaytestTrap()
        {
            if (!CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || !TryAddInventoryItemById(inventoryService, SimpleTrapItemId, 1))
            {
                return false;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.ObtainTrapForTrapping,
                "Simple trap available for trapping playtest.");
            return true;
        }

        public bool TryEquipTrapForTrapping()
        {
            if (!harnessEnabled || boundActiveItemService == null || !boundActiveItemService.IsInitialized)
            {
                return false;
            }

            CCS_ItemDefinition trapItem = FindItemDefinitionById(SimpleTrapItemId);
            if (trapItem == null)
            {
                return false;
            }

            if (!boundActiveItemService.SelectActiveFromTestHarness(trapItem.ItemId, trapItem))
            {
                return false;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.EquipTrapForTrapping,
                "Simple trap selected as active item.");
            return true;
        }

        public bool TryForceTrapTriggerForPlaytest()
        {
            if (boundTrapService == null || !boundTrapService.IsInitialized)
            {
                return false;
            }

            CCS_TrapResult result = boundTrapService.TryForceTriggerForPlaytest(string.Empty);
            return result.IsSuccess || result.ResultType == CCS_TrapResultType.CaptureSuccess;
        }

        public bool TryGrantPlaytestBow()
        {
            if (!CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || !TryAddInventoryItemById(inventoryService, BowItemId, 1))
            {
                return false;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.ObtainBowForHunt,
                "Frontier bow available for hunting playtest.");
            return true;
        }

        public bool TryEquipBowForHunt()
        {
            return TryEquipItemForPlaytest(BowItemId, "frontier bow", CCS_PlaytestStepType.EquipBowForHunt);
        }

        public bool TryPlaytestSellHide()
        {
            if (!EnsureVendorReadyForPlaytest())
            {
                return false;
            }

            CCS_ItemDefinition hide = FindItemDefinitionById(HideItemId);
            if (hide == null)
            {
                return false;
            }

            playtestCurrencyBaseline = GetTradeDollarsBalance();
            CCS_VendorTransactionResult result = boundVendorService.TrySellActiveVendorItem(hide, 1);
            return result.IsSuccess;
        }

        public bool TryPlaytestBuyCordage()
        {
            return TryPlaytestBuyItemById(CordageItemId);
        }

        public bool TryPlaytestBuyHatchet()
        {
            bool purchased = TryPlaytestBuyItemById(BoneHatchetItemId);
            if (purchased)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BuyHatchetForShelter,
                    "Bone hatchet acquired for shelter playtest.");
            }

            return purchased;
        }

        public bool TryGrantShelterCordage()
        {
            if (!harnessEnabled
                || !CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            CCS_ItemDefinition cordage = FindItemDefinitionById(CordageItemId);
            if (cordage == null)
            {
                return false;
            }

            inventoryService.AddItem(cordage, 4);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.AcquireCordageForShelter,
                "Cordage granted for shelter playtest.");
            return true;
        }

        public bool TryGrantHomesteadSupplyCrateKit()
        {
            return TryGrantHomesteadKit(
                SupplyCrateKitItemId,
                CCS_PlaytestStepType.BuySupplyCrateKitForHomestead,
                "Supply Crate kit granted for homestead playtest.");
        }

        public bool TryGrantHomesteadWorkbenchKit()
        {
            return TryGrantHomesteadKit(
                WorkbenchKitItemId,
                CCS_PlaytestStepType.BuyWorkbenchKitForHomestead,
                "Workbench kit granted for homestead playtest.");
        }

        private bool TryGrantHomesteadKit(
            string itemId,
            CCS_PlaytestStepType buyStepType,
            string completionMessage)
        {
            if (!harnessEnabled
                || !CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            CCS_ItemDefinition kit = FindItemDefinitionById(itemId);
            if (kit == null)
            {
                return false;
            }

            inventoryService.AddItem(kit, 1);
            TryCompleteActiveStepOfType(buyStepType, completionMessage);
            return true;
        }

        private bool TryPlaytestBuyItemById(string itemId)
        {
            if (!EnsureVendorReadyForPlaytest())
            {
                return false;
            }

            CCS_ItemDefinition item = FindItemDefinitionById(itemId);
            if (item == null)
            {
                return false;
            }

            CCS_VendorTransactionResult result = boundVendorService.TryBuyActiveVendorItem(item, 1);
            return result.IsSuccess;
        }

        private bool EnsureVendorReadyForPlaytest()
        {
            if (!harnessEnabled
                || boundVendorService == null
                || !boundVendorService.IsInitialized)
            {
                return false;
            }

            if (!boundVendorService.TryGetVendor(GeneralStoreVendorId, out CCS_VendorDefinition vendor))
            {
                return false;
            }

            boundVendorService.SetActiveVendor(vendor);
            return true;
        }

        private bool EnsureStableVendorReadyForPlaytest()
        {
            if (!harnessEnabled
                || boundVendorService == null
                || !boundVendorService.IsInitialized)
            {
                return false;
            }

            if (!boundVendorService.TryGetVendor(FrontierStableVendorId, out CCS_VendorDefinition vendor))
            {
                return false;
            }

            boundVendorService.SetActiveVendor(vendor);
            return true;
        }

        public bool TryPlaytestGrantHorseCurrency()
        {
            if (!harnessEnabled
                || boundCurrencyService == null
                || !boundCurrencyService.IsInitialized)
            {
                return false;
            }

            CCS_CurrencyTransactionResult result = boundCurrencyService.AddCurrency(
                TradeDollarsCurrencyId,
                3000,
                "Playtest horse purchase funds");
            if (result.IsSuccess)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.EarnCurrencyForHorse,
                    "Granted playtest currency for horse purchase.");
            }

            return result.IsSuccess;
        }

        public bool TryPlaytestBuyHorse()
        {
            if (!EnsureStableVendorReadyForPlaytest())
            {
                return false;
            }

            CCS_ItemDefinition horseItem = FindItemDefinitionById(FrontierHorseItemId);
            if (horseItem == null)
            {
                return false;
            }

            CCS_VendorTransactionResult result = boundVendorService.TryBuyActiveVendorItem(horseItem, 1);
            return result.IsSuccess;
        }

        public bool TryPlaytestSummonHorse()
        {
            if (boundMountService == null || !boundMountService.IsInitialized)
            {
                return false;
            }

            bool summoned = boundMountService.TrySummonHorseNearPlayer();
            if (summoned)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.SummonHorse, "Horse summoned near player.");
            }

            return summoned;
        }

        public bool TryPlaytestMountHorseShortcut()
        {
            if (boundMountService == null || !boundMountService.IsInitialized)
            {
                return false;
            }

            if (!boundMountService.OwnsHorse)
            {
                boundMountService.TryGrantHorseOwnership();
                boundMountService.TrySummonHorseNearPlayer();
            }

            return boundMountService.TryMount(boundMountService.ActiveMountInstanceId);
        }

        public bool TryPlaytestGrantWagonCurrency()
        {
            if (!harnessEnabled
                || boundCurrencyService == null
                || !boundCurrencyService.IsInitialized)
            {
                return false;
            }

            CCS_CurrencyTransactionResult result = boundCurrencyService.AddCurrency(
                TradeDollarsCurrencyId,
                5000,
                "Playtest wagon purchase funds");
            if (result.IsSuccess)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.EarnCurrencyForWagon,
                    "Granted playtest currency for wagon purchase.");
            }

            return result.IsSuccess;
        }

        public bool TryPlaytestBuyWagon()
        {
            if (!EnsureStableVendorReadyForPlaytest())
            {
                return false;
            }

            CCS_ItemDefinition wagonItem = FindItemDefinitionById(FrontierWagonItemId);
            if (wagonItem == null)
            {
                return false;
            }

            CCS_VendorTransactionResult result = boundVendorService.TryBuyActiveVendorItem(wagonItem, 1);
            return result.IsSuccess;
        }

        public bool TryPlaytestSummonWagon()
        {
            if (boundVehicleService == null || !boundVehicleService.IsInitialized)
            {
                return false;
            }

            bool summoned = boundVehicleService.TrySummonWagonNearPlayer();
            if (summoned)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.SummonWagon, "Wagon summoned near player.");
            }

            return summoned;
        }

        public bool TryPlaytestWagonFoundationShortcut()
        {
            if (boundVehicleService == null || !boundVehicleService.IsInitialized)
            {
                return false;
            }

            TryPlaytestGrantWagonCurrency();
            if (!boundVehicleService.OwnsWagon)
            {
                if (!TryPlaytestBuyWagon())
                {
                    boundVehicleService.TryGrantWagonOwnership();
                }
            }

            boundVehicleService.TrySummonWagonNearPlayer();
            if (boundMountService != null
                && boundMountService.IsInitialized
                && boundMountService.OwnsHorse)
            {
                boundVehicleService.TryHitchToOwnedHorse();
                boundMountService.TryMount(boundMountService.ActiveMountInstanceId);
            }

            return true;
        }

        public bool TryPlaytestGrantFirearmCurrency()
        {
            if (!harnessEnabled
                || boundCurrencyService == null
                || !boundCurrencyService.IsInitialized)
            {
                return false;
            }

            CCS_CurrencyTransactionResult result = boundCurrencyService.AddCurrency(
                TradeDollarsCurrencyId,
                8000,
                "Playtest firearm purchase funds");
            if (result.IsSuccess)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.EarnCurrencyForFirearm,
                    "Granted playtest currency for firearm purchase.");
            }

            return result.IsSuccess;
        }

        public bool TryPlaytestBuyRevolver()
        {
            if (!EnsureGunsmithVendorReadyForPlaytest())
            {
                return false;
            }

            CCS_ItemDefinition revolverItem = FindItemDefinitionById(FrontierRevolverItemId);
            if (revolverItem == null)
            {
                return false;
            }

            return boundVendorService.TryBuyActiveVendorItem(revolverItem, 1).IsSuccess;
        }

        public bool TryPlaytestBuyRevolverAmmo()
        {
            if (!EnsureGunsmithVendorReadyForPlaytest())
            {
                return false;
            }

            CCS_ItemDefinition ammoItem = FindItemDefinitionById(RevolverCartridgeItemId);
            if (ammoItem == null)
            {
                return false;
            }

            return boundVendorService.TryBuyActiveVendorItem(ammoItem, 24).IsSuccess;
        }

        public bool TryPlaytestEquipRevolver()
        {
            bool equipped = TryEquipItemForPlaytest(
                FrontierRevolverItemId,
                "frontier revolver",
                CCS_PlaytestStepType.EquipFirearm);
            if (equipped && boundActiveItemService != null && boundActiveItemService.IsInitialized)
            {
                boundActiveItemService.SelectActiveFromEquipped(CCS_EquipmentSlotType.MainHand);
            }

            return equipped;
        }

        public bool TryPlaytestReloadRevolver()
        {
            if (boundActiveItemService == null || !boundActiveItemService.IsInitialized)
            {
                return false;
            }

            CCS_ActiveItemUseResult result = boundActiveItemService.TryReloadActiveFirearm();
            return result.ResultType == CCS_ActiveItemUseResultType.FirearmReloaded;
        }

        public bool TryPlaytestFirearmFoundationShortcut()
        {
            TryPlaytestGrantFirearmCurrency();
            TryPlaytestBuyRevolver();
            TryPlaytestBuyRevolverAmmo();
            TryPlaytestEquipRevolver();
            TryPlaytestReloadRevolver();
            return true;
        }

        public bool TryPlaytestGrantMiningPick()
        {
            if (!harnessEnabled
                || !CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            CCS_ItemDefinition ironPick = FindItemDefinitionById(CCS_ProspectingContentIds.IronPickItemId);
            CCS_ItemDefinition primitivePick = FindItemDefinitionById(CCS_ProspectingContentIds.PrimitivePickItemId);
            if (ironPick != null)
            {
                inventoryService.AddItem(ironPick, 1);
            }

            if (primitivePick != null)
            {
                inventoryService.AddItem(primitivePick, 1);
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.AcquirePickForMining,
                "Mining picks granted for prospecting playtest.");
            return ironPick != null || primitivePick != null;
        }

        public bool TryPlaytestMiningFoundationShortcut()
        {
            playtestCurrencyBaseline = GetTradeDollarsBalance();
            TryPlaytestGrantMiningPick();
            TryGrantMiningHarvestBundle();
            TryRefineIronAtForge();
            TryLoadMiningGoodsIntoWagonCargoShortcut();
            return true;
        }

        public bool TryLoadMiningGoodsIntoWagonCargoShortcut()
        {
            if (!CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null)
            {
                return false;
            }

            CCS_ItemDefinition ironOre = FindItemDefinitionById(CCS_ProspectingContentIds.IronOreItemId);
            CCS_ItemDefinition coal = FindItemDefinitionById(CCS_ProspectingContentIds.CoalItemId);
            CCS_ItemDefinition refinedIron = FindItemDefinitionById(CCS_ProspectingContentIds.RefinedIronItemId);
            if (ironOre != null)
            {
                inventoryService.AddItem(ironOre, 6);
            }

            if (coal != null)
            {
                inventoryService.AddItem(coal, 4);
            }

            if (refinedIron != null)
            {
                inventoryService.AddItem(refinedIron, 2);
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.LoadMiningGoodsIntoWagonCargo,
                "Mining bulk goods ready for wagon haul (inventory shortcut).");
            return true;
        }

        private void TryGrantMiningHarvestBundle()
        {
            if (!CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null)
            {
                return;
            }

            CCS_ItemDefinition stone = FindItemDefinitionById(CCS_ProspectingContentIds.StoneItemId);
            CCS_ItemDefinition ironOre = FindItemDefinitionById(CCS_ProspectingContentIds.IronOreItemId);
            CCS_ItemDefinition coal = FindItemDefinitionById(CCS_ProspectingContentIds.CoalItemId);
            if (stone != null)
            {
                inventoryService.AddItem(stone, 4);
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.MineStoneOutcrop, "Stone granted for mining playtest.");
            }

            if (ironOre != null)
            {
                inventoryService.AddItem(ironOre, 4);
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.MineIronVein, "Iron ore granted for mining playtest.");
            }

            if (coal != null)
            {
                inventoryService.AddItem(coal, 4);
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.MineCoalVein, "Coal granted for mining playtest.");
            }
        }

        private static bool IsMiningTradeItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            return itemId == CCS_ProspectingContentIds.IronOreItemId
                || itemId == CCS_ProspectingContentIds.CoalItemId
                || itemId == CCS_ProspectingContentIds.RefinedIronItemId
                || itemId == CCS_ProspectingContentIds.ClayItemId
                || itemId == CCS_ProspectingContentIds.ScrapIronItemId
                || itemId == CCS_ProspectingContentIds.NailsItemId
                || itemId == CCS_ProspectingContentIds.StoneItemId
                || itemId == CCS_ProspectingContentIds.FlintItemId;
        }

        private bool EnsureGunsmithVendorReadyForPlaytest()
        {
            if (!harnessEnabled
                || boundVendorService == null
                || !boundVendorService.IsInitialized)
            {
                return false;
            }

            CCS_VendorDefinition gunsmith = FindVendorDefinitionById(FrontierGunsmithVendorId);
            if (gunsmith == null)
            {
                return false;
            }

            boundVendorService.SetActiveVendor(gunsmith);
            return true;
        }

        private CCS_VendorDefinition FindVendorDefinitionById(string vendorId)
        {
            if (boundVendorService?.ActiveProfile?.VendorProfile?.VendorDefinitions == null
                || string.IsNullOrWhiteSpace(vendorId))
            {
                return null;
            }

            CCS_VendorDefinition[] vendors = boundVendorService.ActiveProfile.VendorProfile.VendorDefinitions;
            for (int index = 0; index < vendors.Length; index++)
            {
                if (vendors[index] != null
                    && string.Equals(vendors[index].VendorId, vendorId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return vendors[index];
                }
            }

            return null;
        }

        private int GetTradeDollarsBalance()
        {
            return boundCurrencyService != null
                ? boundCurrencyService.GetBalance(TradeDollarsCurrencyId)
                : 0;
        }

        private void TryEvaluateShelterCampPlaytestSteps()
        {
            if (HasInventoryItem(LeanToKitItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.CraftLeanToShelter,
                    "Lean-To kit available in inventory.");
            }

            if (boundSleepService != null && boundSleepService.RegisteredSleepSpotCount > 0)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.PlaceBedrollForCamp,
                    "Bedroll sleep spot registered in camp radius.");
            }

            if (boundCampService != null
                && boundCampService.IsInitialized
                && boundCampService.CurrentSnapshot.CampTier == CCS_CampTier.TemporaryCamp)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyTemporaryCampTier,
                    "Temporary frontier camp tier verified.");
            }

            if (boundCampService != null
                && boundCampService.IsInitialized
                && boundCampService.CurrentSnapshot.CampTier >= CCS_CampTier.FrontierCamp)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyFrontierCampTier,
                    "Frontier camp tier verified with storage.");
            }

            if (boundCampService != null
                && boundCampService.IsInitialized
                && boundCampService.CurrentSnapshot.CampTier >= CCS_CampTier.FrontierHomestead)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyFrontierHomesteadTier,
                    "Frontier homestead tier verified.");
            }

            if (HasInventoryItem(SupplyCrateKitItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BuySupplyCrateKitForHomestead,
                    "Supply Crate kit available in inventory.");
            }

            if (HasInventoryItem(WorkbenchKitItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BuyWorkbenchKitForHomestead,
                    "Workbench kit available in inventory.");
            }

            if (boundCampService != null
                && boundCampService.IsInitialized
                && boundCampService.CurrentSnapshot.CampTier >= CCS_CampTier.IndustrialHomestead)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyIndustrialHomesteadTier,
                    "Industrial homestead tier verified.");
            }
        }

        public bool TryGrantWoodForIndustry()
        {
            if (!harnessEnabled
                || !CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            CCS_ItemDefinition wood = FindItemDefinitionById(WoodItemId);
            if (wood == null)
            {
                return false;
            }

            inventoryService.AddItem(wood, 6);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.GatherWoodForIndustry,
                "Wood granted for industry playtest.");
            return true;
        }

        public bool TryRunIndustryProcess(string processId, CCS_PlaytestStepType completionStep)
        {
            if (!TryResolveIndustryService(out CCS_IndustryService industryService))
            {
                return false;
            }

            CCS_IndustryJobResult result = industryService.TryStartProcess(processId);
            if (result.Success)
            {
                TryCompleteActiveStepOfType(completionStep, result.Message);
            }

            return result.Success;
        }

        public bool TryProduceLumberAtSawTable()
        {
            return TryRunIndustryProcess(IndustryProcessWoodLumberId, CCS_PlaytestStepType.ProduceLumberAtSawTable);
        }

        public bool TryProduceCharcoalAtKiln()
        {
            return TryRunIndustryProcess(IndustryProcessWoodCharcoalId, CCS_PlaytestStepType.ProduceCharcoalAtKiln);
        }

        public bool TryRefineIronAtForge()
        {
            if (!harnessEnabled
                || !CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            CCS_ItemDefinition ironOre = FindItemDefinitionById(IronOreItemId);
            if (ironOre != null)
            {
                inventoryService.AddItem(ironOre, 4);
            }

            bool refined = TryRunIndustryProcess(IndustryProcessIronRefineId, CCS_PlaytestStepType.RefineIronAtPrimitiveForge);
            if (refined)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.RefineMinedOreAtForge,
                    "Mined iron ore refined at forge.");
            }

            return refined;
        }

        public bool TryCraftIronHatchetHeadAtForge()
        {
            if (!TryResolveIndustryService(out CCS_IndustryService industryService))
            {
                return false;
            }

            CCS_IndustryJobResult result = industryService.TryCraftBlacksmithRecipe(
                BlacksmithIronHatchetHeadId,
                new CCS_CraftingStationContext(CCS_CraftingStationType.Forge, "Primitive Forge", string.Empty));
            if (result.Success)
            {
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.CraftIronHatchetHeadAtForge, result.Message);
            }

            return result.Success;
        }

        public bool TryGrantIronHatchetUpgrade()
        {
            if (!harnessEnabled
                || !CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            CCS_ItemDefinition ironHatchet = FindItemDefinitionById(IronHatchetItemId);
            if (ironHatchet == null)
            {
                return false;
            }

            inventoryService.AddItem(ironHatchet, 1);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.UpgradeToIronTool,
                "Iron hatchet granted for tool upgrade playtest.");
            return true;
        }

        private static bool TryResolveIndustryService(out CCS_IndustryService industryService)
        {
            industryService = null;
            if (!CCS_CharacterMovementRuntimeBridge.TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            industryService = CCS_IndustryRuntimeBridge.ResolveService(runtimeHost);
            return industryService != null && industryService.IsInitialized;
        }

        private void EvaluateCampPersistenceAfterLoad()
        {
            if (boundCampService == null || !boundCampService.IsInitialized)
            {
                return;
            }

            CCS_CampSnapshot snapshot = boundCampService.CurrentSnapshot;
            if (snapshot.HasShelter && snapshot.CampTier >= CCS_CampTier.TemporaryCamp)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyCampPersistenceAfterLoad,
                    "Camp tier and shelter state restored after load.");
            }
        }

        private void EvaluateHomesteadCampPersistenceAfterLoad()
        {
            if (boundCampService == null || !boundCampService.IsInitialized)
            {
                return;
            }

            CCS_CampSnapshot snapshot = boundCampService.CurrentSnapshot;
            if (snapshot.CampTier >= CCS_CampTier.FrontierHomestead && snapshot.HasStorage && snapshot.HasWorkArea)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyHomesteadCampPersistenceAfterLoad,
                    "Homestead camp tier and structures restored after load.");
            }

            EvaluateIndustryCampPersistenceAfterLoad();
        }

        private void EvaluateIndustryCampPersistenceAfterLoad()
        {
            if (boundCampService == null || !boundCampService.IsInitialized)
            {
                return;
            }

            CCS_CampSnapshot snapshot = boundCampService.CurrentSnapshot;
            if (snapshot.CampTier >= CCS_CampTier.IndustrialHomestead && snapshot.HasPrimitiveForge)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyIndustryCampPersistenceAfterLoad,
                    "Industrial homestead tier and forge restored after load.");
            }

            EvaluateHorsePersistenceAfterLoad();
        }

        private void EvaluateHorsePersistenceAfterLoad()
        {
            if (boundMountService == null || !boundMountService.IsInitialized || !boundMountService.OwnsHorse)
            {
                return;
            }

            CCS_MountSnapshot snapshot = boundMountService.CurrentSnapshot;
            if (snapshot.ownsMount && !string.IsNullOrWhiteSpace(snapshot.instanceId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyHorsePersistenceAfterLoad,
                    "Horse ownership and world state restored after load.");
            }
        }

        private void EvaluateWagonPersistenceAfterLoad()
        {
            if (boundVehicleService == null || !boundVehicleService.IsInitialized || !boundVehicleService.OwnsWagon)
            {
                return;
            }

            CCS_VehicleSnapshot snapshot = boundVehicleService.CurrentSnapshot;
            if (snapshot.ownsVehicle && !string.IsNullOrWhiteSpace(snapshot.instanceId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyWagonPersistenceAfterLoad,
                    "Wagon ownership and world state restored after load.");
            }
        }

        private void EvaluateFirearmPersistenceAfterLoad()
        {
            if (boundFirearmService == null || !boundFirearmService.IsInitialized)
            {
                return;
            }

            CCS_FirearmSnapshot snapshot = boundFirearmService.CurrentSnapshot;
            if (snapshot?.firearmStates != null && snapshot.firearmStates.Length > 0)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyFirearmPersistenceAfterLoad,
                    "Firearm loaded-round state restored after load.");
            }
        }

        private void TryEvaluateEconomyPlaytestSteps()
        {
            if (activeStepIndex < 0 || activeStepIndex >= stepStates.Count)
            {
                return;
            }

            CCS_PlaytestStepState state = stepStates[activeStepIndex];
            if (state.Definition.StepType == CCS_PlaytestStepType.ObtainFishForTrade)
            {
                if (HasInventoryItem(RawFishItemId))
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.ObtainFishForTrade,
                        "Raw fish obtained for trade.");
                }
            }
        }

        private void TryEvaluateTrappingPlaytestSteps()
        {
            if (activeStepIndex < 0 || activeStepIndex >= stepStates.Count)
            {
                return;
            }

            CCS_PlaytestStepState state = stepStates[activeStepIndex];
            if (state.Definition.StepType == CCS_PlaytestStepType.ObtainTrapForTrapping
                && HasInventoryItem(SimpleTrapItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.ObtainTrapForTrapping,
                    "Simple trap obtained for trapping.");
            }

            if (state.Definition.StepType == CCS_PlaytestStepType.VerifyTrapHarvestInventory
                && state.Definition.TargetItemId == HideItemId
                && HasInventoryItem(HideItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyTrapHarvestInventory,
                    "Hide verified after trap harvest.");
            }
        }

        private void TryEvaluateHuntingPlaytestSteps()
        {
            if (activeStepIndex < 0 || activeStepIndex >= stepStates.Count)
            {
                return;
            }

            CCS_PlaytestStepState state = stepStates[activeStepIndex];
            if (state.Definition.StepType == CCS_PlaytestStepType.ObtainBowForHunt
                && HasInventoryItem(BowItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.ObtainBowForHunt,
                    "Frontier bow obtained for hunting.");
            }

            if (state.Definition.StepType == CCS_PlaytestStepType.VerifyVendorInventoryUpdated
                && state.Definition.TargetItemId == HideItemId
                && HasInventoryItem(HideItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyVendorInventoryUpdated,
                    "Hide verified in player inventory after harvest.");
            }
        }

        private void HandleVendorTransactionCompleted(CCS_VendorTransactionResult result)
        {
            if (result == null || !result.IsSuccess)
            {
                return;
            }

            if (boundSettlementService != null
                && boundSettlementService.IsInitialized
                && !string.IsNullOrWhiteSpace(boundSettlementService.LastActivatedVendorId)
                && string.Equals(
                    result.VendorId,
                    boundSettlementService.LastActivatedVendorId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifySettlementVendorRouting,
                    "Settlement service point routed vendor transaction.");
            }

            if (result.WasSell && result.ItemId == RawFishItemId)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SellFishAtVendor,
                    "Sold fish at general store.");
            }

            if (result.WasSell && result.ItemId == HideItemId)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SellHuntingResourceAtVendor,
                    "Sold hide at general store.");
            }

            if (result.WasSell)
            {
                int balance = result.CurrencyBalanceAfter;
                if (balance > playtestCurrencyBaseline)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyCurrencyIncreased,
                        $"Trade dollars increased to {balance}.");

                    if (result.ItemId == HideItemId)
                    {
                        TryCompleteActiveStepOfType(
                            CCS_PlaytestStepType.VerifyHuntingCurrencyIncreased,
                            $"Hunting trade dollars increased to {balance}.");

                        TryCompleteActiveStepOfType(
                            CCS_PlaytestStepType.VerifyTrappingCurrencyIncreased,
                            $"Trapping trade dollars increased to {balance}.");
                    }
                }
            }

            if (result.WasSell && result.ItemId == HideItemId)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SellTrappingResourceAtVendor,
                    "Sold trap harvest hide at general store.");
            }

            if (result.WasSell && IsMiningTradeItem(result.ItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SellMiningGoods,
                    $"Sold mining good {result.ItemId} at vendor.");
            }

            if (result.WasSell && IsMiningTradeItem(result.ItemId) && result.CurrencyBalanceAfter > playtestCurrencyBaseline)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyMiningCurrencyIncreased,
                    $"Mining trade dollars increased to {result.CurrencyBalanceAfter}.");
            }

            if (result.WasSell && IsPreservedFoodItem(result.ItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SellPreservedFoodAtVendor,
                    "Sold preserved food at general store.");
            }

            if (result.WasSell && IsPreservedFoodItem(result.ItemId))
            {
                int balance = result.CurrencyBalanceAfter;
                if (balance > playtestCurrencyBaseline)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyCookingCurrencyIncreased,
                        $"Cooking trade dollars increased to {balance}.");
                }
            }

            if (!result.WasSell && result.ItemId == CordageItemId)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BuyItemFromVendor,
                    "Purchased cordage from general store.");

                if (result.CurrencyAmount > 0)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyCurrencyDecreased,
                        $"Trade dollars spent: {result.CurrencyAmount}.");
                }

                if (HasInventoryItem(CordageItemId))
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyVendorInventoryUpdated,
                        "Cordage added to player inventory.");
                }
            }

            if (!result.WasSell
                && string.Equals(result.VendorId, FrontierStableVendorId, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(result.ItemId, FrontierHorseItemId, System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BuyHorseFromStable,
                    "Purchased frontier horse from stable.");
            }

            if (!result.WasSell
                && string.Equals(result.VendorId, FrontierStableVendorId, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(result.ItemId, FrontierWagonItemId, System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BuyWagonFromStable,
                    "Purchased frontier wagon deed from stable.");
            }

            if (!result.WasSell
                && string.Equals(result.VendorId, FrontierGunsmithVendorId, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(result.ItemId, FrontierRevolverItemId, System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BuyRevolverFromGunsmith,
                    "Purchased frontier revolver from gunsmith.");
            }

            if (!result.WasSell
                && string.Equals(result.VendorId, FrontierGunsmithVendorId, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(result.ItemId, RevolverCartridgeItemId, System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BuyFirearmAmmo,
                    "Purchased revolver cartridges from gunsmith.");
            }

            if (!result.WasSell && result.ItemId == BoneHatchetItemId)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BuyItemFromVendor,
                    "Purchased bone hatchet from general store.");

                if (result.CurrencyAmount > 0)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyCurrencyDecreased,
                        $"Trade dollars spent: {result.CurrencyAmount} (delta {result.CurrencyDelta}).");
                }

                if (HasInventoryItem(BoneHatchetItemId))
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyVendorInventoryUpdated,
                        "Bone hatchet added to player inventory.");
                }
            }
        }

        private bool HasInventoryItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || !CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            CCS_ItemDefinition item = FindItemDefinitionById(itemId);
            return item != null && inventoryService.GetQuantity(item) > 0;
        }

        private static CCS_ItemDefinition FindItemDefinitionById(string itemId)
        {
            if (!CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService?.ActiveProfile == null)
            {
                return null;
            }

            CCS_ItemDefinition[] definitions = inventoryService.ActiveProfile.SaveRestoreItemDefinitions;
            if (definitions == null)
            {
                return null;
            }

            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_ItemDefinition definition = definitions[index];
                if (definition != null && definition.ItemId == itemId)
                {
                    return definition;
                }
            }

            return null;
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
                    return itemId == PocketKnifeItemId;
                }

                if (stepType == CCS_PlaytestStepType.EquipSpearRegression)
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

        private bool TryEquipToolForPlaytest(string toolItemId, string displayLabel)
        {
            if (!harnessEnabled || boundEquipmentService == null || !boundEquipmentService.IsInitialized)
            {
                return false;
            }

            CCS_EquipmentItemDefinition toolDefinition = FindEquipmentDefinitionForItemId(toolItemId);
            if (toolDefinition == null)
            {
                LogDebug($"{displayLabel} equipment definition was not found in the active equipment profile.");
                return false;
            }

            if (boundEquipmentService.IsSlotOccupied(CCS_EquipmentSlotType.Tool))
            {
                boundEquipmentService.UnequipItem(CCS_EquipmentSlotType.Tool);
            }

            if (!boundEquipmentService.EquipItem(toolDefinition))
            {
                LogDebug($"Failed to equip {displayLabel} for playtest.");
                return false;
            }

            if (boundActiveItemService != null && boundActiveItemService.IsInitialized)
            {
                boundActiveItemService.SelectActiveFromEquipped(CCS_EquipmentSlotType.Tool);
            }

            LogDebug($"Equipped {displayLabel} in tool slot for playtest.");
            return true;
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
                Object.FindObjectsByType<CCS_PlayerGameplayController>();
            if (players != null && players.Length > 0 && players[0] != null)
            {
                return players[0].transform.position + players[0].transform.forward * 3f + Vector3.up * 0.5f;
            }

            return new Vector3(0f, 0.5f, 3f);
        }

        private static bool IsGatherResourceItem(string itemId)
        {
            return itemId == StickItemId
                || itemId == WoodItemId
                || itemId == FiberItemId
                || itemId == SaplingItemId
                || itemId == StoneItemId
                || itemId == ScrapIronItemId;
        }

        private void TryEvaluateFrontierRecipeValidationStep()
        {
            if (activeStepIndex < 0 || activeStepIndex >= stepStates.Count)
            {
                return;
            }

            CCS_PlaytestStepState state = stepStates[activeStepIndex];
            if (state.Definition.StepType != CCS_PlaytestStepType.ValidateFrontierRecipe)
            {
                return;
            }

            string recipeId = state.Definition.TargetItemId;
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                return;
            }

            CCS_CraftingRecipeService recipeService = boundCraftingRecipeService;
            if (recipeService == null
                && !CCS_CraftingRuntimeBridge.TryGetCraftingRecipeService(out recipeService))
            {
                return;
            }

            if (recipeService.TryGetRecipeById(recipeId, out _))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.ValidateFrontierRecipe,
                    $"Frontier recipe registered: {recipeId}.");
            }
        }

        private void TryEvaluateCookingPlaytestSteps()
        {
            if (!harnessEnabled || activeStepIndex < 0 || activeStepIndex >= stepStates.Count)
            {
                return;
            }

            CCS_PlaytestStepType stepType = stepStates[activeStepIndex].Definition.StepType;
            if (stepType == CCS_PlaytestStepType.VerifyCookedFoodInInventory)
            {
                if (HasAnyInventoryItem(
                        CookedFishItemId,
                        CookedMeatItemId,
                        CookedRabbitItemId,
                        CookedVenisonItemId,
                        CookedTurkeyItemId))
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyCookedFoodInInventory,
                        "Cooked food verified in inventory.");
                }
            }
        }

        private bool HasAnyInventoryItem(params string[] itemIds)
        {
            for (int index = 0; index < itemIds.Length; index++)
            {
                if (HasInventoryItem(itemIds[index]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsCookedFoodItem(string itemId)
        {
            return itemId == CookedRabbitItemId
                || itemId == CookedVenisonItemId
                || itemId == CookedFishItemId
                || itemId == CookedMeatItemId
                || itemId == CookedTurkeyItemId;
        }

        private static bool IsPreservedFoodItem(string itemId)
        {
            return itemId == JerkyItemId || itemId == DriedFishItemId;
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
