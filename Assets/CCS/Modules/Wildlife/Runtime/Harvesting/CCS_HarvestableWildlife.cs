using CCS.Modules.Equipment;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.WorldResources;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HarvestableWildlife
// CATEGORY: Modules / Wildlife / Runtime / Harvesting
// PURPOSE: Interactable MonoBehaviour wrapper for wildlife carcass placeholders.
// PLACEMENT: Attach to primitive carcass placeholders in bootstrap verification scenes.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Carcass-only foundation. No combat kill state required in 0.9.3.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_HarvestableWildlife : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [Header("Wildlife Configuration")]
        [Tooltip("Wildlife definition that drives harvest rules and drops.")]
        [SerializeField] private CCS_WildlifeDefinition wildlifeDefinition;

        [Tooltip("Optional wildlife profile used when registry services are unavailable.")]
        [SerializeField] private CCS_WildlifeProfile wildlifeProfile;

        [Header("Interaction")]
        [Tooltip("Maximum distance at which this wildlife carcass accepts interaction requests.")]
        [SerializeField] private float interactionDistance = 3f;

        [Tooltip("When enabled, interaction assumes the required tool is available for harvest validation.")]
        [SerializeField] private bool assumeRequiredToolEquipped;

        private CCS_WildlifeState wildlifeState;
        private CCS_WildlifeHarvestService harvestService;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_PlayerEquipmentService equipmentService;
        private string instanceKey;

        #endregion

        #region Properties

        public CCS_WildlifeDefinition WildlifeDefinition => wildlifeDefinition;

        public CCS_WildlifeState WildlifeState => wildlifeState;

        public CCS_WildlifeHarvestService HarvestService => harvestService;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            instanceKey = gameObject.name + "_" + GetEntityId();
            ResetWildlifeState();
        }

        private void Start()
        {
            ResolveServices();
        }

        #endregion

        #region Public Methods

        public string GetInteractionDisplayName()
        {
            if (wildlifeDefinition == null || string.IsNullOrWhiteSpace(wildlifeDefinition.DisplayName))
            {
                return "Wildlife";
            }

            return wildlifeDefinition.DisplayName;
        }

        public bool CanInteract()
        {
            if (wildlifeDefinition == null || wildlifeState == null || wildlifeState.IsDepleted)
            {
                return false;
            }

            return CanHarvest(ResolveEquippedToolType());
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            CCS_WildlifeHarvestResult result = Harvest(ResolveEquippedToolType(), inventoryService);
            return result.IsSuccess;
        }

        public float GetInteractionDistance()
        {
            return interactionDistance;
        }

        public bool CanHarvest(CCS_RequiredToolType equippedToolType)
        {
            if (wildlifeDefinition == null || wildlifeState == null || harvestService == null)
            {
                return false;
            }

            CCS_WildlifeHarvestRequest request = new CCS_WildlifeHarvestRequest(
                wildlifeDefinition,
                wildlifeState,
                equippedToolType,
                instanceKey);

            return harvestService.CanHarvest(request);
        }

        public CCS_WildlifeHarvestResult Harvest(
            CCS_RequiredToolType equippedToolType,
            CCS_PlayerInventoryService inventoryServiceOverride = null)
        {
            if (wildlifeDefinition == null || wildlifeState == null || harvestService == null)
            {
                return CCS_WildlifeHarvestResult.Failure("Harvestable wildlife is not configured.");
            }

            CCS_PlayerInventoryService activeInventoryService =
                inventoryServiceOverride ?? inventoryService;

            CCS_WildlifeHarvestRequest request = new CCS_WildlifeHarvestRequest(
                wildlifeDefinition,
                wildlifeState,
                equippedToolType,
                instanceKey);

            return harvestService.TryHarvest(request, activeInventoryService);
        }

        public void ResetWildlifeState()
        {
            if (wildlifeDefinition == null)
            {
                wildlifeState = new CCS_WildlifeState(0);
                return;
            }

            wildlifeState = CCS_WildlifeState.CreateFromDefinition(wildlifeDefinition);
        }

        public CCS_WildlifeSnapshot GetSnapshot()
        {
            return new CCS_WildlifeSnapshot(wildlifeDefinition, wildlifeState, instanceKey);
        }

        #endregion

        #region Private Methods

        private void ResolveServices()
        {
            if (CCS_WildlifeRuntimeBridge.TryGetHarvestService(out CCS_WildlifeHarvestService registryHarvestService))
            {
                harvestService = registryHarvestService;
            }
            else
            {
                harvestService = new CCS_WildlifeHarvestService();

                if (wildlifeProfile != null)
                {
                    harvestService.InitializeFromProfile(wildlifeProfile);
                }
                else
                {
                    harvestService.Initialize();
                }
            }

            CCS_WildlifeRuntimeBridge.TryGetInventoryService(out inventoryService);
            CCS_EquipmentRuntimeBridge.TryGetEquipmentService(out equipmentService);
        }

        private CCS_RequiredToolType ResolveEquippedToolType()
        {
            if (wildlifeDefinition == null)
            {
                return CCS_RequiredToolType.None;
            }

            if (assumeRequiredToolEquipped)
            {
                return wildlifeDefinition.HarvestToolRequirement;
            }

            CCS_RequiredToolType requiredTool = wildlifeDefinition.HarvestToolRequirement;
            if (requiredTool == CCS_RequiredToolType.None)
            {
                return CCS_RequiredToolType.None;
            }

            CCS_ItemToolType requiredItemTool = (CCS_ItemToolType)(int)requiredTool;
            if (PlayerHasRequiredTool(requiredItemTool))
            {
                return requiredTool;
            }

            return CCS_RequiredToolType.None;
        }

        private bool PlayerHasRequiredTool(CCS_ItemToolType requiredItemTool)
        {
            if (inventoryService != null
                && inventoryService.IsInitialized
                && CCS_InventoryToolUtility.InventoryContainsTool(inventoryService, requiredItemTool))
            {
                return true;
            }

            if (equipmentService == null || !equipmentService.IsInitialized)
            {
                return false;
            }

            return EquippedSlotSatisfiesTool(CCS_EquipmentSlotType.MainHand, requiredItemTool)
                || EquippedSlotSatisfiesTool(CCS_EquipmentSlotType.Tool, requiredItemTool);
        }

        private bool EquippedSlotSatisfiesTool(
            CCS_EquipmentSlotType slotType,
            CCS_ItemToolType requiredItemTool)
        {
            CCS_EquippedItem equippedItem = equipmentService.GetEquippedItem(slotType);
            if (equippedItem?.ItemDefinition == null)
            {
                return false;
            }

            return CCS_InventoryToolUtility.EquippedItemSatisfiesTool(
                equippedItem.ItemDefinition,
                requiredItemTool);
        }

        #endregion
    }
}
