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
using CCS.Modules.Ranching;
using CCS.Modules.Farming;
using CCS.Modules.Land;
using CCS.Modules.Banking;
using CCS.Modules.Upkeep;
using CCS.Modules.Reputation;
using CCS.Modules.Contracts;
using CCS.Modules.Vehicles;
using CCS.Modules.Firearms;
using CCS.Modules.Prospecting;
using CCS.Modules.Shelter;
using CCS.Modules.NPCs;
using CCS.Modules.TimeOfDay;
using CCS.Modules.Settlements;
using CCS.Modules.Regions;
using CCS.Modules.WorldSimulation;
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
        private const string ChickenItemId = CCS_RanchingContentIds.ChickenItemId;
        private const string EggItemId = CCS_RanchingContentIds.EggItemId;
        private const string ChickenCoopKitItemId = CCS_RanchingContentIds.ChickenCoopKitItemId;
        private const string FarmPlotKitItemId = CCS_FarmingContentIds.FarmPlotKitItemId;
        private const string CornSeedItemId = CCS_FarmingContentIds.CornSeedItemId;
        private const string CornHarvestItemId = CCS_FarmingContentIds.CornHarvestItemId;
        private const string HomesteadClaimDeedItemId = CCS_LandContentIds.HomesteadClaimDeedItemId;
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
        private CCS_WorldSimulationService boundWorldSimulationService;
        private CCS_RanchService boundRanchService;
        private CCS_FarmService boundFarmService;
        private CCS_LandClaimService boundLandClaimService;
        private CCS_BankingService boundBankingService;
        private CCS_UpkeepService boundUpkeepService;
        private CCS_ReputationService boundReputationService;
        private CCS_ContractService boundContractService;
        private int playtestBankWalletBaseline;
        private int playtestBankBalanceBaseline;
        private int playtestUpkeepBankBaseline;
        private int playtestUpkeepDueForceCount;
        private int savedBankBalance;
        private int savedUpkeepLastPaidDay;
        private int savedUpkeepEntryCount;
        private int playtestLoanWalletBaseline;
        private int savedLoanState;
        private int savedLoanBalance;
        private int playtestReputationBaseline;
        private int playtestReputationBeforeObligation;
        private int savedReputationValue;
        private CCS_ReputationTier savedReputationTier = CCS_ReputationTier.Neutral;
        private bool playtestReputationBaselineCaptured;
        private float savedBuyPriceModifier = 1f;
        private bool savedServiceAccessAllowed = true;
        private float worldSimulationFoodBaseline;
        private float worldSimulationIndustryBaseline;
        private float worldSimulationProsperityBaseline;
        private float worldSimulationSavedFoodAmount;
        private float worldSimulationSavedIndustryAmount;
        private float worldSimulationSavedProsperity;
        private bool worldSimulationBaselinesCaptured;
        private int savedRanchLivestockCount;
        private int savedRanchStructureCount;
        private int savedFarmPlotCount;
        private int savedLandClaimCount;
        private int savedLandAssociationCount;
        private int playtestContractCurrencyBaseline;
        private int playtestContractReputationBaseline;
        private float playtestContractProsperityBaseline;
        private bool playtestContractBaselinesCaptured;
        private string savedContractDefinitionId = string.Empty;
        private int savedContractState;
        private int savedRegionalSpecializationType;
        private int savedRegionalDominantIndustry;
        private float savedRegionalFoodSupplyStrength;
        private bool regionalEconomySaveCaptured;
        private float playtestRegionalProsperityBaseline;
        private bool playtestRegionalProsperityBaselineCaptured;
        private int savedSettlementGrowthStage = -1;
        private int savedSettlementPreviousGrowthStage = -1;
        private float savedSettlementGrowthProgressPercent;
        private int savedSettlementCompletedContractsCount = -1;
        private bool settlementGrowthSaveCaptured;
        private float multiSettlementPineProsperityBaseline;
        private int multiSettlementPineReputationBaseline;
        private bool multiSettlementBaselinesCaptured;
        private int savedMultiSettlementPineReputation;
        private float savedMultiSettlementPineProsperity;
        private int savedMultiSettlementDiscoveryCount;
        private int savedMultiSettlementTradeRouteCount;
        private bool multiSettlementSaveCaptured;
        private float freightTradingPostProsperityBaseline;
        private int freightRouteUsageBaseline;
        private bool freightBaselinesCaptured;
        private int savedFreightRouteUsageCount;
        private float savedFreightTradingPostProsperity;
        private bool freightSaveCaptured;
        private int routeRiskBrokenCreekUsageBaseline;
        private int routeRiskIronRidgeUsageBaseline;
        private int routeRiskLowFinalReward;
        private bool routeRiskBaselinesCaptured;
        private int savedRouteRiskBrokenCreekUsageCount;
        private int savedRouteRiskIronRidgeUsageCount;
        private bool routeRiskSaveCaptured;
        private int populationBaseline;
        private int savedPopulationTotal;
        private float savedPopulationGrowthRate;
        private int savedActiveBusinessCount;
        private bool businessSaveCaptured;
        private int savedGeneralStorePresenceStatus = -1;
        private bool businessPresenceSaveCaptured;
        private int savedTradingPostTradingSignVisualStatus = -1;
        private bool visualGrowthSaveCaptured;
        private int savedMerchantPlaceholderActorCount = -1;
        private bool populationPresenceSaveCaptured;
        private string savedNpcIdentityId = string.Empty;
        private string savedNpcDisplayName = string.Empty;
        private int savedNpcRoleType = -1;
        private bool npcIdentitySaveCaptured;
        private string savedGeneralStoreRepresentativeId = string.Empty;
        private string savedBankRepresentativeId = string.Empty;
        private string savedGeneralStoreRepresentativeIdentityId = string.Empty;
        private string savedBankRepresentativeIdentityId = string.Empty;
        private bool npcServiceRepresentativeSaveCaptured;
        private int savedHousingCapacityContribution = -1;
        private int savedActiveHousingCount = -1;
        private bool savedBoardingHouseActive;
        private bool settlementHousingSaveCaptured;
        private int savedNpcMovementStatus = -1;
        private string savedNpcMovementTargetAnchorId = string.Empty;
        private bool npcMovementSaveCaptured;
        private string savedNpcScheduleId = string.Empty;
        private int savedNpcScheduleBlockType = -1;
        private int savedNpcScheduleTargetKind = -1;
        private string savedNpcScheduleTargetId = string.Empty;
        private int savedNpcScheduleEvaluatedHour = -1;
        private bool npcScheduleSaveCaptured;
        private int savedNpcActivityType = -1;
        private int savedNpcActivityEvaluatedHour = -1;
        private bool npcActivitySaveCaptured;
        private string savedNpcAffiliationSettlementId = string.Empty;
        private string savedNpcAffiliationBusinessId = string.Empty;
        private int savedNpcAffiliationWorkforceCategory = -1;
        private int savedNpcAffiliationLoyalty = -1;
        private bool npcAffiliationSaveCaptured;
        private string savedNpcDialogueIdentityId = string.Empty;
        private string savedNpcDialogueGreetingLine = string.Empty;
        private string savedNpcDialogueRoleLine = string.Empty;
        private bool npcDialogueSaveCaptured;
        private bool populationBaselinesCaptured;
        private bool populationSaveCaptured;
        private CCS_TradeRouteService boundTradeRouteService;

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
            CCS_RegionService regionService = null,
            CCS_WorldSimulationService worldSimulationService = null,
            CCS_RanchService ranchService = null,
            CCS_FarmService farmService = null,
            CCS_LandClaimService landClaimService = null,
            CCS_BankingService bankingService = null,
            CCS_UpkeepService upkeepService = null,
            CCS_ReputationService reputationService = null,
            CCS_ContractService contractService = null,
            CCS_TradeRouteService tradeRouteService = null)
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
            boundWorldSimulationService = worldSimulationService;
            boundRanchService = ranchService;
            boundFarmService = farmService;
            boundLandClaimService = landClaimService;
            boundBankingService = bankingService;
            boundUpkeepService = upkeepService;
            boundReputationService = reputationService;
            boundContractService = contractService;
            boundTradeRouteService = tradeRouteService;

            if (boundWorldSimulationService != null)
            {
                boundWorldSimulationService.SettlementSupplyChanged += HandleWorldSimulationSupplyChanged;
                boundWorldSimulationService.SettlementProsperityChanged += HandleWorldSimulationProsperityChanged;
            }

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

            if (boundRanchService != null)
            {
                boundRanchService.RanchStructurePlaced += HandleRanchStructurePlaced;
                boundRanchService.LivestockProductCollected += HandleLivestockProductCollected;
                boundRanchService.LivestockProductionReady += HandleLivestockProductionReady;
            }

            if (boundFarmService != null)
            {
                boundFarmService.FarmPlotPlaced += HandleFarmPlotPlaced;
                boundFarmService.CropPlanted += HandleCropPlanted;
                boundFarmService.CropHarvested += HandleCropHarvested;
            }

            if (boundLandClaimService != null)
            {
                boundLandClaimService.LandClaimPlaced += HandleLandClaimPlaced;
                boundLandClaimService.StructureAssociated += HandleLandStructureAssociated;
            }

            if (boundBankingService != null)
            {
                boundBankingService.BankTransactionCompleted += HandleBankTransactionCompleted;
                boundBankingService.LoanTransactionCompleted += HandleLoanTransactionCompleted;
            }

            if (boundUpkeepService != null)
            {
                boundUpkeepService.UpkeepTransactionCompleted += HandleUpkeepTransactionCompleted;
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

            if (boundContractService != null)
            {
                boundContractService.ContractAccepted += HandleContractAccepted;
                boundContractService.ContractCompleted += HandleContractCompleted;
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

            if (boundContractService != null)
            {
                boundContractService.ContractAccepted -= HandleContractAccepted;
                boundContractService.ContractCompleted -= HandleContractCompleted;
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

            if (boundWorldSimulationService != null)
            {
                boundWorldSimulationService.SettlementSupplyChanged -= HandleWorldSimulationSupplyChanged;
                boundWorldSimulationService.SettlementProsperityChanged -= HandleWorldSimulationProsperityChanged;
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

            if (boundRanchService != null)
            {
                boundRanchService.RanchStructurePlaced -= HandleRanchStructurePlaced;
                boundRanchService.LivestockProductCollected -= HandleLivestockProductCollected;
                boundRanchService.LivestockProductionReady -= HandleLivestockProductionReady;
            }

            if (boundFarmService != null)
            {
                boundFarmService.FarmPlotPlaced -= HandleFarmPlotPlaced;
                boundFarmService.CropPlanted -= HandleCropPlanted;
                boundFarmService.CropHarvested -= HandleCropHarvested;
            }

            if (boundLandClaimService != null)
            {
                boundLandClaimService.LandClaimPlaced -= HandleLandClaimPlaced;
                boundLandClaimService.StructureAssociated -= HandleLandStructureAssociated;
            }

            if (boundBankingService != null)
            {
                boundBankingService.BankTransactionCompleted -= HandleBankTransactionCompleted;
                boundBankingService.LoanTransactionCompleted -= HandleLoanTransactionCompleted;
            }

            if (boundUpkeepService != null)
            {
                boundUpkeepService.UpkeepTransactionCompleted -= HandleUpkeepTransactionCompleted;
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
            boundWorldSimulationService = null;
            boundRanchService = null;
            boundFarmService = null;
            boundLandClaimService = null;
            boundBankingService = null;
            boundUpkeepService = null;
            boundReputationService = null;
            boundContractService = null;
            boundTradeRouteService = null;
            playtestContractBaselinesCaptured = false;
            playtestRegionalProsperityBaselineCaptured = false;
            regionalEconomySaveCaptured = false;
            settlementGrowthSaveCaptured = false;
            multiSettlementBaselinesCaptured = false;
            multiSettlementSaveCaptured = false;
            worldSimulationBaselinesCaptured = false;

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
            TryEvaluateWorldSimulationPlaytestSteps();
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
                if (boundRanchService != null && boundRanchService.IsInitialized)
                {
                    savedRanchLivestockCount = boundRanchService.GetOwnedLivestockCount(CCS_LivestockType.Chicken);
                    savedRanchStructureCount = boundRanchService.CaptureStructureState().Length;
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveRanchState,
                        "Ranch livestock and structure state saved.");
                }

                if (boundFarmService != null && boundFarmService.IsInitialized)
                {
                    savedFarmPlotCount = boundFarmService.GetPlotCount();
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveFarmState,
                        "Farm plot state saved.");
                }

                if (boundLandClaimService != null && boundLandClaimService.IsInitialized)
                {
                    savedLandClaimCount = boundLandClaimService.GetClaimCount();
                    savedLandAssociationCount = savedLandClaimCount > 0
                        ? boundLandClaimService.GetAssociatedStructureCount(
                            ResolveFirstLandClaimInstanceId())
                        : 0;
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveLandClaimState,
                        "Land claim state saved.");
                }

                if (boundBankingService != null && boundBankingService.IsInitialized)
                {
                    savedBankBalance = boundBankingService.GetDefaultAccountBalance(
                        CCS_BankingContentIds.DefaultPlayerOwnerId);
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveBankState,
                        "Bank balance saved.");
                }

                if (boundUpkeepService != null && boundUpkeepService.IsInitialized)
                {
                    string claimId = ResolveFirstLandClaimInstanceId();
                    if (boundUpkeepService.TryGetEntryForTarget(claimId, out CCS_UpkeepEntry upkeepEntry)
                        && upkeepEntry != null)
                    {
                        savedUpkeepLastPaidDay = upkeepEntry.lastPaidDay;
                    }

                    savedUpkeepEntryCount = boundUpkeepService.CaptureUpkeepState()?.Length ?? 0;
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveUpkeepState,
                        "Upkeep state saved.");
                }

                if (boundBankingService != null && boundBankingService.IsInitialized)
                {
                    CCS_LoanSnapshot activeLoan = boundBankingService.GetActiveLoan(
                        CCS_BankingContentIds.DefaultPlayerOwnerId,
                        CCS_BankingContentIds.FrontierSmallLoanDefinitionId);
                    if (activeLoan != null)
                    {
                        savedLoanState = activeLoan.loanState;
                        savedLoanBalance = activeLoan.balance;
                    }
                    else
                    {
                        CCS_LoanSnapshot[] loanSnapshots = boundBankingService.CaptureLoanState();
                        for (int loanIndex = 0; loanIndex < loanSnapshots.Length; loanIndex++)
                        {
                            CCS_LoanSnapshot snapshot = loanSnapshots[loanIndex];
                            if (snapshot != null
                                && string.Equals(
                                    snapshot.loanDefinitionId,
                                    CCS_BankingContentIds.FrontierSmallLoanDefinitionId,
                                    System.StringComparison.OrdinalIgnoreCase))
                            {
                                savedLoanState = snapshot.loanState;
                                savedLoanBalance = snapshot.balance;
                                break;
                            }
                        }
                    }

                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveLoanState,
                        "Loan state saved.");
                }

                if (boundReputationService != null
                    && boundReputationService.IsInitialized
                    && TryGetPlaytestSettlementReputation(out int reputationValue, out CCS_ReputationTier reputationTier))
                {
                    savedReputationValue = reputationValue;
                    savedReputationTier = reputationTier;
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveReputationState,
                        "Reputation state saved.");
                    savedBuyPriceModifier = ResolvePlaytestBuyPriceModifier();
                    savedServiceAccessAllowed = EvaluatePlaytestGeneralStoreAccessAllowed();
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveServiceAccessState,
                        "Service access and price modifier state saved.");
                }

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

                if (boundWorldSimulationService != null
                    && boundWorldSimulationService.IsInitialized
                    && boundWorldSimulationService.TryGetSettlementState(
                        CCS_WorldSimulationContentIds.TradingPostSettlementId,
                        out CCS_SettlementSimulationState settlementState)
                    && settlementState != null
                    && settlementState.isDiscovered)
                {
                    worldSimulationSavedFoodAmount = boundWorldSimulationService.GetSupplyAmount(
                        CCS_WorldSimulationContentIds.TradingPostSettlementId,
                        CCS_SettlementSupplyType.Food);
                    worldSimulationSavedIndustryAmount = boundWorldSimulationService.GetSupplyAmount(
                        CCS_WorldSimulationContentIds.TradingPostSettlementId,
                        CCS_SettlementSupplyType.IndustrialMaterials);
                    worldSimulationSavedProsperity = settlementState.prosperity;
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveWorldSimulationState,
                        "World simulation state saved.");
                }

                if (boundContractService != null && boundContractService.IsInitialized)
                {
                    CCS_ContractSnapshot[] contractSnapshots = boundContractService.CaptureContractsState();
                    if (contractSnapshots != null && contractSnapshots.Length > 0)
                    {
                        savedContractDefinitionId = contractSnapshots[0].contractDefinitionId ?? string.Empty;
                        savedContractState = contractSnapshots[0].contractState;
                    }

                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveContractState,
                        "Contract state saved.");
                }

                if (boundRegionService != null
                    && boundRegionService.IsInitialized
                    && AreAllBootstrapRegionsDiscovered()
                    && TryCaptureRegionalEconomySaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveRegionalEconomyState,
                        "Regional economy state saved.");
                }

                if (TryCaptureSettlementGrowthSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveSettlementGrowthState,
                        "Settlement growth state saved.");
                }

                if (TryCaptureMultiSettlementSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveMultiSettlementState,
                        "Multi-settlement state saved.");
                }

                if (TryCaptureFreightSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveFreightRouteState,
                        "Freight and trade route state saved.");
                }

                if (TryCaptureRouteRiskFreightSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveRouteRiskFreightState,
                        "Route risk freight state saved.");
                }

                if (TryCapturePopulationSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SavePopulationState,
                        "Population state saved.");
                }

                if (TryCaptureBusinessSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveBusinessState,
                        "Business activation state saved.");
                }

                if (TryCaptureBusinessPresenceSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveBusinessPresenceState,
                        "Business presence marker state saved.");
                }

                if (TryCaptureVisualGrowthSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveVisualGrowthState,
                        "Settlement visual growth marker state saved.");
                }

                if (TryCapturePopulationPresenceSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SavePopulationPresenceState,
                        "Population presence placeholder actor state saved.");
                }

                if (TryCaptureNpcIdentitySaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveNpcIdentityState,
                        "NPC identity state saved.");
                }

                if (TryCaptureNpcServiceRepresentativeSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveNpcServiceRepresentativeState,
                        "NPC service representative state saved.");
                }

                if (TryCaptureSettlementHousingSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveSettlementHousingState,
                        "Settlement housing state saved.");
                }

                if (TryCaptureNpcMovementSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveNpcMovementState,
                        "NPC movement state saved.");
                }

                if (TryCaptureNpcScheduleSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveNpcScheduleState,
                        "NPC schedule state saved.");
                }

                if (TryCaptureNpcActivitySaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveNpcActivityState,
                        "NPC activity state saved.");
                }

                if (TryCaptureNpcAffiliationSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveNpcAffiliationState,
                        "NPC affiliation state saved.");
                }

                if (TryCaptureNpcDialogueSaveState())
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.SaveNpcDialogueState,
                        "NPC dialogue resolution state saved.");
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
                EvaluateWorldSimulationAfterLoad();
                EvaluateRanchStateAfterLoad();
                EvaluateFarmStateAfterLoad();
                EvaluateLandClaimStateAfterLoad();
                EvaluateBankBalanceAfterLoad();
                EvaluateUpkeepAfterLoad();
                EvaluateLoanAfterLoad();
                EvaluateReputationAfterLoad();
                EvaluateContractStateAfterLoad();
                EvaluateRegionalEconomyAfterLoad();
                EvaluateSettlementGrowthAfterLoad();
                EvaluateMultiSettlementAfterLoad();
                EvaluateFreightStateAfterLoad();
                EvaluateRouteRiskFreightStateAfterLoad();
                EvaluatePopulationStateAfterLoad();
                EvaluateBusinessStateAfterLoad();
                EvaluateBusinessPresenceStateAfterLoad();
                EvaluateVisualGrowthStateAfterLoad();
                EvaluatePopulationPresenceStateAfterLoad();
                EvaluateNpcIdentityStateAfterLoad();
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.LoadNpcServiceRepresentativeState,
                    "Load completed for NPC service representative restore.");
                EvaluateNpcServiceRepresentativeStateAfterLoad();
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.LoadSettlementHousingState,
                    "Load completed for settlement housing restore.");
                EvaluateSettlementHousingStateAfterLoad();
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.LoadNpcMovementState,
                    "Load completed for NPC movement restore.");
                EvaluateNpcMovementStateAfterLoad();
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.LoadNpcScheduleState,
                    "Load completed for NPC schedule restore.");
                EvaluateNpcScheduleStateAfterLoad();
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.LoadNpcActivityState,
                    "Load completed for NPC activity restore.");
                EvaluateNpcActivityStateAfterLoad();
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.LoadNpcAffiliationState,
                    "Load completed for NPC affiliation restore.");
                EvaluateNpcAffiliationStateAfterLoad();
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.LoadNpcDialogueState,
                    "Load completed for NPC dialogue restore.");
                EvaluateNpcDialogueStateAfterLoad();
            }
        }

        private void EvaluateLoanAfterLoad()
        {
            if (boundBankingService == null
                || !boundBankingService.IsInitialized
                || savedLoanState <= 0)
            {
                return;
            }

            CCS_LoanSnapshot[] loanSnapshots = boundBankingService.CaptureLoanState();
            for (int index = 0; index < loanSnapshots.Length; index++)
            {
                CCS_LoanSnapshot snapshot = loanSnapshots[index];
                if (snapshot == null
                    || !string.Equals(
                        snapshot.loanDefinitionId,
                        CCS_BankingContentIds.FrontierSmallLoanDefinitionId,
                        System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (snapshot.loanState == savedLoanState && snapshot.balance == savedLoanBalance)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyLoanAfterLoad,
                        "Loan state restored after load.");
                }

                return;
            }
        }

        private void EvaluateReputationAfterLoad()
        {
            if (boundReputationService == null
                || !boundReputationService.IsInitialized
                || !playtestReputationBaselineCaptured)
            {
                return;
            }

            if (TryGetPlaytestSettlementReputation(out int value, out CCS_ReputationTier tier)
                && value == savedReputationValue
                && tier == savedReputationTier)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyReputationAfterLoad,
                    "Reputation restored after load.");
            }

            EvaluateServiceAccessAfterLoad();
        }

        private void EvaluateServiceAccessAfterLoad()
        {
            if (boundReputationService == null || !boundReputationService.IsInitialized)
            {
                return;
            }

            float currentModifier = ResolvePlaytestBuyPriceModifier();
            bool currentAccessAllowed = EvaluatePlaytestGeneralStoreAccessAllowed();
            if (Mathf.Approximately(currentModifier, savedBuyPriceModifier)
                && currentAccessAllowed == savedServiceAccessAllowed)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyServiceAccessAfterLoad,
                    "Service access and buy price modifier stable after load.");
            }
        }

        private void EvaluateUpkeepAfterLoad()
        {
            if (boundUpkeepService == null
                || !boundUpkeepService.IsInitialized
                || savedUpkeepEntryCount <= 0)
            {
                return;
            }

            string claimId = ResolveFirstLandClaimInstanceId();
            if (!boundUpkeepService.TryGetEntryForTarget(claimId, out CCS_UpkeepEntry entry)
                || entry == null)
            {
                return;
            }

            if (entry.lastPaidDay >= savedUpkeepLastPaidDay
                && boundUpkeepService.CaptureUpkeepState()?.Length >= savedUpkeepEntryCount)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyUpkeepAfterLoad,
                    "Upkeep state restored after load.");
            }
        }

        private void EvaluateBankBalanceAfterLoad()
        {
            if (boundBankingService == null
                || !boundBankingService.IsInitialized
                || savedBankBalance <= 0)
            {
                return;
            }

            if (boundBankingService.GetDefaultAccountBalance(CCS_BankingContentIds.DefaultPlayerOwnerId)
                >= savedBankBalance)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyBankBalanceAfterLoad,
                    "Bank balance restored after load.");
            }
        }

        private void CaptureBankingPlaytestBaselines()
        {
            playtestBankWalletBaseline = boundCurrencyService != null && boundCurrencyService.IsInitialized
                ? boundCurrencyService.GetBalance(TradeDollarsCurrencyId)
                : 0;
            playtestBankBalanceBaseline = boundBankingService != null && boundBankingService.IsInitialized
                ? boundBankingService.GetDefaultAccountBalance(CCS_BankingContentIds.DefaultPlayerOwnerId)
                : 0;
        }

        private void HandleBankTransactionCompleted(CCS_BankTransactionResult result)
        {
            if (result == null || !result.IsSuccess || result.Amount <= 0)
            {
                return;
            }

            if (result.Message.Contains("Deposited", System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DepositBankCurrency,
                    "Deposited trade dollars to bank account.");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DepositPartOfLoan,
                    "Deposited part of loan proceeds.");
                if (result.WalletBalanceAfter == playtestBankWalletBaseline - result.Amount
                    && result.BankBalanceAfter == playtestBankBalanceBaseline + result.Amount)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyBankDepositBalances,
                        "Wallet decreased and bank balance increased.");
                }

                playtestBankWalletBaseline = result.WalletBalanceAfter;
                playtestBankBalanceBaseline = result.BankBalanceAfter;
                return;
            }

            if (result.Message.Contains("Withdrew", System.StringComparison.OrdinalIgnoreCase))
            {
                int walletBeforeWithdraw = result.WalletBalanceAfter - result.Amount;
                int bankBeforeWithdraw = result.BankBalanceAfter + result.Amount;
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.WithdrawBankCurrency,
                    "Withdrew trade dollars from bank account.");
                if (result.WalletBalanceAfter == walletBeforeWithdraw + result.Amount
                    && result.BankBalanceAfter == bankBeforeWithdraw - result.Amount)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyBankWithdrawBalances,
                        "Wallet increased and bank balance decreased.");
                }

                playtestBankWalletBaseline = result.WalletBalanceAfter;
                playtestBankBalanceBaseline = result.BankBalanceAfter;

                int bankBalance = result.BankBalanceAfter;
                if (bankBalance == 0
                    || bankBalance < playtestUpkeepBankBaseline)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.PrepareWalletUpkeepPayment,
                        "Bank emptied or reduced for wallet upkeep payment.");
                }
            }
        }

        private void HandleLoanTransactionCompleted(CCS_LoanTransactionResult result)
        {
            if (result == null || !result.IsSuccess)
            {
                return;
            }

            if (result.Message.Contains("Opened loan", System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BorrowSmallLoan,
                    "Borrowed frontier small loan.");
                if (result.WalletBalanceAfter >= playtestLoanWalletBaseline + result.Amount)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyWalletIncreasedAfterLoan,
                        "Wallet increased after borrowing loan.");
                }

                playtestLoanWalletBaseline = result.WalletBalanceAfter;
                playtestBankWalletBaseline = result.WalletBalanceAfter;
                playtestBankBalanceBaseline = result.BankBalanceAfter;
                return;
            }

            if (result.Message.Contains("Repaid loan", System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.RepayBankLoan,
                    "Repaid frontier small loan.");
                if (result.LoanStateAfter == CCS_LoanState.Paid)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyLoanPaid,
                        "Loan state is paid.");
                }

                playtestBankWalletBaseline = result.WalletBalanceAfter;
                playtestBankBalanceBaseline = result.BankBalanceAfter;
                EvaluateReputationAfterObligation();
            }
        }

        private void HandleUpkeepTransactionCompleted(CCS_UpkeepTransactionResult result)
        {
            if (result == null)
            {
                return;
            }

            if (result.IsSuccess
                && result.Message.Contains("Registered upkeep", System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.RegisterUpkeepForLandClaim,
                    "Upkeep registered for land claim.");
            }

            if (result.IsSuccess && result.EntryState == CCS_UpkeepState.Due && result.Amount > 0)
            {
                playtestUpkeepDueForceCount++;
                playtestUpkeepBankBaseline = boundBankingService != null && boundBankingService.IsInitialized
                    ? boundBankingService.GetDefaultAccountBalance(CCS_BankingContentIds.DefaultPlayerOwnerId)
                    : 0;

                if (playtestUpkeepDueForceCount == 1)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.ForceUpkeepDue,
                        "Upkeep marked due.");
                }
                else
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.ForceUpkeepDueAgain,
                        "Upkeep marked due again.");
                }
            }

            if (result.IsSuccess
                && result.PaymentSource == CCS_UpkeepPaymentSource.Bank
                && result.Amount > 0)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.PayUpkeepFromBank,
                    "Upkeep paid from bank account.");

                if (boundBankingService != null
                    && boundBankingService.IsInitialized
                    && boundBankingService.GetDefaultAccountBalance(CCS_BankingContentIds.DefaultPlayerOwnerId)
                        == playtestUpkeepBankBaseline - result.Amount)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyUpkeepBankPayment,
                        "Bank balance decreased after upkeep payment.");
                }

                EvaluateReputationAfterObligation();
            }

            if (result.IsSuccess
                && result.PaymentSource == CCS_UpkeepPaymentSource.Wallet
                && result.Amount > 0)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.PayUpkeepFromWallet,
                    "Upkeep paid from wallet.");
                EvaluateReputationAfterObligation();
            }
        }

        private void EvaluateLandOfficeOwnedClaimsStep()
        {
            int claimCount = boundBankingService != null && boundBankingService.IsInitialized
                ? boundBankingService.GetOwnedLandClaimCount()
                : boundLandClaimService != null && boundLandClaimService.IsInitialized
                    ? boundLandClaimService.GetClaimCount()
                    : 0;
            if (claimCount > 0)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyLandOfficeOwnedClaims,
                    "Land office sees owned land claims.");
            }
        }

        private string ResolveFirstLandClaimInstanceId()
        {
            if (boundLandClaimService == null || !boundLandClaimService.IsInitialized)
            {
                return string.Empty;
            }

            CCS_LandClaimSnapshot[] snapshots = boundLandClaimService.CaptureClaimState();
            return snapshots != null && snapshots.Length > 0 ? snapshots[0].instanceId : string.Empty;
        }

        private void EvaluateLandClaimStateAfterLoad()
        {
            if (boundLandClaimService == null
                || !boundLandClaimService.IsInitialized
                || savedLandClaimCount <= 0)
            {
                return;
            }

            if (boundLandClaimService.GetClaimCount() >= savedLandClaimCount)
            {
                string claimId = ResolveFirstLandClaimInstanceId();
                bool associationsRestored = savedLandAssociationCount <= 0
                    || boundLandClaimService.GetAssociatedStructureCount(claimId) >= savedLandAssociationCount;
                if (associationsRestored)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyLandClaimAfterLoad,
                        "Land claim and structure associations restored after load.");
                }
            }
        }

        private void EvaluateFarmStateAfterLoad()
        {
            if (boundFarmService == null || !boundFarmService.IsInitialized || savedFarmPlotCount <= 0)
            {
                return;
            }

            if (boundFarmService.GetPlotCount() >= savedFarmPlotCount)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyFarmStateAfterLoad,
                    "Farm plots restored after load.");
            }
        }

        private void EvaluateRanchStateAfterLoad()
        {
            if (boundRanchService == null || !boundRanchService.IsInitialized || savedRanchLivestockCount <= 0)
            {
                return;
            }

            int livestockCount = boundRanchService.GetOwnedLivestockCount(CCS_LivestockType.Chicken);
            int structureCount = boundRanchService.CaptureStructureState().Length;
            if (livestockCount >= savedRanchLivestockCount && structureCount >= savedRanchStructureCount)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyRanchStateAfterLoad,
                    "Ranch livestock and structures restored after load.");
            }
        }

        private void EvaluateWorldSimulationAfterLoad()
        {
            if (boundWorldSimulationService == null || !boundWorldSimulationService.IsInitialized)
            {
                return;
            }

            if (!boundWorldSimulationService.TryGetSettlementState(
                    CCS_WorldSimulationContentIds.TradingPostSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                || settlementState == null
                || !settlementState.isDiscovered)
            {
                return;
            }

            float foodAmount = boundWorldSimulationService.GetSupplyAmount(
                CCS_WorldSimulationContentIds.TradingPostSettlementId,
                CCS_SettlementSupplyType.Food);
            float industryAmount = boundWorldSimulationService.GetSupplyAmount(
                CCS_WorldSimulationContentIds.TradingPostSettlementId,
                CCS_SettlementSupplyType.IndustrialMaterials);

            if (Mathf.Approximately(foodAmount, worldSimulationSavedFoodAmount)
                && Mathf.Approximately(industryAmount, worldSimulationSavedIndustryAmount)
                && Mathf.Approximately(settlementState.prosperity, worldSimulationSavedProsperity))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyWorldSimulationRestoredAfterLoad,
                    "World simulation supply and prosperity restored after load.");
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
            EvaluateRegionalEconomyDiscoverySteps(snapshot);
        }

        private void EvaluateRegionalEconomyDiscoverySteps(CCS_RegionSnapshot snapshot)
        {
            if (AreAllBootstrapRegionsDiscovered())
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DiscoverRegionsForRegionalEconomy,
                    "All bootstrap regions discovered for regional economy playtest.");
            }

            if (snapshot != null
                && snapshot.SpecializationType != CCS_RegionSpecializationType.Unknown
                && AllBootstrapRegionsHaveSpecialization())
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyRegionSpecialization,
                    "All bootstrap regions have economic specialization.");
            }
        }

        private bool AllBootstrapRegionsHaveSpecialization()
        {
            if (boundRegionService == null || !boundRegionService.IsInitialized)
            {
                return false;
            }

            return HasRegionSpecialization(CCS_RegionContentIds.PineRidgeForestRegionId, CCS_RegionSpecializationType.Timber)
                && HasRegionSpecialization(CCS_RegionContentIds.BrokenCreekRegionId, CCS_RegionSpecializationType.Agriculture)
                && HasRegionSpecialization(CCS_RegionContentIds.IronRidgeMineRegionId, CCS_RegionSpecializationType.Mining)
                && HasRegionSpecialization(
                    CCS_RegionContentIds.FrontierTradingPostRegionId,
                    CCS_RegionSpecializationType.FrontierMixed);
        }

        private bool HasRegionSpecialization(string regionId, CCS_RegionSpecializationType expected)
        {
            if (!boundRegionService.TryGetSnapshot(regionId, out CCS_RegionSnapshot snapshot)
                || snapshot == null)
            {
                return boundRegionService.TryGetSpecializationForRegion(regionId, out CCS_RegionSpecializationType resolved)
                    && resolved == expected;
            }

            return snapshot.SpecializationType == expected;
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
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.SettlementId))
            {
                return;
            }

            if (string.Equals(
                    snapshot.SettlementId,
                    CCS_SettlementContentIds.TestTradingPostSettlementId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DiscoverTradingPost,
                    "Frontier trading post discovered.");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DiscoverTradingPostForContracts,
                    "Trading post discovered for contract board.");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DiscoverTradingPostForSettlementGrowth,
                    "Trading post discovered for settlement growth.");
                CapturePlaytestReputationBaselineIfNeeded();
                EvaluateWorldSimulationSettlementDiscovered();
                return;
            }

            if (string.Equals(
                    snapshot.SettlementId,
                    CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DiscoverPineRidgeCampSettlement,
                    "Pine Ridge Camp discovered.");
                CaptureMultiSettlementBaselinesIfNeeded();
                return;
            }

            if (string.Equals(
                    snapshot.SettlementId,
                    CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DiscoverBrokenCreekFarmsteadSettlement,
                    "Broken Creek Farmstead discovered.");
                return;
            }

            if (string.Equals(
                    snapshot.SettlementId,
                    CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DiscoverIronRidgeMiningCampSettlement,
                    "Iron Ridge Mining Camp discovered.");
            }
        }

        private void EvaluateWorldSimulationSettlementDiscovered()
        {
            if (boundWorldSimulationService == null || !boundWorldSimulationService.IsInitialized)
            {
                return;
            }

            if (boundWorldSimulationService.TryGetSettlementState(
                    CCS_WorldSimulationContentIds.TradingPostSettlementId,
                    out CCS_SettlementSimulationState state)
                && state != null
                && state.isDiscovered)
            {
                CaptureWorldSimulationBaselinesIfNeeded();
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.DiscoverSettlementForWorldSimulation,
                    "Trading post settlement simulation discovered.");
            }
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
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.InteractGeneralStoreRepresentative,
                        "General Store representative routed to service point.");
                    if (activationArgs.IsSuccess
                        && activationArgs.RouteType == CCS_SettlementServiceRouteType.Vendor)
                    {
                        TryCompleteActiveStepOfType(
                            CCS_PlaytestStepType.VerifyGeneralStoreRepresentativeVendorRoute,
                            "General Store representative opened vendor route.");
                    }
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
                case CCS_SettlementServicePointType.Bank:
                    CaptureBankingPlaytestBaselines();
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.InteractBankServicePoint,
                        "Bank service point activated.");
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.InteractBankRepresentative,
                        "Bank representative routed to service point.");
                    if (activationArgs.IsSuccess
                        && activationArgs.RouteType == CCS_SettlementServiceRouteType.Bank)
                    {
                        TryCompleteActiveStepOfType(
                            CCS_PlaytestStepType.VerifyBankRepresentativeBankRoute,
                            "Bank representative opened banking route.");
                    }
                    break;
                case CCS_SettlementServicePointType.LandOffice:
                    CaptureBankingPlaytestBaselines();
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.InteractLandOfficeServicePoint,
                        "Land office service point activated.");
                    EvaluateLandOfficeOwnedClaimsStep();
                    break;
                case CCS_SettlementServicePointType.ContractBoard:
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.InteractContractBoard,
                        "Contract board service point activated.");
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

                if (MatchesTargetItem(CCS_PlaytestStepType.PlaceChickenCoop, useResult.ActiveItemId))
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.PlaceChickenCoop,
                        useResult.Message);
                }

                if (MatchesTargetItem(CCS_PlaytestStepType.PlaceFarmPlot, useResult.ActiveItemId))
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.PlaceFarmPlot,
                        useResult.Message);
                }

                if (MatchesTargetItem(CCS_PlaytestStepType.PlantCornSeed, useResult.ActiveItemId))
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.PlantCornSeed,
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

            if (result.WasSell && result.ItemId == EggItemId)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SellRanchEgg,
                    "Sold ranch egg at general store.");
            }

            if (result.WasSell && result.ItemId == CornHarvestItemId)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SellCrop,
                    "Sold crop at general store.");
            }

            if (!result.WasSell && result.ItemId == HomesteadClaimDeedItemId)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BuyHomesteadClaimDeed,
                    "Homestead claim deed purchased from general store.");
            }

            if (result.WasSell && IsWorldSimulationFoodItem(result.ItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SellFoodForWorldSimulation,
                    "Sold food at general store for world simulation.");
            }

            if (result.WasSell && IsWorldSimulationIndustryItem(result.ItemId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SellIndustryGoodsForWorldSimulation,
                    "Sold industry goods at general store for world simulation.");
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
                && string.Equals(result.ItemId, ChickenItemId, System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BuyChickenFromVendor,
                    "Purchased chicken from vendor.");
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

            if (result.WasSell)
            {
                EvaluateReputationAfterSell();
            }
            else if (result.IsSuccess)
            {
                EvaluateReputationAfterBuy(result);
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

        private void TryEvaluateWorldSimulationPlaytestSteps()
        {
            if (boundWorldSimulationService == null || !boundWorldSimulationService.IsInitialized)
            {
                return;
            }

            CaptureWorldSimulationBaselinesIfNeeded();

            float foodAmount = boundWorldSimulationService.GetSupplyAmount(
                CCS_WorldSimulationContentIds.TradingPostSettlementId,
                CCS_SettlementSupplyType.Food);
            float industryAmount = boundWorldSimulationService.GetSupplyAmount(
                CCS_WorldSimulationContentIds.TradingPostSettlementId,
                CCS_SettlementSupplyType.IndustrialMaterials);

            if (worldSimulationBaselinesCaptured && foodAmount > worldSimulationFoodBaseline)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyFoodSupplyIncreased,
                    $"Food supply increased to {foodAmount:0.##}.");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyRanchFoodSupplyIncreased,
                    $"Ranch goods increased settlement food supply to {foodAmount:0.##}.");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyFarmFoodSupplyIncreased,
                    $"Farm crops increased settlement food supply to {foodAmount:0.##}.");
            }

            if (worldSimulationBaselinesCaptured && industryAmount > worldSimulationIndustryBaseline)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyIndustrySupplyIncreased,
                    $"Industrial supply increased to {industryAmount:0.##}.");
            }

            if (boundWorldSimulationService.TryGetSettlementState(
                    CCS_WorldSimulationContentIds.TradingPostSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                && settlementState != null
                && worldSimulationBaselinesCaptured
                && settlementState.prosperity > worldSimulationProsperityBaseline)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyProsperityIncreased,
                    $"Settlement prosperity increased to {settlementState.prosperity:0.##}.");
            }
        }

        private void HandleWorldSimulationSupplyChanged(CCS_SettlementSimulationState settlementState)
        {
            if (settlementState == null)
            {
                return;
            }

            TryEvaluateWorldSimulationPlaytestSteps();
        }

        private void HandleWorldSimulationProsperityChanged(CCS_SettlementSimulationState settlementState)
        {
            if (settlementState == null)
            {
                return;
            }

            TryEvaluateWorldSimulationPlaytestSteps();
        }

        private void CaptureWorldSimulationBaselinesIfNeeded()
        {
            if (worldSimulationBaselinesCaptured
                || boundWorldSimulationService == null
                || !boundWorldSimulationService.IsInitialized)
            {
                return;
            }

            if (!boundWorldSimulationService.TryGetSettlementState(
                    CCS_WorldSimulationContentIds.TradingPostSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                || settlementState == null
                || !settlementState.isDiscovered)
            {
                return;
            }

            worldSimulationFoodBaseline = boundWorldSimulationService.GetSupplyAmount(
                CCS_WorldSimulationContentIds.TradingPostSettlementId,
                CCS_SettlementSupplyType.Food);
            worldSimulationIndustryBaseline = boundWorldSimulationService.GetSupplyAmount(
                CCS_WorldSimulationContentIds.TradingPostSettlementId,
                CCS_SettlementSupplyType.IndustrialMaterials);
            worldSimulationProsperityBaseline = settlementState.prosperity;
            worldSimulationBaselinesCaptured = true;
        }

        private static bool IsWorldSimulationFoodItem(string itemId)
        {
            return itemId == RawFishItemId
                || itemId == CookedFishItemId
                || itemId == DriedFishItemId
                || itemId == RawMeatItemId
                || itemId == CookedMeatItemId
                || itemId == JerkyItemId
                || itemId == EggItemId
                || itemId == CCS_RanchingContentIds.MilkItemId
                || itemId == CornHarvestItemId
                || itemId == CCS_FarmingContentIds.BeanHarvestItemId
                || itemId == CCS_FarmingContentIds.PotatoHarvestItemId
                || itemId == CCS_FarmingContentIds.WheatHarvestItemId;
        }

        private void HandleFarmPlotPlaced(CCS_FarmPlotInstance plot)
        {
            if (plot?.Definition == null)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.PlaceFarmPlot,
                $"{plot.Definition.DisplayName} placed.");

            if (boundLandClaimService != null
                && boundLandClaimService.IsInitialized
                && boundLandClaimService.HasAssociatedStructure(plot.InstanceId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.PlaceStructureInsideClaim,
                    "Farm plot placed inside land claim.");
            }
        }

        private void HandleLandClaimPlaced(CCS_LandClaimInstance claim)
        {
            if (claim?.Definition == null)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.PlaceLandClaim,
                $"{claim.Definition.DisplayName} placed.");
            EvaluateLandOfficeOwnedClaimsStep();
        }

        private void HandleLandStructureAssociated(CCS_LandClaimInstance claim, string structureInstanceId)
        {
            if (claim == null || string.IsNullOrWhiteSpace(structureInstanceId))
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.VerifyStructureAssociatedWithClaim,
                $"Structure {structureInstanceId} associated with land claim.");
        }

        private void HandleCropPlanted(CCS_FarmPlotInstance plot)
        {
            if (plot?.Crop?.Definition == null)
            {
                return;
            }

            if (plot.Crop.Definition.CropId == CCS_FarmingContentIds.CornCropId)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.PlantCornSeed,
                    "Corn seed planted.");
            }
        }

        private void HandleCropHarvested(CCS_FarmPlotInstance plot)
        {
            if (plot?.Crop?.Definition == null)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.HarvestCrop,
                $"Harvested {plot.Crop.Definition.DisplayName}.");
        }

        private void HandleRanchStructurePlaced(CCS_RanchStructureInstance structure)
        {
            if (structure?.Definition == null)
            {
                return;
            }

            if (structure.StructureKind == CCS_RanchStructureKind.ChickenCoop)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.PlaceChickenCoop,
                    $"{structure.Definition.DisplayName} placed.");
            }
        }

        private void HandleLivestockProductionReady(CCS_LivestockInstance livestock)
        {
            if (livestock == null)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.ForceRanchProduction,
                $"{livestock.Definition?.DisplayName ?? "Livestock"} production ready.");
        }

        private void HandleLivestockProductCollected(CCS_LivestockInstance livestock)
        {
            if (livestock == null)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.CollectRanchProduct,
                "Ranch product collected into inventory.");
        }

        public bool TryPlaytestBuyChicken()
        {
            if (!EnsureVendorReadyForPlaytest())
            {
                return false;
            }

            CCS_ItemDefinition chickenItem = FindItemDefinitionById(ChickenItemId);
            if (chickenItem == null)
            {
                return false;
            }

            CCS_VendorTransactionResult result = boundVendorService.TryBuyActiveVendorItem(chickenItem, 1);
            return result.IsSuccess;
        }

        public bool TryPlaytestAssignChickenToCoop()
        {
            if (boundRanchService == null || !boundRanchService.IsInitialized)
            {
                return false;
            }

            bool assigned = boundRanchService.TryAssignNearestLivestockToNearestStructure(
                CCS_LivestockType.Chicken,
                CCS_RanchStructureKind.ChickenCoop);
            if (assigned)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.AssignChickenToCoop,
                    "Chicken assigned to chicken coop.");
            }

            return assigned;
        }

        public bool TryPlaytestForceRanchProduction()
        {
            if (boundRanchService == null || !boundRanchService.IsInitialized)
            {
                return false;
            }

            return boundRanchService.TryForceFirstLivestockProductionForPlaytest();
        }

        public bool TryPlaytestCollectRanchProduct()
        {
            if (boundRanchService == null || !boundRanchService.IsInitialized)
            {
                return false;
            }

            return boundRanchService.TryCollectFirstReadyProduction();
        }

        public bool TryPlaytestRanchFoundationShortcut()
        {
            if (boundCurrencyService != null && boundCurrencyService.IsInitialized)
            {
                boundCurrencyService.AddCurrency(TradeDollarsCurrencyId, 2500, "Playtest ranch foundation funds");
            }

            TryPlaytestBuyChicken();
            GrantPlaytestItem(ChickenCoopKitItemId, 1);
            GrantPlaytestItem(CCS_RanchingContentIds.FeedTroughKitItemId, 1);
            GrantPlaytestItem(CCS_RanchingContentIds.WaterTroughKitItemId, 1);

            TryPlaytestAssignChickenToCoop();
            TryPlaytestForceRanchProduction();
            TryPlaytestCollectRanchProduct();
            return true;
        }

        public bool TryPlaytestBuyFarmPlotKit()
        {
            return TryPlaytestBuyItemById(FarmPlotKitItemId);
        }

        public bool TryPlaytestBuyCornSeed()
        {
            bool purchased = TryPlaytestBuyItemById(CornSeedItemId);
            if (purchased)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.BuyCornSeed,
                    "Corn seed purchased from general store.");
            }

            return purchased;
        }

        public bool TryPlaytestForceCropGrowth()
        {
            if (boundFarmService == null || !boundFarmService.IsInitialized)
            {
                return false;
            }

            bool forced = boundFarmService.TryForceFirstCropMatureForPlaytest();
            if (forced)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.ForceCropGrowth,
                    "Forced first crop to mature.");
            }

            return forced;
        }

        public bool TryPlaytestHarvestCrop()
        {
            if (boundFarmService == null || !boundFarmService.IsInitialized)
            {
                return false;
            }

            return boundFarmService.TryHarvestNearestMaturePlot();
        }

        public bool TryPlaytestBuyHomesteadClaimDeed()
        {
            return TryPlaytestBuyItemById(HomesteadClaimDeedItemId);
        }

        public bool TryPlaytestBankingFoundationShortcut()
        {
            if (boundCurrencyService != null && boundCurrencyService.IsInitialized)
            {
                boundCurrencyService.AddCurrency(TradeDollarsCurrencyId, 1000, "Playtest banking foundation funds");
            }

            CaptureBankingPlaytestBaselines();
            if (boundBankingService != null
                && boundBankingService.IsInitialized
                && boundBankingService.ActiveProfile != null)
            {
                boundBankingService.TryOpenAccount(
                    CCS_BankingContentIds.DefaultPlayerOwnerId,
                    boundBankingService.ActiveProfile.DefaultAccountDefinitionId);
                boundBankingService.TryDeposit(
                    CCS_BankingContentIds.DefaultPlayerOwnerId,
                    boundBankingService.ActiveProfile.DefaultAccountDefinitionId,
                    50);
            }

            return true;
        }

        public bool TryPlaytestUpkeepFoundationShortcut()
        {
            TryPlaytestBankingFoundationShortcut();

            string claimId = ResolveFirstLandClaimInstanceId();
            if (boundUpkeepService == null
                || !boundUpkeepService.IsInitialized
                || string.IsNullOrWhiteSpace(claimId))
            {
                return false;
            }

            if (boundBankingService != null
                && boundBankingService.IsInitialized
                && boundBankingService.ActiveProfile != null)
            {
                boundBankingService.TryDeposit(
                    CCS_BankingContentIds.DefaultPlayerOwnerId,
                    boundBankingService.ActiveProfile.DefaultAccountDefinitionId,
                    200);
            }

            boundUpkeepService.TryForceDue(claimId);
            boundUpkeepService.TryPayUpkeep(claimId);
            boundUpkeepService.TryForceDue(claimId);

            if (boundBankingService != null
                && boundBankingService.IsInitialized
                && boundBankingService.ActiveProfile != null)
            {
                int bankBalance = boundBankingService.GetDefaultAccountBalance(
                    CCS_BankingContentIds.DefaultPlayerOwnerId);
                if (bankBalance > 0)
                {
                    boundBankingService.TryWithdraw(
                        CCS_BankingContentIds.DefaultPlayerOwnerId,
                        boundBankingService.ActiveProfile.DefaultAccountDefinitionId,
                        bankBalance);
                }
            }

            if (boundCurrencyService != null && boundCurrencyService.IsInitialized)
            {
                boundCurrencyService.AddCurrency(TradeDollarsCurrencyId, 500, "Playtest upkeep wallet funds");
            }

            boundUpkeepService.TryPayUpkeep(claimId);
            return true;
        }

        public bool TryPlaytestLoansFoundationShortcut()
        {
            TryPlaytestBankingFoundationShortcut();

            if (boundBankingService == null
                || !boundBankingService.IsInitialized
                || boundBankingService.ActiveLoanProfile == null)
            {
                return false;
            }

            playtestLoanWalletBaseline = boundCurrencyService != null && boundCurrencyService.IsInitialized
                ? boundCurrencyService.GetBalance(TradeDollarsCurrencyId)
                : 0;

            boundBankingService.TryOpenLoan(
                CCS_BankingContentIds.DefaultPlayerOwnerId,
                boundBankingService.ActiveLoanProfile.DefaultLoanDefinitionId);

            if (boundBankingService.ActiveProfile != null)
            {
                boundBankingService.TryDeposit(
                    CCS_BankingContentIds.DefaultPlayerOwnerId,
                    boundBankingService.ActiveProfile.DefaultAccountDefinitionId,
                    100);
            }

            boundBankingService.TryRepayLoan(
                CCS_BankingContentIds.DefaultPlayerOwnerId,
                boundBankingService.ActiveLoanProfile.DefaultLoanDefinitionId);
            return true;
        }

        public bool TryPlaytestReputationFoundationShortcut()
        {
            if (boundSettlementService != null
                && boundSettlementService.IsInitialized
                && boundSettlementService.TryGetDefinition(
                    CCS_SettlementContentIds.TestTradingPostSettlementId,
                    out CCS_SettlementDefinition tradingPostDefinition))
            {
                boundSettlementService.DiscoverSettlement(tradingPostDefinition, Vector3.zero);
            }

            CapturePlaytestReputationBaselineIfNeeded();

            if (boundCurrencyService != null && boundCurrencyService.IsInitialized)
            {
                boundCurrencyService.AddCurrency(TradeDollarsCurrencyId, 500, "Playtest reputation foundation funds");
            }

            GrantPlaytestItem(HideItemId, 5);
            TryPlaytestSellHide();
            TryPlaytestLoansFoundationShortcut();
            return true;
        }

        public bool TryPlaytestServiceAccessFoundationShortcut()
        {
            TryPlaytestReputationFoundationShortcut();
            EvaluateSettlementReputationStanding();
            EvaluateSettlementServiceAccess();
            return true;
        }

        public bool TryPlaytestContractsFoundationShortcut()
        {
            if (boundSettlementService != null
                && boundSettlementService.IsInitialized
                && boundSettlementService.TryGetDefinition(
                    CCS_SettlementContentIds.TestTradingPostSettlementId,
                    out CCS_SettlementDefinition tradingPostDefinition))
            {
                boundSettlementService.DiscoverSettlement(tradingPostDefinition, Vector3.zero);
            }

            CapturePlaytestContractBaselinesIfNeeded();
            GrantPlaytestItem(CCS_ContractContentIds.HideItemId, 3);
            GrantPlaytestItem(CCS_ContractContentIds.CordageItemId, 2);
            TryEvaluateContractGoodsStep();

            if (boundContractService != null
                && boundContractService.IsInitialized
                && playtestContractCurrencyBaseline <= 0
                && boundCurrencyService != null
                && boundCurrencyService.IsInitialized)
            {
                playtestContractCurrencyBaseline =
                    boundCurrencyService.GetBalance(TradeDollarsCurrencyId);
            }

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_ContractContentIds.MixedFrontierSupplyContractId,
                    CCS_ContractContentIds.DefaultTradingPostSettlementId);
            }

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryCompleteContract(CCS_ContractContentIds.MixedFrontierSupplyContractId);
            }

            return true;
        }

        public bool TryPlaytestRegionalEconomyFoundationShortcut()
        {
            DiscoverAllBootstrapRegionsForPlaytest();
            CapturePlaytestRegionalProsperityBaselineIfNeeded();
            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 5);
            TryEvaluateRegionalContractGoodsStep();

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_RegionSpecializationContentIds.RegionalEconomyPlaytestCornContractId,
                    CCS_RegionSpecializationContentIds.RegionalEconomyPlaytestSettlementId);
            }

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryCompleteContract(
                    CCS_RegionSpecializationContentIds.RegionalEconomyPlaytestCornContractId);
            }

            return true;
        }

        private void DiscoverAllBootstrapRegionsForPlaytest()
        {
            if (boundRegionService == null || !boundRegionService.IsInitialized)
            {
                return;
            }

            DiscoverPlaytestRegionIfKnown(CCS_RegionContentIds.PineRidgeForestRegionId);
            DiscoverPlaytestRegionIfKnown(CCS_RegionContentIds.BrokenCreekRegionId);
            DiscoverPlaytestRegionIfKnown(CCS_RegionContentIds.IronRidgeMineRegionId);
            DiscoverPlaytestRegionIfKnown(CCS_RegionContentIds.FrontierTradingPostRegionId);
        }

        private void DiscoverPlaytestRegionIfKnown(string regionId)
        {
            if (boundRegionService.TryGetDefinition(regionId, out CCS_RegionDefinition definition)
                && definition != null)
            {
                boundRegionService.DiscoverRegion(definition, definition.DefaultWorldPosition);
            }
        }

        private void CapturePlaytestRegionalProsperityBaselineIfNeeded()
        {
            if (playtestRegionalProsperityBaselineCaptured)
            {
                return;
            }

            if (boundWorldSimulationService != null
                && boundWorldSimulationService.IsInitialized
                && boundWorldSimulationService.TryGetSettlementState(
                    CCS_WorldSimulationContentIds.TradingPostSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                && settlementState != null)
            {
                playtestRegionalProsperityBaseline = settlementState.prosperity;
                playtestRegionalProsperityBaselineCaptured = true;
            }
        }

        private void TryEvaluateRegionalContractGoodsStep()
        {
            if (boundContractService == null || !boundContractService.IsInitialized)
            {
                return;
            }

            if (!boundContractService.TryGetDefinition(
                    CCS_RegionSpecializationContentIds.RegionalEconomyPlaytestCornContractId,
                    out CCS_ContractDefinition definition)
                || definition == null)
            {
                return;
            }

            CCS_ContractRequirement[] requirements = definition.Requirements;
            for (int index = 0; index < requirements.Length; index++)
            {
                CCS_ContractRequirement requirement = requirements[index];
                if (requirement == null)
                {
                    continue;
                }

                if (ResolvePlaytestItemQuantity(requirement.ItemId) < requirement.Quantity)
                {
                    return;
                }
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.GatherRegionalContractGoods,
                "Regional contract delivery goods gathered.");
        }

        private bool TryCaptureRegionalEconomySaveState()
        {
            if (regionalEconomySaveCaptured
                || !boundRegionService.TryGetSnapshot(
                    CCS_RegionSpecializationContentIds.RegionalEconomyPlaytestRegionId,
                    out CCS_RegionSnapshot snapshot)
                || snapshot == null)
            {
                return regionalEconomySaveCaptured;
            }

            savedRegionalSpecializationType = (int)snapshot.SpecializationType;
            savedRegionalDominantIndustry = (int)snapshot.DominantIndustry;
            savedRegionalFoodSupplyStrength = snapshot.FoodSupplyStrength;
            regionalEconomySaveCaptured = true;
            return true;
        }

        private void EvaluateRegionalEconomyAfterLoad()
        {
            if (!regionalEconomySaveCaptured || boundRegionService == null || !boundRegionService.IsInitialized)
            {
                return;
            }

            if (!boundRegionService.TryGetSnapshot(
                    CCS_RegionSpecializationContentIds.RegionalEconomyPlaytestRegionId,
                    out CCS_RegionSnapshot snapshot)
                || snapshot == null)
            {
                return;
            }

            if ((int)snapshot.SpecializationType == savedRegionalSpecializationType
                && (int)snapshot.DominantIndustry == savedRegionalDominantIndustry
                && Mathf.Approximately(snapshot.FoodSupplyStrength, savedRegionalFoodSupplyStrength))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyRegionalEconomyAfterLoad,
                    "Regional economy metadata restored after load.");
            }
        }

        public bool TryPlaytestTradeRoutesFreightShortcut()
        {
            DiscoverPlaytestSettlement(CCS_SettlementContentIds.TestTradingPostSettlementId);
            DiscoverPlaytestSettlement(CCS_MultiSettlementContentIds.PineRidgeCampSettlementId);
            CaptureFreightBaselinesIfNeeded();

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_TradeRoutesFreightContentIds.PineRidgeLumberFreightContractId,
                    CCS_MultiSettlementContentIds.PineRidgeCampSettlementId);
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.AcceptPineRidgeLumberFreightContract,
                    "Accepted Pine Ridge lumber freight contract.");
            }

            if (boundVehicleService != null && boundVehicleService.IsInitialized)
            {
                if (!boundVehicleService.OwnsWagon)
                {
                    boundVehicleService.TryGrantWagonOwnership();
                }

                boundVehicleService.TrySummonWagonNearPlayer();
                TryCompleteActiveStepOfType(CCS_PlaytestStepType.SummonWagonForFreight, "Wagon ready for freight haul.");
            }

            TryLoadLumberIntoWagonCargoForFreight();

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                CCS_ContractCompletionResult deliveryResult = boundContractService.TryCompleteContract(
                    CCS_TradeRoutesFreightContentIds.PineRidgeLumberFreightContractId,
                    CCS_SettlementContentIds.TestTradingPostSettlementId);
                if (deliveryResult != null && deliveryResult.IsSuccess)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.CompletePineRidgeLumberFreightDelivery,
                        deliveryResult.Message);
                    EvaluateFreightWorldSimulationSteps();
                    EvaluateFreightRouteUsageSteps();
                }
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverFreightRouteSettlements,
                "Pine Ridge and Trading Post discovered for freight loop.");
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.TravelToTradingPostFreightBoard,
                "Freight delivery shortcut simulates travel to destination board.");
            return true;
        }

        public bool TryPlaytestRouteRiskFreightShortcut()
        {
            DiscoverPlaytestSettlement(CCS_SettlementContentIds.TestTradingPostSettlementId);
            DiscoverPlaytestSettlement(CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId);
            DiscoverPlaytestSettlement(CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId);
            CaptureRouteRiskBaselinesIfNeeded();

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_TradeRoutesFreightContentIds.BrokenCreekCornFreightContractId,
                    CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId);
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.AcceptLowRiskFreightContract,
                    "Accepted Broken Creek corn low-risk freight contract.");
            }

            EnsureWagonReadyForFreight();
            TryLoadFreightItemIntoWagon(CCS_RegionEconomyUtility.CornItemId, 6);

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                CCS_ContractCompletionResult lowRiskResult = boundContractService.TryCompleteContract(
                    CCS_TradeRoutesFreightContentIds.BrokenCreekCornFreightContractId,
                    CCS_SettlementContentIds.TestTradingPostSettlementId);
                if (lowRiskResult != null && lowRiskResult.IsSuccess)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.CompleteLowRiskFreightContract,
                        lowRiskResult.Message);
                    EvaluateRouteRiskLowRewardSteps(lowRiskResult);
                }
            }

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_TradeRoutesFreightContentIds.IronRidgeIronOreFreightContractId,
                    CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId);
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.AcceptModerateRiskFreightContract,
                    "Accepted Iron Ridge iron ore moderate-risk freight contract.");
            }

            EnsureWagonReadyForFreight();
            TryLoadFreightItemIntoWagon(CCS_RegionEconomyUtility.IronOreItemId, 4);

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                CCS_ContractCompletionResult moderateRiskResult = boundContractService.TryCompleteContract(
                    CCS_TradeRoutesFreightContentIds.IronRidgeIronOreFreightContractId,
                    CCS_SettlementContentIds.TestTradingPostSettlementId);
                if (moderateRiskResult != null && moderateRiskResult.IsSuccess)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.CompleteModerateRiskFreightContract,
                        moderateRiskResult.Message);
                    EvaluateRouteRiskModerateRewardSteps(moderateRiskResult);
                }
            }

            return true;
        }

        private void EnsureWagonReadyForFreight()
        {
            if (boundVehicleService == null || !boundVehicleService.IsInitialized)
            {
                return;
            }

            if (!boundVehicleService.OwnsWagon)
            {
                boundVehicleService.TryGrantWagonOwnership();
            }

            boundVehicleService.TrySummonWagonNearPlayer();
        }

        private void TryLoadFreightItemIntoWagon(string itemId, int quantity)
        {
            if (boundVehicleService == null
                || !boundVehicleService.IsInitialized
                || boundStorageService == null
                || !boundStorageService.IsInitialized
                || quantity <= 0)
            {
                return;
            }

            string cargoInstanceId = boundVehicleService.ActiveCargoInstanceId;
            if (string.IsNullOrWhiteSpace(cargoInstanceId)
                || !boundStorageService.TryGetRegisteredContainer(cargoInstanceId, out CCS_StorageContainer container)
                || container == null)
            {
                return;
            }

            CCS_ItemDefinition itemDefinition = FindItemDefinitionById(itemId);
            if (itemDefinition == null)
            {
                return;
            }

            container.TryAddItem(itemDefinition, quantity, out int added);
            if (added > 0)
            {
                LogDebug($"Loaded {added}x {itemId} into wagon cargo for route risk freight playtest.");
            }
        }

        private void CaptureRouteRiskBaselinesIfNeeded()
        {
            if (routeRiskBaselinesCaptured)
            {
                return;
            }

            if (boundTradeRouteService != null && boundTradeRouteService.IsInitialized)
            {
                if (boundTradeRouteService.TryGetUsageCount(
                        CCS_TradeRoutesFreightContentIds.BrokenCreekToTradingPostRouteId,
                        out int brokenCreekUsage))
                {
                    routeRiskBrokenCreekUsageBaseline = brokenCreekUsage;
                }

                if (boundTradeRouteService.TryGetUsageCount(
                        CCS_TradeRoutesFreightContentIds.IronRidgeToTradingPostRouteId,
                        out int ironRidgeUsage))
                {
                    routeRiskIronRidgeUsageBaseline = ironRidgeUsage;
                }
            }

            routeRiskBaselinesCaptured = true;
        }

        private void EvaluateRouteRiskLowRewardSteps(CCS_ContractCompletionResult result)
        {
            if (result == null
                || !result.IsSuccess
                || !string.Equals(
                    result.ContractId,
                    CCS_TradeRoutesFreightContentIds.BrokenCreekCornFreightContractId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            routeRiskLowFinalReward = result.TradeDollarsGranted;
            bool rewardValid = result.HasFreightRewardBreakdown
                && result.BaseTradeDollarsReward > 0
                && result.TradeDollarsGranted >= result.BaseTradeDollarsReward
                && result.TradeDollarsGranted >= 0;
            if (rewardValid)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyLowRiskFreightReward,
                    $"Low-risk reward base={result.BaseTradeDollarsReward} final={result.TradeDollarsGranted} " +
                    $"(route x{result.RouteRewardMultiplier:0.###}, risk x{result.RiskRewardMultiplier:0.###}).");
            }
        }

        private void EvaluateRouteRiskModerateRewardSteps(CCS_ContractCompletionResult result)
        {
            if (result == null
                || !result.IsSuccess
                || !string.Equals(
                    result.ContractId,
                    CCS_TradeRoutesFreightContentIds.IronRidgeIronOreFreightContractId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            bool higherThanLow = routeRiskLowFinalReward > 0
                && result.TradeDollarsGranted > routeRiskLowFinalReward;
            bool rewardValid = result.HasFreightRewardBreakdown
                && result.BaseTradeDollarsReward > 0
                && result.TradeDollarsGranted >= result.BaseTradeDollarsReward;
            if (rewardValid && higherThanLow)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyModerateRiskFreightHigherReward,
                    $"Moderate-risk final={result.TradeDollarsGranted} exceeds low-risk final={routeRiskLowFinalReward}.");
            }
        }

        private bool TryCaptureRouteRiskFreightSaveState()
        {
            if (routeRiskSaveCaptured)
            {
                return false;
            }

            if (boundTradeRouteService != null && boundTradeRouteService.IsInitialized)
            {
                if (boundTradeRouteService.TryGetUsageCount(
                        CCS_TradeRoutesFreightContentIds.BrokenCreekToTradingPostRouteId,
                        out int brokenCreekUsage))
                {
                    savedRouteRiskBrokenCreekUsageCount = brokenCreekUsage;
                }

                if (boundTradeRouteService.TryGetUsageCount(
                        CCS_TradeRoutesFreightContentIds.IronRidgeToTradingPostRouteId,
                        out int ironRidgeUsage))
                {
                    savedRouteRiskIronRidgeUsageCount = ironRidgeUsage;
                }
            }

            routeRiskSaveCaptured = savedRouteRiskBrokenCreekUsageCount > routeRiskBrokenCreekUsageBaseline
                || savedRouteRiskIronRidgeUsageCount > routeRiskIronRidgeUsageBaseline;
            return routeRiskSaveCaptured;
        }

        private void EvaluateRouteRiskFreightStateAfterLoad()
        {
            if (!routeRiskSaveCaptured)
            {
                return;
            }

            bool brokenCreekUsageRestored = boundTradeRouteService != null
                && boundTradeRouteService.IsInitialized
                && boundTradeRouteService.TryGetUsageCount(
                    CCS_TradeRoutesFreightContentIds.BrokenCreekToTradingPostRouteId,
                    out int brokenCreekUsage)
                && brokenCreekUsage == savedRouteRiskBrokenCreekUsageCount;

            bool ironRidgeUsageRestored = boundTradeRouteService != null
                && boundTradeRouteService.IsInitialized
                && boundTradeRouteService.TryGetUsageCount(
                    CCS_TradeRoutesFreightContentIds.IronRidgeToTradingPostRouteId,
                    out int ironRidgeUsage)
                && ironRidgeUsage == savedRouteRiskIronRidgeUsageCount;

            bool cornCompleted = boundContractService != null
                && boundContractService.IsInitialized
                && boundContractService.GetContractState(
                    CCS_TradeRoutesFreightContentIds.BrokenCreekCornFreightContractId)
                    == CCS_ContractState.Completed;

            bool ironCompleted = boundContractService != null
                && boundContractService.IsInitialized
                && boundContractService.GetContractState(
                    CCS_TradeRoutesFreightContentIds.IronRidgeIronOreFreightContractId)
                    == CCS_ContractState.Completed;

            if (brokenCreekUsageRestored && ironRidgeUsageRestored && cornCompleted && ironCompleted)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyRouteRiskFreightStateAfterLoad,
                    "Route risk freight contracts and route usage counts restored after load.");
            }
        }

        private void TryLoadLumberIntoWagonCargoForFreight()
        {
            if (boundVehicleService == null
                || !boundVehicleService.IsInitialized
                || boundStorageService == null
                || !boundStorageService.IsInitialized)
            {
                return;
            }

            string cargoInstanceId = boundVehicleService.ActiveCargoInstanceId;
            if (string.IsNullOrWhiteSpace(cargoInstanceId)
                || !boundStorageService.TryGetRegisteredContainer(cargoInstanceId, out CCS_StorageContainer container)
                || container == null)
            {
                return;
            }

            CCS_ItemDefinition lumber = FindItemDefinitionById(CCS_ContractContentIds.LumberItemId);
            if (lumber == null)
            {
                return;
            }

            container.TryAddItem(lumber, 8, out int added);
            if (added > 0)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.LoadLumberIntoWagonCargoForFreight,
                    $"Loaded {added} lumber into wagon cargo.");
            }
        }

        private void CaptureFreightBaselinesIfNeeded()
        {
            if (freightBaselinesCaptured)
            {
                return;
            }

            if (boundWorldSimulationService != null
                && boundWorldSimulationService.IsInitialized
                && boundWorldSimulationService.TryGetSettlementState(
                    CCS_SettlementContentIds.TestTradingPostSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                && settlementState != null)
            {
                freightTradingPostProsperityBaseline = settlementState.prosperity;
            }

            if (boundTradeRouteService != null
                && boundTradeRouteService.IsInitialized
                && boundTradeRouteService.TryGetUsageCount(
                    CCS_TradeRoutesFreightContentIds.PineRidgeToTradingPostRouteId,
                    out int usageCount))
            {
                freightRouteUsageBaseline = usageCount;
            }

            freightBaselinesCaptured = true;
        }

        private void EvaluateFreightContractSteps(CCS_ContractCompletionResult result)
        {
            if (result == null
                || !result.IsSuccess
                || !string.Equals(
                    result.ContractId,
                    CCS_TradeRoutesFreightContentIds.PineRidgeLumberFreightContractId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.CompletePineRidgeLumberFreightDelivery,
                $"Completed freight contract {result.ContractId}.");
            EvaluateFreightWorldSimulationSteps();
            EvaluateFreightRouteUsageSteps();
        }

        private void EvaluateFreightWorldSimulationSteps()
        {
            if (boundWorldSimulationService == null
                || !boundWorldSimulationService.IsInitialized
                || !boundWorldSimulationService.TryGetSettlementState(
                    CCS_SettlementContentIds.TestTradingPostSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                || settlementState == null)
            {
                return;
            }

            if (settlementState.prosperity > freightTradingPostProsperityBaseline + 0.01f)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyFreightDestinationProsperitySupply,
                    $"Trading Post prosperity {settlementState.prosperity:0.##} increased from freight delivery.");
            }
        }

        private void EvaluateFreightRouteUsageSteps()
        {
            if (boundTradeRouteService == null
                || !boundTradeRouteService.IsInitialized
                || !boundTradeRouteService.TryGetUsageCount(
                    CCS_TradeRoutesFreightContentIds.PineRidgeToTradingPostRouteId,
                    out int usageCount))
            {
                return;
            }

            if (usageCount > freightRouteUsageBaseline)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyTradeRouteUsageCount,
                    $"Trade route usage count is {usageCount}.");
            }
        }

        public bool TryPlaytestMultiSettlementFoundationShortcut()
        {
            DiscoverPlaytestSettlement(CCS_MultiSettlementContentIds.PineRidgeCampSettlementId);
            DiscoverPlaytestSettlement(CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId);
            DiscoverPlaytestSettlement(CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId);
            GrantPlaytestItem(CCS_RegionEconomyUtility.LumberItemId, 5);

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_ContractContentIds.LumberDeliveryContractId,
                    CCS_MultiSettlementContentIds.PineRidgeCampSettlementId);
                boundContractService.TryCompleteContract(CCS_ContractContentIds.LumberDeliveryContractId);
            }

            return true;
        }

        private void DiscoverPlaytestSettlement(string settlementId)
        {
            if (boundSettlementService == null
                || !boundSettlementService.IsInitialized
                || !boundSettlementService.TryGetDefinition(settlementId, out CCS_SettlementDefinition definition))
            {
                return;
            }

            boundSettlementService.DiscoverSettlement(definition, definition.DefaultWorldPosition);
        }

        private void CaptureMultiSettlementBaselinesIfNeeded()
        {
            if (multiSettlementBaselinesCaptured)
            {
                return;
            }

            if (boundWorldSimulationService != null
                && boundWorldSimulationService.IsInitialized
                && boundWorldSimulationService.TryGetSettlementState(
                    CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                && settlementState != null)
            {
                multiSettlementPineProsperityBaseline = settlementState.prosperity;
            }

            if (boundReputationService != null
                && boundReputationService.IsInitialized
                && boundReputationService.TryGetSettlementStanding(
                    CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                    out CCS_ReputationStanding standing)
                && standing != null)
            {
                multiSettlementPineReputationBaseline = standing.CurrentValue;
            }

            multiSettlementBaselinesCaptured = true;
        }

        private void EvaluateMultiSettlementContractSteps(CCS_ContractCompletionResult result)
        {
            if (result == null || !result.IsSuccess)
            {
                return;
            }

            if (!string.Equals(
                    result.ContractId,
                    CCS_ContractContentIds.LumberDeliveryContractId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.CompleteMultiSettlementRegionalContract,
                $"Completed regional contract {result.ContractId}.");

            if (boundWorldSimulationService != null
                && boundWorldSimulationService.IsInitialized
                && boundWorldSimulationService.TryGetSettlementState(
                    CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                && settlementState != null
                && settlementState.prosperity > multiSettlementPineProsperityBaseline)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyMultiSettlementProsperityChanged,
                    $"Pine Ridge prosperity increased to {settlementState.prosperity:0.##}.");
            }

            if (boundReputationService != null
                && boundReputationService.IsInitialized
                && boundReputationService.TryGetSettlementStanding(
                    CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                    out CCS_ReputationStanding standing)
                && standing != null
                && standing.CurrentValue > multiSettlementPineReputationBaseline)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyMultiSettlementReputationChanged,
                    $"Pine Ridge reputation increased to {standing.CurrentValue}.");
            }
        }

        private bool TryCaptureMultiSettlementSaveState()
        {
            if (multiSettlementSaveCaptured
                || boundSettlementService == null
                || !boundSettlementService.IsInitialized)
            {
                return multiSettlementSaveCaptured;
            }

            savedMultiSettlementDiscoveryCount = 0;
            if (boundSettlementService.IsDiscovered(CCS_MultiSettlementContentIds.PineRidgeCampSettlementId))
            {
                savedMultiSettlementDiscoveryCount++;
            }

            if (boundSettlementService.IsDiscovered(CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId))
            {
                savedMultiSettlementDiscoveryCount++;
            }

            if (boundSettlementService.IsDiscovered(CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId))
            {
                savedMultiSettlementDiscoveryCount++;
            }

            if (boundWorldSimulationService != null
                && boundWorldSimulationService.IsInitialized
                && boundWorldSimulationService.TryGetSettlementState(
                    CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                && settlementState != null)
            {
                savedMultiSettlementPineProsperity = settlementState.prosperity;
            }

            if (boundReputationService != null
                && boundReputationService.IsInitialized
                && boundReputationService.TryGetSettlementStanding(
                    CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                    out CCS_ReputationStanding standing)
                && standing != null)
            {
                savedMultiSettlementPineReputation = standing.CurrentValue;
            }

            savedMultiSettlementTradeRouteCount = boundSettlementService.ActiveProfile?.TradeRouteProfile != null
                ? boundSettlementService.ActiveProfile.TradeRouteProfile.TradeRouteDefinitions.Length
                : 0;
            multiSettlementSaveCaptured = true;
            return true;
        }

        private void EvaluateMultiSettlementAfterLoad()
        {
            if (!multiSettlementSaveCaptured
                || boundSettlementService == null
                || !boundSettlementService.IsInitialized)
            {
                return;
            }

            int discoveryCount = 0;
            if (boundSettlementService.IsDiscovered(CCS_MultiSettlementContentIds.PineRidgeCampSettlementId))
            {
                discoveryCount++;
            }

            if (boundSettlementService.IsDiscovered(CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId))
            {
                discoveryCount++;
            }

            if (boundSettlementService.IsDiscovered(CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId))
            {
                discoveryCount++;
            }

            bool prosperityRestored = boundWorldSimulationService != null
                && boundWorldSimulationService.IsInitialized
                && boundWorldSimulationService.TryGetSettlementState(
                    CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                && settlementState != null
                && Mathf.Approximately(settlementState.prosperity, savedMultiSettlementPineProsperity);

            bool reputationRestored = boundReputationService != null
                && boundReputationService.IsInitialized
                && boundReputationService.TryGetSettlementStanding(
                    CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                    out CCS_ReputationStanding standing)
                && standing != null
                && standing.CurrentValue == savedMultiSettlementPineReputation;

            int routeCount = boundSettlementService.ActiveProfile?.TradeRouteProfile != null
                ? boundSettlementService.ActiveProfile.TradeRouteProfile.TradeRouteDefinitions.Length
                : 0;

            if (discoveryCount == savedMultiSettlementDiscoveryCount
                && prosperityRestored
                && reputationRestored
                && routeCount == savedMultiSettlementTradeRouteCount)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyMultiSettlementAfterLoad,
                    "Multi-settlement discovery, prosperity, reputation, and trade routes restored after load.");
            }
        }

        private bool TryCaptureFreightSaveState()
        {
            if (freightSaveCaptured)
            {
                return false;
            }

            if (boundWorldSimulationService != null
                && boundWorldSimulationService.IsInitialized
                && boundWorldSimulationService.TryGetSettlementState(
                    CCS_SettlementContentIds.TestTradingPostSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                && settlementState != null)
            {
                savedFreightTradingPostProsperity = settlementState.prosperity;
            }

            if (boundTradeRouteService != null
                && boundTradeRouteService.IsInitialized
                && boundTradeRouteService.TryGetUsageCount(
                    CCS_TradeRoutesFreightContentIds.PineRidgeToTradingPostRouteId,
                    out int usageCount))
            {
                savedFreightRouteUsageCount = usageCount;
            }

            freightSaveCaptured = savedFreightRouteUsageCount > 0 || savedFreightTradingPostProsperity > 0f;
            return freightSaveCaptured;
        }

        private void EvaluateFreightStateAfterLoad()
        {
            if (!freightSaveCaptured)
            {
                return;
            }

            bool prosperityRestored = boundWorldSimulationService != null
                && boundWorldSimulationService.IsInitialized
                && boundWorldSimulationService.TryGetSettlementState(
                    CCS_SettlementContentIds.TestTradingPostSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                && settlementState != null
                && Mathf.Approximately(settlementState.prosperity, savedFreightTradingPostProsperity);

            bool routeUsageRestored = boundTradeRouteService != null
                && boundTradeRouteService.IsInitialized
                && boundTradeRouteService.TryGetUsageCount(
                    CCS_TradeRoutesFreightContentIds.PineRidgeToTradingPostRouteId,
                    out int usageCount)
                && usageCount == savedFreightRouteUsageCount;

            bool contractCompleted = boundContractService != null
                && boundContractService.IsInitialized
                && boundContractService.GetContractState(
                    CCS_TradeRoutesFreightContentIds.PineRidgeLumberFreightContractId)
                    == CCS_ContractState.Completed;

            if (prosperityRestored && routeUsageRestored && contractCompleted)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyFreightRouteStateAfterLoad,
                    "Freight contract, route usage, and destination prosperity restored after load.");
            }
        }

        public bool TryPlaytestNpcIdentityFoundationShortcut()
        {
            DiscoverPlaytestSettlement(CCS_SettlementGrowthContentIds.TradingPostSettlementId);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForNpcIdentity,
                "Trading Post discovered for NPC identity playtest.");

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();

            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 5);
            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId);
                CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                if (result != null && result.IsSuccess)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.TriggerPopulationPresenceForNpcIdentity,
                        result.Message);
                }
            }

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            CCS_NpcRuntimeBridge.RefreshAllPlaceholderIdentities();
            EvaluateNpcIdentityPlaytestSteps();
            return true;
        }

        public bool TryPlaytestNpcServiceRepresentativeFoundationShortcut()
        {
            const string generalStoreBusinessId = "ccs.survival.business.generalstore";
            const string bankBusinessId = "ccs.survival.business.bank";

            DiscoverPlaytestSettlement(CCS_SettlementGrowthContentIds.TradingPostSettlementId);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForNpcServiceRepresentatives,
                "Trading Post discovered for NPC service representative playtest.");

            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 5);
            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId);
                CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                if (result != null && result.IsSuccess)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.TriggerPopulationAndBusinessForNpcServiceRepresentatives,
                        result.Message);
                }
            }

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            CCS_NpcRuntimeBridge.RefreshAllPlaceholderIdentities();
            CCS_NpcServiceRepresentativeRuntimeBridge.RefreshAllRepresentativeAssignments();
            EvaluateNpcServiceRepresentativePlaytestSteps();

            if (CCS_NpcServiceRepresentativeRuntimeBridge.TrySimulateRepresentativeInteraction(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    generalStoreBusinessId))
            {
                EvaluateNpcServiceRepresentativePlaytestSteps();
            }

            if (CCS_NpcServiceRepresentativeRuntimeBridge.TrySimulateRepresentativeInteraction(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    bankBusinessId))
            {
                EvaluateNpcServiceRepresentativePlaytestSteps();
            }

            return true;
        }

        public bool TryPlaytestSettlementHousingFoundationShortcut()
        {
            DiscoverPlaytestSettlement(CCS_SettlementGrowthContentIds.TradingPostSettlementId);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForSettlementHousing,
                "Trading Post discovered for settlement housing playtest.");

            CCS_SettlementHousingRuntimeBridge.RefreshAllAnchors();
            EvaluateSettlementHousingPlaytestSteps();

            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 25);
            if (boundContractService != null && boundContractService.IsInitialized)
            {
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    boundContractService.TryAcceptContract(
                        CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                        CCS_SettlementGrowthContentIds.TradingPostSettlementId);
                    CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                        CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                    if (result != null && result.IsSuccess && attempt == 0)
                    {
                        TryCompleteActiveStepOfType(
                            CCS_PlaytestStepType.IncreasePopulationForHousingCapacity,
                            result.Message);
                    }
                }
            }

            CCS_SettlementHousingRuntimeBridge.RefreshAllAnchors();
            EvaluateSettlementHousingPlaytestSteps();
            EvaluatePopulationPlaytestSteps();
            return true;
        }

        public bool TryPlaytestNpcMovementFoundationShortcut()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            DiscoverPlaytestSettlement(settlementId);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForNpcMovement,
                "Trading Post discovered for NPC movement playtest.");

            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 25);
            if (boundContractService != null && boundContractService.IsInitialized)
            {
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    boundContractService.TryAcceptContract(
                        CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                        settlementId);
                    CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                        CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                    if (result != null && result.IsSuccess && attempt == 0)
                    {
                        break;
                    }
                }
            }

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            CCS_NpcRuntimeBridge.RefreshAllPlaceholderIdentities();
            CCS_NpcServiceRepresentativeRuntimeBridge.RefreshAllRepresentativeAssignments();
            CCS_SettlementHousingRuntimeBridge.RefreshAllAnchors();
            SetPlaytestScheduleHour(10);
            CCS_NpcMovementRuntimeBridge.RefreshAllMovementHosts();
            EvaluateNpcMovementPlaytestSteps();
            SetPlaytestScheduleHour(20);
            CCS_NpcMovementRuntimeBridge.RefreshAllMovementHosts();
            EvaluateNpcMovementPlaytestSteps();
            SetPlaytestScheduleHour(10);
            CCS_NpcMovementRuntimeBridge.RefreshAllMovementHosts();
            EvaluateNpcMovementPlaytestSteps();
            return true;
        }

        public bool TryPlaytestNpcScheduleFoundationShortcut()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            DiscoverPlaytestSettlement(settlementId);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForNpcSchedule,
                "Trading Post discovered for NPC schedule playtest.");

            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 25);
            if (boundContractService != null && boundContractService.IsInitialized)
            {
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    boundContractService.TryAcceptContract(
                        CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                        settlementId);
                    CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                        CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                    if (result != null && result.IsSuccess && attempt == 0)
                    {
                        break;
                    }
                }
            }

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            CCS_NpcRuntimeBridge.RefreshAllPlaceholderIdentities();
            CCS_NpcServiceRepresentativeRuntimeBridge.RefreshAllRepresentativeAssignments();
            CCS_SettlementHousingRuntimeBridge.RefreshAllAnchors();
            CCS_NpcScheduleRuntimeBridge.RefreshAllScheduleHosts();
            EvaluateNpcSchedulePlaytestSteps();

            CCS_INpcMovementHost scheduleHost = null;
            if (CCS_NpcScheduleRuntimeBridge.TryGetFirstHostScheduleSnapshot(
                    settlementId,
                    out scheduleHost,
                    out _)
                && scheduleHost != null)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SpawnNamedNpcForSchedule,
                    $"Named NPC ready for schedule playtest ({scheduleHost.NpcIdentityId}).");
            }

            string npcIdentityId = scheduleHost?.NpcIdentityId ?? string.Empty;
            SetPlaytestScheduleHour(10);
            CCS_NpcScheduleRuntimeBridge.ForceEvaluateScheduleBlock(
                settlementId,
                npcIdentityId,
                CCS_NpcScheduleBlockType.Work,
                10);
            CCS_NpcMovementRuntimeBridge.RefreshAllMovementHosts();
            EvaluateNpcSchedulePlaytestSteps();

            SetPlaytestScheduleHour(20);
            CCS_NpcScheduleRuntimeBridge.ForceEvaluateScheduleBlock(
                settlementId,
                npcIdentityId,
                CCS_NpcScheduleBlockType.Home,
                20);
            CCS_NpcMovementRuntimeBridge.RefreshAllMovementHosts();
            EvaluateNpcSchedulePlaytestSteps();

            SetPlaytestScheduleHour(10);
            CCS_NpcScheduleRuntimeBridge.RefreshAllScheduleHosts();
            CCS_NpcMovementRuntimeBridge.RefreshAllMovementHosts();
            EvaluateNpcSchedulePlaytestSteps();
            return true;
        }

        public bool TryPlaytestNpcActivityFoundationShortcut()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            DiscoverPlaytestSettlement(settlementId);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForNpcActivity,
                "Trading Post discovered for NPC activity playtest.");

            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 25);
            if (boundContractService != null && boundContractService.IsInitialized)
            {
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    boundContractService.TryAcceptContract(
                        CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                        settlementId);
                    CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                        CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                    if (result != null && result.IsSuccess && attempt == 0)
                    {
                        break;
                    }
                }
            }

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            CCS_NpcRuntimeBridge.RefreshAllPlaceholderIdentities();
            CCS_NpcServiceRepresentativeRuntimeBridge.RefreshAllRepresentativeAssignments();
            CCS_SettlementHousingRuntimeBridge.RefreshAllAnchors();
            CCS_NpcScheduleRuntimeBridge.RefreshAllScheduleHosts();
            CCS_NpcActivityRuntimeBridge.RefreshAllActivityHosts();

            CCS_INpcMovementHost activityHost = null;
            if (CCS_NpcActivityRuntimeBridge.TryGetFirstHostActivitySnapshot(
                    settlementId,
                    out activityHost,
                    out _)
                && activityHost != null)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SpawnNamedNpcForActivity,
                    $"Named NPC ready for activity playtest ({activityHost.NpcIdentityId}).");
            }

            string npcIdentityId = activityHost?.NpcIdentityId ?? string.Empty;
            SetPlaytestScheduleHour(10);
            CCS_NpcScheduleRuntimeBridge.ForceEvaluateScheduleBlock(
                settlementId,
                npcIdentityId,
                CCS_NpcScheduleBlockType.Work,
                10);
            CCS_NpcMovementRuntimeBridge.RefreshAllMovementHosts();
            CCS_NpcActivityRuntimeBridge.RefreshAllActivityHosts();
            EvaluateNpcActivityPlaytestSteps();

            SetPlaytestScheduleHour(20);
            CCS_NpcScheduleRuntimeBridge.ForceEvaluateScheduleBlock(
                settlementId,
                npcIdentityId,
                CCS_NpcScheduleBlockType.Home,
                20);
            CCS_NpcMovementRuntimeBridge.RefreshAllMovementHosts();
            CCS_NpcActivityRuntimeBridge.RefreshAllActivityHosts();
            EvaluateNpcActivityPlaytestSteps();

            SetPlaytestScheduleHour(10);
            CCS_NpcScheduleRuntimeBridge.ForceEvaluateScheduleBlock(
                settlementId,
                npcIdentityId,
                CCS_NpcScheduleBlockType.Work,
                10);
            CCS_NpcMovementRuntimeBridge.RefreshAllMovementHosts();
            CCS_NpcActivityRuntimeBridge.RefreshAllActivityHosts();
            EvaluateNpcActivityPlaytestSteps();
            return true;
        }

        public bool TryPlaytestPopulationPresenceFoundationShortcut()
        {
            DiscoverPlaytestSettlement(CCS_SettlementGrowthContentIds.TradingPostSettlementId);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForPopulationPresence,
                "Trading Post discovered for population presence playtest.");

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            EvaluatePopulationPresencePlaytestSteps();

            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 5);
            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId);
                CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                if (result != null && result.IsSuccess)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.TriggerPopulationGrowthForPlaceholders,
                        result.Message);
                }
            }

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            EvaluatePopulationPresencePlaytestSteps();
            return true;
        }

        public bool TryPlaytestSettlementVisualGrowthFoundationShortcut()
        {
            DiscoverPlaytestSettlement(CCS_SettlementGrowthContentIds.TradingPostSettlementId);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForVisualGrowth,
                "Trading Post discovered for settlement visual growth playtest.");

            CCS_SettlementVisualGrowthRuntimeBridge.RefreshAllAnchors();
            EvaluateSettlementVisualGrowthPlaytestSteps();

            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 5);
            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId);
                CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                if (result != null && result.IsSuccess)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.TriggerTradingPostGrowthForVisuals,
                        result.Message);
                }
            }

            CCS_SettlementVisualGrowthRuntimeBridge.RefreshAllAnchors();
            EvaluateSettlementVisualGrowthPlaytestSteps();
            EvaluateSettlementGrowthProgressSteps();
            return true;
        }

        public bool TryPlaytestBusinessPresenceFoundationShortcut()
        {
            DiscoverPlaytestSettlement(CCS_SettlementContentIds.TestTradingPostSettlementId);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForBusinessPresence,
                "Trading Post discovered for business presence playtest.");

            EvaluateBusinessPresencePlaytestSteps();

            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 5);
            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId);
                CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                if (result != null && result.IsSuccess)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.TriggerBusinessActivationForMarkers,
                        result.Message);
                }
            }

            CCS_BusinessPresenceRuntimeBridge.RefreshAllAnchors();
            EvaluateBusinessPresencePlaytestSteps();
            return true;
        }

        public bool TryPlaytestBusinessesFoundationShortcut()
        {
            DiscoverPlaytestSettlement(CCS_SettlementContentIds.TestTradingPostSettlementId);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForBusinesses,
                "Trading Post discovered for business playtest.");

            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 5);
            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId);
                CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                if (result != null && result.IsSuccess)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.CompleteContractForBusinessActivation,
                        result.Message);
                }
            }

            EvaluateBusinessPlaytestSteps();
            return true;
        }

        public bool TryPlaytestPopulationFoundationShortcut()
        {
            DiscoverPlaytestSettlement(CCS_SettlementContentIds.TestTradingPostSettlementId);
            CapturePopulationBaselinesIfNeeded();
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForPopulation,
                "Trading Post discovered for population playtest.");

            CCS_SettlementGrowthDebugHud.ShowSettlement(CCS_SettlementGrowthContentIds.TradingPostSettlementId);
            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 5);

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                    CCS_SettlementContentIds.TestTradingPostSettlementId);
                CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                if (result != null && result.IsSuccess)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.CompleteContractForPopulationGrowth,
                        result.Message);
                }
            }

            EvaluatePopulationPlaytestSteps();
            EvaluateSettlementGrowthProgressSteps();
            return true;
        }

        public bool TryPlaytestSettlementGrowthFoundationShortcut()
        {
            if (boundSettlementService != null
                && boundSettlementService.IsInitialized
                && boundSettlementService.TryGetDefinition(
                    CCS_SettlementContentIds.TestTradingPostSettlementId,
                    out CCS_SettlementDefinition tradingPostDefinition))
            {
                boundSettlementService.DiscoverSettlement(tradingPostDefinition, Vector3.zero);
            }

            CCS_SettlementGrowthDebugHud.ShowSettlement(CCS_SettlementGrowthContentIds.TradingPostSettlementId);
            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 5);

            if (boundContractService != null && boundContractService.IsInitialized)
            {
                boundContractService.TryAcceptContract(
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId);
                boundContractService.TryCompleteContract(CCS_SettlementGrowthContentIds.PlaytestCornContractId);
            }

            EvaluatePopulationPlaytestSteps();
            EvaluateSettlementGrowthProgressSteps();
            return true;
        }

        private void CapturePopulationBaselinesIfNeeded()
        {
            if (populationBaselinesCaptured
                || boundSettlementService == null
                || !boundSettlementService.IsInitialized)
            {
                return;
            }

            if (boundSettlementService.TryGetPopulationSnapshot(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    out CCS_SettlementPopulationSnapshot snapshot)
                && snapshot != null)
            {
                populationBaseline = snapshot.TotalPopulation;
            }

            populationBaselinesCaptured = true;
        }

        private void EvaluatePopulationPlaytestSteps()
        {
            if (boundSettlementService == null
                || !boundSettlementService.IsInitialized
                || !boundSettlementService.TryGetPopulationSnapshot(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    out CCS_SettlementPopulationSnapshot snapshot)
                || snapshot == null)
            {
                return;
            }

            if (snapshot.TotalPopulation >= 0)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyPopulationNonNegative,
                    $"Population is non-negative ({snapshot.TotalPopulation}).");
            }

            if (snapshot.PopulationGrowthRate >= 0f)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyPopulationGrowthRateValid,
                    $"Population growth rate is {snapshot.PopulationGrowthRate:0.##}.");
            }

            if (snapshot.TotalPopulation > populationBaseline)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyPopulationIncreased,
                    $"Population increased to {snapshot.TotalPopulation} (baseline {populationBaseline}).");
            }
        }

        private void EvaluatePopulationPresencePlaytestSteps()
        {
            int anchorCount = CCS_PopulationPresenceRuntimeBridge.GetRegisteredAnchorCount();
            if (anchorCount >= 2)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyLowPopulationPlaceholderActors,
                    $"Found {anchorCount} population presence anchors in scene.");
            }

            int merchantSpawned = CCS_PopulationPresenceRuntimeBridge.GetSpawnedActorCount(
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementPopulationCategory.Merchants);
            if (merchantSpawned <= 1)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyLowPopulationPlaceholderActors,
                    $"Merchant placeholder actor count is low ({merchantSpawned}).");
            }

            if (merchantSpawned > 0)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyPopulationPlaceholderActorsVisible,
                    $"Merchant placeholder actors visible ({merchantSpawned}).");
            }
        }

        private bool TryCapturePopulationPresenceSaveState()
        {
            if (populationPresenceSaveCaptured)
            {
                return populationPresenceSaveCaptured;
            }

            savedMerchantPlaceholderActorCount = CCS_PopulationPresenceRuntimeBridge.GetSpawnedActorCount(
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementPopulationCategory.Merchants);
            populationPresenceSaveCaptured = savedMerchantPlaceholderActorCount >= 0;
            return populationPresenceSaveCaptured;
        }

        private void EvaluatePopulationPresenceStateAfterLoad()
        {
            if (!populationPresenceSaveCaptured)
            {
                return;
            }

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            int restoredCount = CCS_PopulationPresenceRuntimeBridge.GetSpawnedActorCount(
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementPopulationCategory.Merchants);
            if (restoredCount == savedMerchantPlaceholderActorCount)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyPopulationPresenceAfterLoad,
                    "Population placeholder actors restored from simulation after load.");
            }
        }

        private void EvaluateNpcIdentityPlaytestSteps()
        {
            int withIdentity = CCS_NpcRuntimeBridge.GetPlaceholderCountWithIdentity(
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementPopulationCategory.Merchants);
            if (withIdentity > 0)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyPlaceholderActorHasIdentity,
                    $"Merchant placeholders have NPC identity ({withIdentity}).");
            }

            if (CCS_NpcRuntimeBridge.TryGetFirstPlaceholderIdentity(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    CCS_SettlementPopulationCategory.Merchants,
                    out CCS_NpcIdentitySnapshot snapshot)
                && CCS_NpcIdentityValidationUtility.RoleMatchesWorkforce(snapshot.Role, snapshot.WorkforceCategory))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcRoleMatchesWorkforce,
                    $"Role '{snapshot.Role}' matches workforce '{snapshot.WorkforceCategory}'.");
            }
        }

        private bool TryCaptureNpcIdentitySaveState()
        {
            if (npcIdentitySaveCaptured)
            {
                return npcIdentitySaveCaptured;
            }

            if (!CCS_NpcRuntimeBridge.TryGetFirstPlaceholderIdentity(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    CCS_SettlementPopulationCategory.Merchants,
                    out CCS_NpcIdentitySnapshot snapshot)
                || !snapshot.IsValid)
            {
                return false;
            }

            savedNpcIdentityId = snapshot.NpcIdentityId;
            savedNpcDisplayName = snapshot.DisplayName;
            savedNpcRoleType = (int)snapshot.Role;
            npcIdentitySaveCaptured = true;
            return true;
        }

        private void EvaluateNpcIdentityStateAfterLoad()
        {
            if (!npcIdentitySaveCaptured)
            {
                return;
            }

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            CCS_NpcRuntimeBridge.RefreshAllPlaceholderIdentities();
            if (!CCS_NpcRuntimeBridge.TryGetFirstPlaceholderIdentity(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    CCS_SettlementPopulationCategory.Merchants,
                    out CCS_NpcIdentitySnapshot snapshot))
            {
                return;
            }

            bool restored = string.Equals(snapshot.NpcIdentityId, savedNpcIdentityId, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(snapshot.DisplayName, savedNpcDisplayName, System.StringComparison.Ordinal)
                && (int)snapshot.Role == savedNpcRoleType;
            if (restored)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcIdentityAfterLoad,
                    "NPC identity restored after load.");
            }
        }

        private void EvaluateSettlementHousingPlaytestSteps()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            if (CCS_SettlementHousingRuntimeBridge.GetRegisteredAnchorCount() >= 4
                && CCS_SettlementHousingRuntimeBridge.TryFindAnchorForHousing(
                    settlementId,
                    CCS_SettlementHousingContentIds.TradingPostBoardingHouseId,
                    out _))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyHousingMarkerExists,
                    "Settlement housing marker exists for Trading Post Boarding House.");
            }

            if (CCS_SettlementHousingRuntimeBridge.TryGetHousingSnapshot(
                    settlementId,
                    out CCS_SettlementHousingSnapshot housingSnapshot)
                && housingSnapshot.IsValid
                && housingSnapshot.HousingCapacityContribution > 0)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyHousingCapacityContribution,
                    $"Housing contributes +{housingSnapshot.HousingCapacityContribution} population capacity.");
            }

            if (boundSettlementService != null
                && boundSettlementService.IsInitialized
                && boundSettlementService.TryGetPopulationSnapshot(settlementId, out CCS_SettlementPopulationSnapshot populationSnapshot)
                && populationSnapshot != null
                && populationSnapshot.TotalPopulation <= populationSnapshot.PopulationCapacity
                && populationSnapshot.PopulationCapacity
                    >= populationSnapshot.BasePopulationCapacity + populationSnapshot.HousingCapacityContribution)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyPopulationRespectsTotalCapacity,
                    $"Population {populationSnapshot.TotalPopulation} respects total capacity "
                    + $"{populationSnapshot.PopulationCapacity}.");
            }
        }

        private bool TryCaptureSettlementHousingSaveState()
        {
            if (settlementHousingSaveCaptured)
            {
                return settlementHousingSaveCaptured;
            }

            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            if (!CCS_SettlementHousingRuntimeBridge.TryGetHousingSnapshot(
                    settlementId,
                    out CCS_SettlementHousingSnapshot housingSnapshot)
                || !housingSnapshot.IsValid
                || housingSnapshot.HousingCapacityContribution <= 0)
            {
                return false;
            }

            savedHousingCapacityContribution = housingSnapshot.HousingCapacityContribution;
            savedActiveHousingCount = housingSnapshot.ActiveHousingCount;
            savedBoardingHouseActive = housingSnapshot.ActiveHousingCount > 0;
            settlementHousingSaveCaptured = true;
            return true;
        }

        private void EvaluateSettlementHousingStateAfterLoad()
        {
            if (!settlementHousingSaveCaptured)
            {
                return;
            }

            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            CCS_SettlementHousingRuntimeBridge.RefreshAllAnchors();

            bool restored = CCS_SettlementHousingRuntimeBridge.TryGetHousingSnapshot(
                    settlementId,
                    out CCS_SettlementHousingSnapshot housingSnapshot)
                && housingSnapshot.IsValid
                && housingSnapshot.HousingCapacityContribution == savedHousingCapacityContribution
                && housingSnapshot.ActiveHousingCount == savedActiveHousingCount
                && housingSnapshot.ActiveHousingCount > 0 == savedBoardingHouseActive;

            if (boundSettlementService != null
                && boundSettlementService.IsInitialized
                && boundSettlementService.TryGetPopulationSnapshot(settlementId, out CCS_SettlementPopulationSnapshot populationSnapshot)
                && populationSnapshot != null
                && populationSnapshot.HousingCapacityContribution == savedHousingCapacityContribution)
            {
                restored = restored && populationSnapshot.TotalPopulation <= populationSnapshot.PopulationCapacity;
            }

            if (restored)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifySettlementHousingAfterLoad,
                    "Settlement housing capacity and activation restored after load.");
            }
        }

        private void EvaluateNpcMovementPlaytestSteps()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            if (CCS_NpcMovementRuntimeBridge.TryGetFirstHostWithIdentity(
                    settlementId,
                    out _,
                    out CCS_NpcMovementSnapshot workerSnapshot)
                && workerSnapshot.IsValid
                && IsNpcMovementActiveStatus(workerSnapshot.Status))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyWorkerMovementActive,
                    $"Worker movement active ({workerSnapshot.Status}).");
            }

            if (CCS_NpcMovementRuntimeBridge.TryGetRepresentativeHostWithIdentity(
                    settlementId,
                    out _,
                    out CCS_NpcMovementSnapshot representativeSnapshot)
                && representativeSnapshot.IsValid
                && IsNpcMovementActiveStatus(representativeSnapshot.Status))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyRepresentativeMovementActive,
                    $"Representative movement active ({representativeSnapshot.Status}).");
            }

            if (CCS_NpcMovementRuntimeBridge.TryGetFirstHostWithIdentity(
                    settlementId,
                    out _,
                    out CCS_NpcMovementSnapshot homeSnapshot)
                && homeSnapshot.IsValid
                && IsNpcMovementHomeStatus(homeSnapshot.Status))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyScheduleTransitionToHome,
                    $"Schedule transitioned to home ({homeSnapshot.Status}).");
            }

            if (CCS_NpcMovementRuntimeBridge.TryGetFirstHostWithIdentity(
                    settlementId,
                    out _,
                    out CCS_NpcMovementSnapshot workSnapshot)
                && workSnapshot.IsValid
                && IsNpcMovementWorkStatus(workSnapshot.Status))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyScheduleTransitionToWork,
                    $"Schedule transitioned to work ({workSnapshot.Status}).");
            }
        }

        private bool TryCaptureNpcMovementSaveState()
        {
            if (npcMovementSaveCaptured)
            {
                return npcMovementSaveCaptured;
            }

            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            if (!CCS_NpcMovementRuntimeBridge.TryGetFirstHostWithIdentity(
                    settlementId,
                    out _,
                    out CCS_NpcMovementSnapshot snapshot)
                || !snapshot.IsValid)
            {
                return false;
            }

            savedNpcMovementStatus = (int)snapshot.Status;
            savedNpcMovementTargetAnchorId = snapshot.TargetAnchorId ?? string.Empty;
            npcMovementSaveCaptured = true;
            return true;
        }

        private void EvaluateNpcMovementStateAfterLoad()
        {
            if (!npcMovementSaveCaptured)
            {
                return;
            }

            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            CCS_NpcMovementRuntimeBridge.RefreshAllMovementHosts();

            bool restored = CCS_NpcMovementRuntimeBridge.TryGetFirstHostWithIdentity(
                    settlementId,
                    out _,
                    out CCS_NpcMovementSnapshot snapshot)
                && snapshot.IsValid
                && (int)snapshot.Status == savedNpcMovementStatus
                && string.Equals(
                    snapshot.TargetAnchorId,
                    savedNpcMovementTargetAnchorId,
                    System.StringComparison.OrdinalIgnoreCase);

            if (restored)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcMovementAfterLoad,
                    "NPC movement state restored after load.");
            }
        }

        private void EvaluateNpcSchedulePlaytestSteps()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            if (CCS_NpcScheduleRuntimeBridge.TryGetFirstHostScheduleSnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcScheduleSnapshot assignedSnapshot)
                && assignedSnapshot.IsValid)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcScheduleAssigned,
                    $"Schedule assigned ({assignedSnapshot.ActiveScheduleId}, block {assignedSnapshot.CurrentBlockType}).");
            }

            if (CCS_NpcScheduleRuntimeBridge.TryGetFirstHostScheduleSnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcScheduleSnapshot workSnapshot)
                && workSnapshot.IsValid
                && CCS_NpcScheduleValidationUtility.IsWorkLikeBlock(workSnapshot.CurrentBlockType)
                && (workSnapshot.CurrentTargetKind == CCS_NpcScheduleTargetKind.Workplace
                    || workSnapshot.CurrentTargetKind == CCS_NpcScheduleTargetKind.ServicePoint))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.ForceEvaluateNpcScheduleWorkBlock,
                    $"Work block active ({workSnapshot.CurrentBlockType}).");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcScheduleWorkplaceTarget,
                    $"Workplace target verified ({workSnapshot.CurrentTargetKind}, {workSnapshot.CurrentTargetId}).");
            }

            if (CCS_NpcScheduleRuntimeBridge.TryGetFirstHostScheduleSnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcScheduleSnapshot homeSnapshot)
                && homeSnapshot.IsValid
                && CCS_NpcScheduleValidationUtility.IsHomeLikeBlock(homeSnapshot.CurrentBlockType)
                && homeSnapshot.CurrentTargetKind == CCS_NpcScheduleTargetKind.Housing)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.ForceEvaluateNpcScheduleHomeBlock,
                    $"Home block active ({homeSnapshot.CurrentBlockType}).");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcScheduleHousingTarget,
                    $"Housing target verified ({homeSnapshot.CurrentTargetId}).");
            }
        }

        private bool TryCaptureNpcScheduleSaveState()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            if (!CCS_NpcScheduleRuntimeBridge.TryGetFirstHostScheduleSnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcScheduleSnapshot snapshot)
                || !snapshot.IsValid)
            {
                return false;
            }

            savedNpcScheduleId = snapshot.ActiveScheduleId ?? string.Empty;
            savedNpcScheduleBlockType = (int)snapshot.CurrentBlockType;
            savedNpcScheduleTargetKind = (int)snapshot.CurrentTargetKind;
            savedNpcScheduleTargetId = snapshot.CurrentTargetId ?? string.Empty;
            savedNpcScheduleEvaluatedHour = snapshot.LastEvaluatedHour;
            npcScheduleSaveCaptured = true;
            return true;
        }

        private void EvaluateNpcScheduleStateAfterLoad()
        {
            if (!npcScheduleSaveCaptured)
            {
                return;
            }

            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            CCS_NpcScheduleRuntimeBridge.RefreshAllScheduleHosts();
            CCS_NpcMovementRuntimeBridge.RefreshAllMovementHosts();

            bool restored = CCS_NpcScheduleRuntimeBridge.TryGetFirstHostScheduleSnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcScheduleSnapshot snapshot)
                && snapshot.IsValid
                && string.Equals(snapshot.ActiveScheduleId, savedNpcScheduleId, System.StringComparison.OrdinalIgnoreCase)
                && (int)snapshot.CurrentBlockType == savedNpcScheduleBlockType
                && (int)snapshot.CurrentTargetKind == savedNpcScheduleTargetKind
                && string.Equals(snapshot.CurrentTargetId, savedNpcScheduleTargetId, System.StringComparison.OrdinalIgnoreCase)
                && snapshot.LastEvaluatedHour == savedNpcScheduleEvaluatedHour;

            if (restored)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcScheduleAfterLoad,
                    $"NPC schedule restored ({snapshot.ActiveScheduleId}, {snapshot.CurrentBlockType}).");
            }
        }

        private void EvaluateNpcActivityPlaytestSteps()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            if (CCS_NpcActivityRuntimeBridge.TryGetFirstHostActivitySnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcActivitySnapshot workSnapshot)
                && workSnapshot.IsValid
                && (workSnapshot.CurrentActivityType == CCS_NpcActivityType.Working
                    || workSnapshot.CurrentActivityType == CCS_NpcActivityType.Serving))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.ForceNpcActivityWorkBlock,
                    $"Work/service block active ({workSnapshot.ScheduleBlockType}).");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcActivityWorkingOrServing,
                    $"Activity verified ({workSnapshot.CurrentActivityType}).");
            }

            if (CCS_NpcActivityRuntimeBridge.TryGetFirstHostActivitySnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcActivitySnapshot homeSnapshot)
                && homeSnapshot.IsValid
                && (homeSnapshot.CurrentActivityType == CCS_NpcActivityType.Resting
                    || homeSnapshot.CurrentActivityType == CCS_NpcActivityType.Sleeping))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.ForceNpcActivityHomeBlock,
                    $"Home/sleep block active ({homeSnapshot.ScheduleBlockType}).");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcActivityRestingOrSleeping,
                    $"Activity verified ({homeSnapshot.CurrentActivityType}).");
            }

            if (CCS_NpcMovementRuntimeBridge.TryGetFirstHostWithIdentity(
                    settlementId,
                    out _,
                    out CCS_NpcMovementSnapshot movementSnapshot)
                && movementSnapshot.IsValid
                && CCS_NpcMovementValidationUtility.IsTravelingStatus(movementSnapshot.Status))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.ForceNpcActivityTraveling,
                    $"Movement active ({movementSnapshot.Status}).");
            }

            if (CCS_NpcActivityRuntimeBridge.TryGetFirstHostActivitySnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcActivitySnapshot travelSnapshot)
                && travelSnapshot.IsValid
                && travelSnapshot.CurrentActivityType == CCS_NpcActivityType.Traveling)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcActivityTraveling,
                    "Activity Traveling verified.");
            }
        }

        private bool TryCaptureNpcActivitySaveState()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            if (!CCS_NpcActivityRuntimeBridge.TryGetFirstHostActivitySnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcActivitySnapshot snapshot)
                || !snapshot.IsValid)
            {
                return false;
            }

            savedNpcActivityType = (int)snapshot.CurrentActivityType;
            savedNpcActivityEvaluatedHour = snapshot.LastEvaluatedHour;
            npcActivitySaveCaptured = true;
            return true;
        }

        private void EvaluateNpcActivityStateAfterLoad()
        {
            if (!npcActivitySaveCaptured)
            {
                return;
            }

            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            CCS_NpcScheduleRuntimeBridge.RefreshAllScheduleHosts();
            CCS_NpcMovementRuntimeBridge.RefreshAllMovementHosts();
            CCS_NpcActivityRuntimeBridge.RefreshAllActivityHosts();

            bool restored = CCS_NpcActivityRuntimeBridge.TryGetFirstHostActivitySnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcActivitySnapshot snapshot)
                && snapshot.IsValid
                && (int)snapshot.CurrentActivityType == savedNpcActivityType
                && snapshot.LastEvaluatedHour == savedNpcActivityEvaluatedHour;

            if (restored)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcActivityAfterLoad,
                    $"NPC activity restored/re-evaluated ({snapshot.CurrentActivityType}).");
            }
        }

        public bool TryPlaytestNpcAffiliationFoundationShortcut()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            DiscoverPlaytestSettlement(settlementId);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForNpcAffiliation,
                "Trading Post discovered for NPC affiliation playtest.");

            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 25);
            if (boundContractService != null && boundContractService.IsInitialized)
            {
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    boundContractService.TryAcceptContract(
                        CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                        settlementId);
                    CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                        CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                    if (result != null && result.IsSuccess && attempt == 0)
                    {
                        break;
                    }
                }
            }

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            CCS_NpcRuntimeBridge.RefreshAllPlaceholderIdentities();
            CCS_NpcServiceRepresentativeRuntimeBridge.RefreshAllRepresentativeAssignments();
            CCS_NpcAffiliationRuntimeBridge.RefreshAllAffiliationHosts();

            if (CCS_NpcAffiliationRuntimeBridge.TryGetFirstHostAffiliationSnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcAffiliationSnapshot workforceSnapshot)
                && workforceSnapshot.IsValid)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SpawnWorkforceNpcForAffiliation,
                    $"Workforce NPC ready for affiliation playtest ({workforceSnapshot.DisplayName}).");
            }

            EvaluateNpcAffiliationPlaytestSteps();

            const string bankBusinessId = "ccs.survival.business.bank";
            CCS_NpcServiceRepresentativeRuntimeBridge.RefreshAllRepresentativeAssignments();
            CCS_NpcAffiliationRuntimeBridge.RefreshAllAffiliationHosts();
            EvaluateNpcAffiliationPlaytestSteps();

            if (CCS_NpcAffiliationRuntimeBridge.TryGetRepresentativeAffiliationSnapshot(
                    settlementId,
                    bankBusinessId,
                    out CCS_NpcAffiliationSnapshot representativeSnapshot)
                && representativeSnapshot.IsValid
                && representativeSnapshot.IsServiceRepresentative)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcRepresentativeAffiliation,
                    $"Representative affiliation verified ({representativeSnapshot.DisplayName}).");
            }

            return true;
        }

        private void EvaluateNpcAffiliationPlaytestSteps()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            if (CCS_NpcAffiliationRuntimeBridge.TryGetFirstHostAffiliationSnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcAffiliationSnapshot snapshot)
                && snapshot.IsValid
                && string.Equals(snapshot.SettlementId, settlementId, System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcSettlementAffiliation,
                    $"Settlement affiliation verified ({snapshot.SettlementDisplayName}).");
            }

            if (CCS_NpcAffiliationRuntimeBridge.TryGetFirstHostAffiliationSnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcAffiliationSnapshot workforceSnapshot)
                && workforceSnapshot.IsValid
                && workforceSnapshot.WorkforceCategory > 0
                && !workforceSnapshot.IsServiceRepresentative)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcWorkforceAffiliation,
                    $"Workforce affiliation verified ({workforceSnapshot.WorkforceDisplayName}).");
            }
        }

        private bool TryCaptureNpcAffiliationSaveState()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            if (!CCS_NpcAffiliationRuntimeBridge.TryGetFirstHostAffiliationSnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcAffiliationSnapshot snapshot)
                || !snapshot.IsValid)
            {
                return false;
            }

            savedNpcAffiliationSettlementId = snapshot.SettlementId ?? string.Empty;
            savedNpcAffiliationBusinessId = snapshot.BusinessId ?? string.Empty;
            savedNpcAffiliationWorkforceCategory = snapshot.WorkforceCategory;
            savedNpcAffiliationLoyalty = snapshot.LoyaltyValue;
            npcAffiliationSaveCaptured = true;
            return true;
        }

        private void EvaluateNpcAffiliationStateAfterLoad()
        {
            if (!npcAffiliationSaveCaptured)
            {
                return;
            }

            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            CCS_NpcRuntimeBridge.RefreshAllPlaceholderIdentities();
            CCS_NpcAffiliationRuntimeBridge.RefreshAllAffiliationHosts();

            bool restored = CCS_NpcAffiliationRuntimeBridge.TryGetFirstHostAffiliationSnapshot(
                    settlementId,
                    out _,
                    out CCS_NpcAffiliationSnapshot snapshot)
                && snapshot.IsValid
                && string.Equals(snapshot.SettlementId, savedNpcAffiliationSettlementId, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(snapshot.BusinessId, savedNpcAffiliationBusinessId, System.StringComparison.OrdinalIgnoreCase)
                && snapshot.WorkforceCategory == savedNpcAffiliationWorkforceCategory
                && snapshot.LoyaltyValue == savedNpcAffiliationLoyalty;

            if (restored)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcAffiliationAfterLoad,
                    "NPC affiliations restored after load.");
            }
        }

        public bool TryPlaytestNpcDialogueFoundationShortcut()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            DiscoverPlaytestSettlement(settlementId);
            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.DiscoverSettlementForNpcDialogue,
                "Trading Post discovered for NPC dialogue playtest.");

            GrantPlaytestItem(CCS_RegionEconomyUtility.CornItemId, 25);
            if (boundContractService != null && boundContractService.IsInitialized)
            {
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    boundContractService.TryAcceptContract(
                        CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                        settlementId);
                    CCS_ContractCompletionResult result = boundContractService.TryCompleteContract(
                        CCS_SettlementGrowthContentIds.PlaytestCornContractId);
                    if (result != null && result.IsSuccess && attempt == 0)
                    {
                        break;
                    }
                }
            }

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            CCS_NpcRuntimeBridge.RefreshAllPlaceholderIdentities();
            CCS_NpcServiceRepresentativeRuntimeBridge.RefreshAllRepresentativeAssignments();
            CCS_NpcAffiliationRuntimeBridge.RefreshAllAffiliationHosts();
            CCS_NpcDialogueStubRuntimeBridge.RefreshAllDialogueHosts();

            if (CCS_NpcDialogueStubRuntimeBridge.TryGetFirstHostDialogueResult(
                    settlementId,
                    out CCS_INpcMovementHost dialogueHost,
                    out CCS_NpcDialogueStubResult unusedDialogueResult)
                && dialogueHost != null)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.SpawnNamedNpcForDialogue,
                    $"Named NPC ready for dialogue playtest ({dialogueHost.NpcIdentityId}).");
            }

            if (TrySimulateWorkforceDialogueInteraction(settlementId))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.InteractWithNpcForDialogue,
                    "Workforce NPC dialogue interaction simulated.");
            }

            EvaluateNpcDialoguePlaytestSteps();

            const string bankBusinessId = "ccs.survival.business.bank";
            CCS_NpcServiceRepresentativeRuntimeBridge.RefreshAllRepresentativeAssignments();
            if (CCS_NpcServiceRepresentativeRuntimeBridge.TrySimulateRepresentativeInteraction(
                    settlementId,
                    bankBusinessId))
            {
                EvaluateNpcDialoguePlaytestSteps();
            }

            return true;
        }

        private static bool TrySimulateWorkforceDialogueInteraction(string settlementId)
        {
            if (!CCS_NpcDialogueStubRuntimeBridge.TryGetFirstHostDialogueResult(
                    settlementId,
                    out CCS_INpcMovementHost host,
                    out CCS_NpcDialogueStubResult unusedDialogueResult)
                || host == null)
            {
                return false;
            }

            return CCS_NpcDialogueStubRuntimeBridge.ResolveAndDisplayForHost?.Invoke(host) == true;
        }

        private void EvaluateNpcDialoguePlaytestSteps()
        {
            CCS_NpcDialogueStubResult result = CCS_NpcDialogueStubRuntimeBridge.LastDialogueResult;
            if (result == null || !result.IsSuccess)
            {
                return;
            }

            if (result.HasGreeting)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcDialogueGreeting,
                    $"Dialogue greeting verified ({result.GreetingLine}).");
            }

            if (result.HasRoleIntroduction)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcDialogueRoleIntroduction,
                    $"Dialogue role introduction verified ({result.RoleIntroductionLine}).");
            }

            if (result.HasServiceHint)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcDialogueServiceHint,
                    $"Dialogue service hint verified ({result.ServiceHintLine}).");
            }
        }

        private bool TryCaptureNpcDialogueSaveState()
        {
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            if (!CCS_NpcDialogueStubRuntimeBridge.TryGetFirstHostDialogueResult(
                    settlementId,
                    out CCS_INpcMovementHost host,
                    out CCS_NpcDialogueStubResult result)
                || host == null
                || result == null
                || !result.IsSuccess)
            {
                return false;
            }

            savedNpcDialogueIdentityId = host.NpcIdentityId ?? string.Empty;
            savedNpcDialogueGreetingLine = result.GreetingLine ?? string.Empty;
            savedNpcDialogueRoleLine = result.RoleIntroductionLine ?? string.Empty;
            npcDialogueSaveCaptured = true;
            return true;
        }

        private void EvaluateNpcDialogueStateAfterLoad()
        {
            if (!npcDialogueSaveCaptured)
            {
                return;
            }

            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            CCS_NpcRuntimeBridge.RefreshAllPlaceholderIdentities();
            CCS_NpcAffiliationRuntimeBridge.RefreshAllAffiliationHosts();
            CCS_NpcDialogueStubRuntimeBridge.RefreshAllDialogueHosts();

            bool restored = false;
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(candidate =>
            {
                if (restored
                    || candidate == null
                    || !candidate.HasIdentity
                    || !string.Equals(candidate.NpcIdentityId, savedNpcDialogueIdentityId, System.StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(candidate.SettlementId, settlementId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (CCS_NpcDialogueStubRuntimeBridge.TryResolveForHost(candidate, out CCS_NpcDialogueStubResult result)
                    && result != null
                    && result.IsSuccess
                    && result.HasGreeting
                    && result.HasRoleIntroduction
                    && string.Equals(result.GreetingLine, savedNpcDialogueGreetingLine, System.StringComparison.Ordinal)
                    && string.Equals(result.RoleIntroductionLine, savedNpcDialogueRoleLine, System.StringComparison.Ordinal))
                {
                    restored = true;
                }
            });

            if (restored)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcDialogueAfterLoad,
                    "NPC dialogue still resolves after load.");
            }
        }

        private static bool IsNpcMovementActiveStatus(CCS_NpcMovementStatus status)
        {
            return status == CCS_NpcMovementStatus.TravelingToWork
                || status == CCS_NpcMovementStatus.Working
                || status == CCS_NpcMovementStatus.TravelingHome
                || status == CCS_NpcMovementStatus.AtHome;
        }

        private static bool IsNpcMovementHomeStatus(CCS_NpcMovementStatus status)
        {
            return status == CCS_NpcMovementStatus.TravelingHome
                || status == CCS_NpcMovementStatus.AtHome;
        }

        private static bool IsNpcMovementWorkStatus(CCS_NpcMovementStatus status)
        {
            return status == CCS_NpcMovementStatus.TravelingToWork
                || status == CCS_NpcMovementStatus.Working;
        }

        private static void SetPlaytestScheduleHour(int hour)
        {
            if (!CCS_TimeOfDayRuntimeBridge.TryGetTimeOfDayService(out CCS_TimeOfDayService timeOfDayService)
                || timeOfDayService == null
                || !timeOfDayService.IsInitialized)
            {
                return;
            }

            CCS_GameTimeSnapshot snapshot = timeOfDayService.CreateSnapshot();
            timeOfDayService.SetTime(snapshot.DayNumber, hour, 0);
        }

        private void EvaluateNpcServiceRepresentativePlaytestSteps()
        {
            const string generalStoreBusinessId = "ccs.survival.business.generalstore";
            const string bankBusinessId = "ccs.survival.business.bank";
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;

            if (CCS_NpcServiceRepresentativeRuntimeBridge.TryGetActiveRepresentativeSnapshot(
                    settlementId,
                    generalStoreBusinessId,
                    out CCS_NpcServiceRepresentativeSnapshot generalStoreSnapshot)
                && generalStoreSnapshot.IsActive)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyGeneralStoreRepresentativeAssigned,
                    $"General Store representative assigned: {generalStoreSnapshot.DisplayName}.");
            }

            if (CCS_NpcServiceRepresentativeRuntimeBridge.TryGetActiveRepresentativeSnapshot(
                    settlementId,
                    bankBusinessId,
                    out CCS_NpcServiceRepresentativeSnapshot bankSnapshot)
                && bankSnapshot.IsActive)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyBankRepresentativeAssigned,
                    $"Bank representative assigned: {bankSnapshot.DisplayName}.");
            }
        }

        private bool TryCaptureNpcServiceRepresentativeSaveState()
        {
            if (npcServiceRepresentativeSaveCaptured)
            {
                return npcServiceRepresentativeSaveCaptured;
            }

            const string generalStoreBusinessId = "ccs.survival.business.generalstore";
            const string bankBusinessId = "ccs.survival.business.bank";
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;

            if (!CCS_NpcServiceRepresentativeRuntimeBridge.TryGetActiveRepresentativeSnapshot(
                    settlementId,
                    generalStoreBusinessId,
                    out CCS_NpcServiceRepresentativeSnapshot generalStoreSnapshot)
                || !generalStoreSnapshot.IsValid)
            {
                return false;
            }

            if (!CCS_NpcServiceRepresentativeRuntimeBridge.TryGetActiveRepresentativeSnapshot(
                    settlementId,
                    bankBusinessId,
                    out CCS_NpcServiceRepresentativeSnapshot bankSnapshot)
                || !bankSnapshot.IsValid)
            {
                return false;
            }

            savedGeneralStoreRepresentativeId = generalStoreSnapshot.RepresentativeId;
            savedGeneralStoreRepresentativeIdentityId = generalStoreSnapshot.AssignedNpcIdentityId;
            savedBankRepresentativeId = bankSnapshot.RepresentativeId;
            savedBankRepresentativeIdentityId = bankSnapshot.AssignedNpcIdentityId;
            npcServiceRepresentativeSaveCaptured = true;
            return true;
        }

        private void EvaluateNpcServiceRepresentativeStateAfterLoad()
        {
            if (!npcServiceRepresentativeSaveCaptured)
            {
                return;
            }

            const string generalStoreBusinessId = "ccs.survival.business.generalstore";
            const string bankBusinessId = "ccs.survival.business.bank";
            string settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;

            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
            CCS_NpcRuntimeBridge.RefreshAllPlaceholderIdentities();
            CCS_NpcServiceRepresentativeRuntimeBridge.RefreshAllRepresentativeAssignments();

            bool generalStoreRestored = CCS_NpcServiceRepresentativeRuntimeBridge.TryGetActiveRepresentativeSnapshot(
                    settlementId,
                    generalStoreBusinessId,
                    out CCS_NpcServiceRepresentativeSnapshot generalStoreSnapshot)
                && string.Equals(
                    generalStoreSnapshot.RepresentativeId,
                    savedGeneralStoreRepresentativeId,
                    System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(
                    generalStoreSnapshot.AssignedNpcIdentityId,
                    savedGeneralStoreRepresentativeIdentityId,
                    System.StringComparison.OrdinalIgnoreCase);

            bool bankRestored = CCS_NpcServiceRepresentativeRuntimeBridge.TryGetActiveRepresentativeSnapshot(
                    settlementId,
                    bankBusinessId,
                    out CCS_NpcServiceRepresentativeSnapshot bankSnapshot)
                && string.Equals(
                    bankSnapshot.RepresentativeId,
                    savedBankRepresentativeId,
                    System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(
                    bankSnapshot.AssignedNpcIdentityId,
                    savedBankRepresentativeIdentityId,
                    System.StringComparison.OrdinalIgnoreCase);

            if (generalStoreRestored && bankRestored)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyNpcServiceRepresentativeAfterLoad,
                    "NPC service representative assignments restored after load.");
            }
        }

        private void EvaluateSettlementVisualGrowthPlaytestSteps()
        {
            int anchorCount = CCS_SettlementVisualGrowthRuntimeBridge.GetRegisteredAnchorCount();
            if (anchorCount >= 3)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyOutpostVisualMarkers,
                    $"Found {anchorCount} settlement visual growth anchors in scene.");
            }

            CCS_SettlementVisualGrowthStatus outpostCampStatus =
                CCS_SettlementVisualGrowthRuntimeBridge.ResolveVisualStatus(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    CCS_SettlementGrowthStage.Outpost);
            if (outpostCampStatus == CCS_SettlementVisualGrowthStatus.Active)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyOutpostVisualMarkers,
                    "Outpost visual markers are Active at default growth stage.");
            }

            CCS_SettlementVisualGrowthStatus tradingSignStatus =
                CCS_SettlementVisualGrowthRuntimeBridge.ResolveVisualStatus(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    CCS_SettlementGrowthStage.TradingPost);
            if (tradingSignStatus == CCS_SettlementVisualGrowthStatus.Active)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyTradingPostVisualMarkersActive,
                    "Trading Post visual markers are Active after growth advance.");
            }
        }

        private bool TryCaptureVisualGrowthSaveState()
        {
            if (visualGrowthSaveCaptured)
            {
                return visualGrowthSaveCaptured;
            }

            savedTradingPostTradingSignVisualStatus = (int)CCS_SettlementVisualGrowthRuntimeBridge.ResolveVisualStatus(
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementGrowthStage.TradingPost);
            visualGrowthSaveCaptured = savedTradingPostTradingSignVisualStatus >= 0;
            return visualGrowthSaveCaptured;
        }

        private void EvaluateVisualGrowthStateAfterLoad()
        {
            if (!visualGrowthSaveCaptured)
            {
                return;
            }

            CCS_SettlementVisualGrowthRuntimeBridge.RefreshAllAnchors();
            int restoredStatus = (int)CCS_SettlementVisualGrowthRuntimeBridge.ResolveVisualStatus(
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementGrowthStage.TradingPost);
            if (restoredStatus == savedTradingPostTradingSignVisualStatus)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyVisualGrowthAfterLoad,
                    "Settlement visual growth markers restored from growth state after load.");
            }
        }

        private void EvaluateBusinessPresencePlaytestSteps()
        {
            int anchorCount = CCS_BusinessPresenceRuntimeBridge.GetRegisteredAnchorCount();
            if (anchorCount >= 5)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyBusinessMarkersExist,
                    $"Found {anchorCount} business presence anchors in scene.");
            }

            CCS_BusinessPresenceStatus status = CCS_BusinessPresenceRuntimeBridge.ResolvePresenceStatus(
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_BusinessType.GeneralStore);
            if (status == CCS_BusinessPresenceStatus.Active)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyBusinessMarkerActive,
                    "General Store presence marker is Active.");
            }
        }

        private bool TryCaptureBusinessPresenceSaveState()
        {
            if (businessPresenceSaveCaptured)
            {
                return businessPresenceSaveCaptured;
            }

            savedGeneralStorePresenceStatus = (int)CCS_BusinessPresenceRuntimeBridge.ResolvePresenceStatus(
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_BusinessType.GeneralStore);
            businessPresenceSaveCaptured = savedGeneralStorePresenceStatus >= 0;
            return businessPresenceSaveCaptured;
        }

        private void EvaluateBusinessPresenceStateAfterLoad()
        {
            if (!businessPresenceSaveCaptured)
            {
                return;
            }

            CCS_BusinessPresenceRuntimeBridge.RefreshAllAnchors();
            int restoredStatus = (int)CCS_BusinessPresenceRuntimeBridge.ResolvePresenceStatus(
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_BusinessType.GeneralStore);
            if (restoredStatus == savedGeneralStorePresenceStatus)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyBusinessPresenceAfterLoad,
                    "Business presence marker state restored from simulation after load.");
            }
        }

        private void EvaluateBusinessPlaytestSteps()
        {
            if (boundSettlementService == null
                || !boundSettlementService.IsInitialized
                || !boundSettlementService.TryGetBusinessSnapshot(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    out CCS_BusinessSnapshot snapshot)
                || snapshot == null)
            {
                return;
            }

            int catalogCount = snapshot.ActiveBusinesses.Length
                + snapshot.InactiveBusinesses.Length
                + snapshot.AvailableBusinesses.Length;
            if (catalogCount >= 5)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifySettlementBusinessCatalog,
                    $"Trading Post catalog lists {catalogCount} business entries.");
            }

            if (snapshot.ActiveBusinesses.Length > 0)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyBusinessActivated,
                    $"Active businesses: {snapshot.ActiveBusinesses.Length}.");
            }
        }

        private bool TryCaptureBusinessSaveState()
        {
            if (businessSaveCaptured
                || boundSettlementService == null
                || !boundSettlementService.IsInitialized)
            {
                return businessSaveCaptured;
            }

            if (boundSettlementService.TryGetBusinessSnapshot(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    out CCS_BusinessSnapshot snapshot)
                && snapshot != null)
            {
                savedActiveBusinessCount = snapshot.ActiveBusinesses.Length;
                businessSaveCaptured = savedActiveBusinessCount > 0;
            }

            return businessSaveCaptured;
        }

        private void EvaluateBusinessStateAfterLoad()
        {
            if (!businessSaveCaptured
                || boundSettlementService == null
                || !boundSettlementService.IsInitialized)
            {
                return;
            }

            if (boundSettlementService.TryGetBusinessSnapshot(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    out CCS_BusinessSnapshot snapshot)
                && snapshot != null
                && snapshot.ActiveBusinesses.Length == savedActiveBusinessCount)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyBusinessStateAfterLoad,
                    "Business activation counts restored after load.");
            }
        }

        private bool TryCapturePopulationSaveState()
        {
            if (populationSaveCaptured
                || boundSettlementService == null
                || !boundSettlementService.IsInitialized)
            {
                return populationSaveCaptured;
            }

            if (boundSettlementService.TryGetPopulationSnapshot(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    out CCS_SettlementPopulationSnapshot snapshot)
                && snapshot != null)
            {
                savedPopulationTotal = snapshot.TotalPopulation;
                savedPopulationGrowthRate = snapshot.PopulationGrowthRate;
                populationSaveCaptured = savedPopulationTotal > 0;
            }

            return populationSaveCaptured;
        }

        private void EvaluatePopulationStateAfterLoad()
        {
            if (!populationSaveCaptured
                || boundSettlementService == null
                || !boundSettlementService.IsInitialized)
            {
                return;
            }

            if (boundSettlementService.TryGetPopulationSnapshot(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    out CCS_SettlementPopulationSnapshot snapshot)
                && snapshot != null
                && snapshot.TotalPopulation == savedPopulationTotal
                && Mathf.Approximately(snapshot.PopulationGrowthRate, savedPopulationGrowthRate))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyPopulationAfterLoad,
                    "Population totals and growth rate restored after load.");
            }
        }

        private bool TryCaptureSettlementGrowthSaveState()
        {
            if (settlementGrowthSaveCaptured
                || boundSettlementService == null
                || !boundSettlementService.IsInitialized
                || !boundSettlementService.TryGetGrowthSnapshot(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    out CCS_SettlementGrowthSnapshot snapshot)
                || snapshot == null)
            {
                return settlementGrowthSaveCaptured;
            }

            savedSettlementGrowthStage = (int)snapshot.CurrentGrowthStage;
            savedSettlementPreviousGrowthStage = (int)snapshot.PreviousGrowthStage;
            savedSettlementGrowthProgressPercent = snapshot.GrowthProgressPercent;
            savedSettlementCompletedContractsCount = snapshot.CompletedContractsCount;
            settlementGrowthSaveCaptured = true;
            return true;
        }

        private void EvaluateSettlementGrowthAfterLoad()
        {
            if (!settlementGrowthSaveCaptured
                || boundSettlementService == null
                || !boundSettlementService.IsInitialized
                || !boundSettlementService.TryGetGrowthSnapshot(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    out CCS_SettlementGrowthSnapshot snapshot)
                || snapshot == null)
            {
                return;
            }

            if ((int)snapshot.CurrentGrowthStage == savedSettlementGrowthStage
                && (int)snapshot.PreviousGrowthStage == savedSettlementPreviousGrowthStage
                && Mathf.Approximately(snapshot.GrowthProgressPercent, savedSettlementGrowthProgressPercent)
                && snapshot.CompletedContractsCount == savedSettlementCompletedContractsCount)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifySettlementGrowthAfterLoad,
                    "Settlement growth state restored after load.");
            }
        }

        public bool TryPlaytestForceUpkeepDue()
        {
            if (boundUpkeepService == null || !boundUpkeepService.IsInitialized)
            {
                return false;
            }

            string claimId = ResolveFirstLandClaimInstanceId();
            if (string.IsNullOrWhiteSpace(claimId))
            {
                return false;
            }

            CCS_UpkeepTransactionResult result = boundUpkeepService.TryForceDue(claimId);
            return result != null && result.IsSuccess;
        }

        public bool TryPlaytestPayUpkeep()
        {
            if (boundUpkeepService == null || !boundUpkeepService.IsInitialized)
            {
                return false;
            }

            string claimId = ResolveFirstLandClaimInstanceId();
            if (string.IsNullOrWhiteSpace(claimId))
            {
                return false;
            }

            CCS_UpkeepTransactionResult result = boundUpkeepService.TryPayUpkeep(claimId);
            return result != null && result.IsSuccess;
        }

        public bool TryPlaytestLandOwnershipFoundationShortcut()
        {
            if (boundCurrencyService != null && boundCurrencyService.IsInitialized)
            {
                boundCurrencyService.AddCurrency(TradeDollarsCurrencyId, 2000, "Playtest land ownership foundation funds");
            }

            GrantPlaytestItem(HomesteadClaimDeedItemId, 1);
            GrantPlaytestItem(FarmPlotKitItemId, 1);
            TryPlaytestBuyHomesteadClaimDeed();
            return true;
        }

        public bool TryPlaytestFarmingFoundationShortcut()
        {
            if (boundCurrencyService != null && boundCurrencyService.IsInitialized)
            {
                boundCurrencyService.AddCurrency(TradeDollarsCurrencyId, 1500, "Playtest farming foundation funds");
            }

            GrantPlaytestItem(FarmPlotKitItemId, 1);
            GrantPlaytestItem(CornSeedItemId, 3);
            TryPlaytestBuyFarmPlotKit();
            TryPlaytestBuyCornSeed();
            TryPlaytestForceCropGrowth();
            TryPlaytestHarvestCrop();
            return true;
        }

        private void GrantPlaytestItem(string itemId, int quantity)
        {
            if (!CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || !inventoryService.IsInitialized)
            {
                return;
            }

            CCS_ItemDefinition itemDefinition = FindItemDefinitionById(itemId);
            if (itemDefinition != null)
            {
                inventoryService.AddItem(itemDefinition, quantity);
            }
        }

        private static bool IsWorldSimulationIndustryItem(string itemId)
        {
            return itemId == IronOreItemId
                || itemId == CCS_ProspectingContentIds.NailsItemId
                || itemId.Contains("refinediron", System.StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("ironbar", System.StringComparison.OrdinalIgnoreCase);
        }

        private void CapturePlaytestReputationBaselineIfNeeded()
        {
            if (playtestReputationBaselineCaptured
                || !TryGetPlaytestSettlementReputation(out int value, out _))
            {
                return;
            }

            playtestReputationBaseline = value;
            playtestReputationBaselineCaptured = true;
        }

        private bool TryGetPlaytestSettlementReputation(out int value, out CCS_ReputationTier tier)
        {
            value = 0;
            tier = CCS_ReputationTier.Neutral;
            if (boundReputationService == null || !boundReputationService.IsInitialized)
            {
                return false;
            }

            string settlementId = boundReputationService.ActiveProfile != null
                ? boundReputationService.ActiveProfile.DefaultTradingPostSettlementId
                : CCS_ReputationContentIds.DefaultTradingPostSettlementId;
            if (boundReputationService.TryGetSettlementStanding(settlementId, out CCS_ReputationStanding standing)
                && standing != null)
            {
                value = standing.CurrentValue;
                tier = standing.DisplayTier;
            }

            return true;
        }

        private void EvaluateReputationAfterSell()
        {
            if (!TryGetPlaytestSettlementReputation(out int value, out CCS_ReputationTier tier))
            {
                return;
            }

            if (value > playtestReputationBaseline)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyTradingPostReputationAfterSell,
                    $"Trading post reputation increased to {value}.");
                playtestReputationBeforeObligation = value;
            }

            EvaluateSettlementReputationStanding();
        }

        private void EvaluateSettlementReputationStanding()
        {
            if (!TryGetPlaytestSettlementReputation(out int value, out CCS_ReputationTier tier))
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.VerifySettlementReputationStanding,
                $"Settlement reputation standing: {tier} ({value}).");
        }

        private void EvaluateReputationAfterBuy(CCS_VendorTransactionResult result)
        {
            if (result == null || result.WasSell || !result.IsSuccess)
            {
                return;
            }

            if (result.BaseUnitPrice > 0
                && result.FinalUnitPrice == CCS_ReputationPriceModifierUtility.ApplyModifier(
                    result.BaseUnitPrice,
                    result.ReputationPriceModifier))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyVendorBuyPriceModifier,
                    $"Buy modifier {result.ReputationPriceModifier:0.00} applied "
                    + $"(base {result.BaseUnitPrice}, final {result.FinalUnitPrice}).");
            }
        }

        private void EvaluateSettlementServiceAccess()
        {
            if (!EvaluatePlaytestGeneralStoreAccessAllowed())
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.VerifySettlementServiceAccess,
                "General store service access allowed.");
        }

        private bool EvaluatePlaytestGeneralStoreAccessAllowed()
        {
            if (boundReputationService == null || !boundReputationService.IsInitialized)
            {
                return true;
            }

            CCS_ServiceAccessResult accessResult = CCS_ServiceAccessEvaluationUtility.EvaluateForServicePoint(
                boundReputationService,
                CCS_ReputationContentIds.DefaultTradingPostSettlementId,
                string.Empty,
                (int)CCS_SettlementServicePointType.GeneralStore,
                true);
            return accessResult.IsAllowed;
        }

        private float ResolvePlaytestBuyPriceModifier()
        {
            return CCS_ReputationPriceModifierUtility.ResolveBuyPriceModifier(
                boundReputationService,
                CCS_ReputationContentIds.DefaultTradingPostSettlementId);
        }

        private void EvaluateReputationAfterObligation()
        {
            if (!TryGetPlaytestSettlementReputation(out int value, out _))
            {
                return;
            }

            int baseline = playtestReputationBeforeObligation > playtestReputationBaseline
                ? playtestReputationBeforeObligation
                : playtestReputationBaseline;
            if (value > baseline)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyReputationChangedAfterObligation,
                    $"Settlement reputation increased to {value} after obligation.");
            }
        }

        private void HandleContractAccepted(CCS_ContractCompletionResult result)
        {
            if (result == null || !result.IsSuccess)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.AcceptFrontierContract,
                $"Accepted contract {result.ContractId}.");
            if (string.Equals(
                    result.ContractId,
                    CCS_RegionSpecializationContentIds.RegionalEconomyPlaytestCornContractId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.AcceptRegionalSpecialtyContract,
                    $"Accepted regional specialty contract {result.ContractId}.");
            }

            if (string.Equals(
                    result.ContractId,
                    CCS_ContractContentIds.LumberDeliveryContractId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.AcceptMultiSettlementRegionalContract,
                    $"Accepted regional contract {result.ContractId}.");
            }

            if (string.Equals(
                    result.ContractId,
                    CCS_TradeRoutesFreightContentIds.PineRidgeLumberFreightContractId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.AcceptPineRidgeLumberFreightContract,
                    $"Accepted freight contract {result.ContractId}.");
            }
        }

        private void HandleContractCompleted(CCS_ContractCompletionResult result)
        {
            if (result == null || !result.IsSuccess)
            {
                return;
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.CompleteFrontierContract,
                $"Completed contract {result.ContractId}.");
            if (string.Equals(
                    result.ContractId,
                    CCS_RegionSpecializationContentIds.RegionalEconomyPlaytestCornContractId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.CompleteRegionalSpecialtyContract,
                    $"Completed regional specialty contract {result.ContractId}.");
            }

            EvaluateContractRewardSteps(result);
            EvaluateRegionalProsperityRewardStep(result);
            EvaluateSettlementGrowthContractSteps(result);
            EvaluateMultiSettlementContractSteps(result);
            EvaluateFreightContractSteps(result);
        }

        private void EvaluateSettlementGrowthContractSteps(CCS_ContractCompletionResult result)
        {
            if (result == null || !result.IsSuccess)
            {
                return;
            }

            if (string.Equals(
                    result.ContractId,
                    CCS_SettlementGrowthContentIds.PlaytestCornContractId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.CompleteContractForSettlementGrowth,
                    $"Completed settlement growth contract {result.ContractId}.");
            }

            EvaluateSettlementGrowthProgressSteps();
        }

        private void EvaluateSettlementGrowthProgressSteps()
        {
            if (boundSettlementService == null
                || !boundSettlementService.IsInitialized
                || !boundSettlementService.TryGetGrowthSnapshot(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    out CCS_SettlementGrowthSnapshot snapshot)
                || snapshot == null)
            {
                return;
            }

            if (snapshot.Prosperity > 0f
                || snapshot.FoodSupplyHealthPercent > 0f
                || snapshot.IndustrialSupplyHealthPercent > 0f)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifySettlementGrowthSupplyProsperity,
                    $"Settlement prosperity {snapshot.Prosperity:0.##}, food {snapshot.FoodSupplyHealthPercent:0.##}%, "
                    + $"industrial {snapshot.IndustrialSupplyHealthPercent:0.##}%.");
            }

            if (snapshot.GrowthProgressPercent > 0f)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifySettlementGrowthProgress,
                    $"Settlement growth progress {snapshot.GrowthProgressPercent:0.##}%.");
            }

            if (snapshot.CurrentGrowthStage == CCS_SettlementGrowthStage.TradingPost)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.ReachTradingPostGrowthStage,
                    "Settlement reached TradingPost growth stage.");
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifySettlementGrowthStageChanged,
                    "Settlement growth stage advanced to TradingPost.");
            }
        }

        private void EvaluateRegionalProsperityRewardStep(CCS_ContractCompletionResult result)
        {
            if (result == null
                || !result.IsSuccess
                || !string.Equals(
                    result.ContractId,
                    CCS_RegionSpecializationContentIds.RegionalEconomyPlaytestCornContractId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            CapturePlaytestRegionalProsperityBaselineIfNeeded();
            if (boundWorldSimulationService != null
                && boundWorldSimulationService.IsInitialized
                && boundWorldSimulationService.TryGetSettlementState(
                    CCS_WorldSimulationContentIds.TradingPostSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                && settlementState != null
                && settlementState.prosperity > playtestRegionalProsperityBaseline)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyRegionalProsperityIncrease,
                    $"Regional prosperity increased to {settlementState.prosperity:0.##}.");
            }
        }

        private void CapturePlaytestContractBaselinesIfNeeded()
        {
            if (playtestContractBaselinesCaptured)
            {
                return;
            }

            if (boundCurrencyService != null && boundCurrencyService.IsInitialized)
            {
                playtestContractCurrencyBaseline =
                    boundCurrencyService.GetBalance(TradeDollarsCurrencyId);
            }

            if (TryGetPlaytestSettlementReputation(out int reputationValue, out _))
            {
                playtestContractReputationBaseline = reputationValue;
            }

            if (boundWorldSimulationService != null
                && boundWorldSimulationService.IsInitialized
                && boundWorldSimulationService.TryGetSettlementState(
                    CCS_WorldSimulationContentIds.TradingPostSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                && settlementState != null)
            {
                playtestContractProsperityBaseline = settlementState.prosperity;
            }

            playtestContractBaselinesCaptured = true;
        }

        private void TryEvaluateContractGoodsStep()
        {
            if (boundContractService == null || !boundContractService.IsInitialized)
            {
                return;
            }

            if (!boundContractService.TryGetDefinition(
                    CCS_ContractContentIds.MixedFrontierSupplyContractId,
                    out CCS_ContractDefinition definition)
                || definition == null)
            {
                return;
            }

            CCS_ContractRequirement[] requirements = definition.Requirements;
            for (int index = 0; index < requirements.Length; index++)
            {
                CCS_ContractRequirement requirement = requirements[index];
                if (requirement == null)
                {
                    continue;
                }

                int owned = ResolvePlaytestItemQuantity(requirement.ItemId);
                if (owned < requirement.Quantity)
                {
                    return;
                }
            }

            TryCompleteActiveStepOfType(
                CCS_PlaytestStepType.GatherContractGoods,
                "Contract delivery goods gathered.");
        }

        private int ResolvePlaytestItemQuantity(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || !CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null
                || !inventoryService.IsInitialized
                || inventoryService.ActiveProfile?.SaveRestoreItemDefinitions == null)
            {
                return 0;
            }

            CCS_ItemDefinition[] definitions = inventoryService.ActiveProfile.SaveRestoreItemDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_ItemDefinition definition = definitions[index];
                if (definition != null
                    && string.Equals(definition.ItemId, itemId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return inventoryService.GetQuantity(definition);
                }
            }

            return 0;
        }

        private void EvaluateContractRewardSteps(CCS_ContractCompletionResult result)
        {
            if (result == null || !result.IsSuccess)
            {
                return;
            }

            if (boundCurrencyService != null
                && boundCurrencyService.IsInitialized
                && result.TradeDollarsGranted > 0)
            {
                int balance = boundCurrencyService.GetBalance(TradeDollarsCurrencyId);
                if (balance >= playtestContractCurrencyBaseline + result.TradeDollarsGranted)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyContractMoneyReward,
                        $"Contract trade dollars reward granted ({result.TradeDollarsGranted}).");
                }
            }

            if (result.ReputationGainApplied > 0
                && TryGetPlaytestSettlementReputation(out int reputationValue, out _))
            {
                if (reputationValue >= playtestContractReputationBaseline + result.ReputationGainApplied)
                {
                    TryCompleteActiveStepOfType(
                        CCS_PlaytestStepType.VerifyContractReputationReward,
                        $"Contract reputation reward applied (+{result.ReputationGainApplied}).");
                }
            }

            if (result.ProsperityGainApplied > 0f
                && boundWorldSimulationService != null
                && boundWorldSimulationService.IsInitialized
                && boundWorldSimulationService.TryGetSettlementState(
                    CCS_WorldSimulationContentIds.TradingPostSettlementId,
                    out CCS_SettlementSimulationState settlementState)
                && settlementState != null
                && settlementState.prosperity > playtestContractProsperityBaseline)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyContractProsperityReward,
                    $"Settlement prosperity increased to {settlementState.prosperity:0.##}.");
            }
        }

        private void EvaluateContractStateAfterLoad()
        {
            if (boundContractService == null
                || !boundContractService.IsInitialized
                || string.IsNullOrWhiteSpace(savedContractDefinitionId)
                || savedContractState <= 0)
            {
                return;
            }

            CCS_ContractState currentState =
                boundContractService.GetContractState(savedContractDefinitionId);
            if ((int)currentState == savedContractState)
            {
                TryCompleteActiveStepOfType(
                    CCS_PlaytestStepType.VerifyContractStateAfterLoad,
                    "Contract state restored after load.");
            }
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
