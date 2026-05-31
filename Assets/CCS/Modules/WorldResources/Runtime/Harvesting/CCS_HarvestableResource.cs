using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HarvestableResource
// CATEGORY: Modules / WorldResources / Runtime / Harvesting
// PURPOSE: Interactable MonoBehaviour wrapper for harvestable world resource nodes.
// PLACEMENT: Attach to primitive placeholder nodes in bootstrap verification scenes.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Integrates Interaction and Inventory services through the runtime registry in 0.5.2.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public sealed class CCS_HarvestableResource : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [Header("Resource Configuration")]
        [Tooltip("Resource definition that drives harvest rules and drops.")]
        [SerializeField] private CCS_ResourceDefinition resourceDefinition;

        [Tooltip("Optional world resource profile used when registry services are unavailable.")]
        [SerializeField] private CCS_WorldResourceProfile worldResourceProfile;

        [Header("Interaction")]
        [Tooltip("Maximum distance at which this resource accepts interaction requests.")]
        [SerializeField] private float interactionDistance = 3f;

        [Tooltip("When enabled, interaction assumes the required tool is available for harvest validation.")]
        [SerializeField] private bool assumeRequiredToolEquipped = true;

        private CCS_ResourceNodeState nodeState;
        private CCS_ResourceHarvestService harvestService;
        private CCS_ResourceRespawnService respawnService;
        private CCS_PlayerInventoryService inventoryService;
        private string nodeKey;
        private bool respawnRegistered;

        #endregion

        #region Properties

        public CCS_ResourceDefinition ResourceDefinition => resourceDefinition;

        public CCS_ResourceNodeState NodeState => nodeState;

        public CCS_ResourceHarvestService HarvestService => harvestService;

        public CCS_ResourceRespawnService RespawnService => respawnService;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            nodeKey = gameObject.name + "_" + GetInstanceID();
            ResetNode();
        }

        private void Start()
        {
            ResolveServices();
        }

        private void Update()
        {
            if (nodeState == null || respawnService == null || !respawnRegistered)
            {
                return;
            }

            respawnService.TickNode(nodeKey, Time.deltaTime);
            nodeState.RespawnRemainingSeconds = respawnService.GetRemainingSeconds(nodeKey);
        }

        #endregion

        #region Public Methods

        public string GetInteractionDisplayName()
        {
            if (resourceDefinition == null || string.IsNullOrWhiteSpace(resourceDefinition.DisplayName))
            {
                return "Resource";
            }

            return resourceDefinition.DisplayName;
        }

        public bool CanInteract()
        {
            if (resourceDefinition == null || nodeState == null || nodeState.IsDepleted)
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
            CCS_HarvestResult result = Harvest(ResolveEquippedToolType(), inventoryService);
            return result.IsSuccess;
        }

        public float GetInteractionDistance()
        {
            return interactionDistance;
        }

        public bool CanHarvest(CCS_RequiredToolType equippedToolType)
        {
            if (resourceDefinition == null || nodeState == null || harvestService == null)
            {
                return false;
            }

            CCS_HarvestRequest request = new CCS_HarvestRequest(
                resourceDefinition,
                nodeState,
                equippedToolType,
                nodeKey);

            return harvestService.CanHarvest(request);
        }

        public CCS_HarvestResult Harvest(
            CCS_RequiredToolType equippedToolType,
            CCS_PlayerInventoryService inventoryServiceOverride = null)
        {
            if (resourceDefinition == null || nodeState == null || harvestService == null)
            {
                return CCS_HarvestResult.Failure("Harvestable resource is not configured.");
            }

            CCS_PlayerInventoryService activeInventoryService =
                inventoryServiceOverride ?? inventoryService;

            CCS_HarvestRequest request = new CCS_HarvestRequest(
                resourceDefinition,
                nodeState,
                equippedToolType,
                nodeKey);

            CCS_HarvestResult result = harvestService.TryHarvest(request, activeInventoryService);

            if (result.IsSuccess && nodeState.IsDepleted && respawnService != null && !respawnRegistered)
            {
                respawnRegistered = true;
                respawnService.RegisterDepletedNode(
                    nodeKey,
                    resourceDefinition,
                    nodeState,
                    ResetNodeFromRespawn);
            }

            return result;
        }

        public void ResetNode()
        {
            if (resourceDefinition == null)
            {
                nodeState = new CCS_ResourceNodeState(0);
                respawnRegistered = false;
                respawnService?.ClearNode(nodeKey);
                return;
            }

            nodeState = CCS_ResourceNodeState.CreateFromDefinition(resourceDefinition);
            respawnRegistered = false;
            respawnService?.ClearNode(nodeKey);
        }

        public CCS_ResourceSnapshot GetSnapshot()
        {
            return new CCS_ResourceSnapshot(resourceDefinition, nodeState, nodeKey);
        }

        #endregion

        #region Private Methods

        private void ResolveServices()
        {
            if (CCS_WorldResourceRuntimeBridge.TryGetHarvestService(out CCS_ResourceHarvestService registryHarvestService))
            {
                harvestService = registryHarvestService;
            }
            else
            {
                harvestService = new CCS_ResourceHarvestService();

                if (worldResourceProfile != null)
                {
                    harvestService.InitializeFromProfile(worldResourceProfile);
                }
                else
                {
                    harvestService.Initialize();
                }
            }

            if (CCS_WorldResourceRuntimeBridge.TryGetRespawnService(out CCS_ResourceRespawnService registryRespawnService))
            {
                respawnService = registryRespawnService;
            }
            else
            {
                respawnService = new CCS_ResourceRespawnService();

                if (worldResourceProfile != null)
                {
                    respawnService.InitializeFromProfile(worldResourceProfile);
                }
                else
                {
                    respawnService.Initialize();
                }
            }

            CCS_WorldResourceRuntimeBridge.TryGetInventoryService(out inventoryService);
        }

        private CCS_RequiredToolType ResolveEquippedToolType()
        {
            if (assumeRequiredToolEquipped && resourceDefinition != null)
            {
                return resourceDefinition.RequiredToolType;
            }

            return CCS_RequiredToolType.None;
        }

        private void ResetNodeFromRespawn(CCS_ResourceNodeState _)
        {
            ResetNode();
        }

        #endregion
    }
}
