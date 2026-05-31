using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HarvestableResource
// CATEGORY: Modules / WorldResources / Runtime / Harvesting
// PURPOSE: MonoBehaviour wrapper for harvestable world resource nodes.
// PLACEMENT: Attach to primitive placeholder nodes in bootstrap verification scenes.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No visual destruction effects, UI, or save integration in 0.5.1 foundation.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public sealed class CCS_HarvestableResource : MonoBehaviour
    {
        #region Variables

        [Header("Resource Configuration")]
        [Tooltip("Resource definition that drives harvest rules and drops.")]
        [SerializeField] private CCS_ResourceDefinition resourceDefinition;

        [Tooltip("Optional world resource profile. Uses defaults when unassigned.")]
        [SerializeField] private CCS_WorldResourceProfile worldResourceProfile;

        private CCS_ResourceNodeState nodeState;
        private CCS_ResourceHarvestService harvestService;
        private CCS_ResourceRespawnService respawnService;
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
            harvestService = new CCS_ResourceHarvestService();
            respawnService = new CCS_ResourceRespawnService();

            if (worldResourceProfile != null)
            {
                harvestService.InitializeFromProfile(worldResourceProfile);
                respawnService.InitializeFromProfile(worldResourceProfile);
            }
            else
            {
                harvestService.Initialize();
                respawnService.Initialize();
            }

            ResetNode();
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
            CCS_PlayerInventoryService inventoryService = null)
        {
            if (resourceDefinition == null || nodeState == null || harvestService == null)
            {
                return CCS_HarvestResult.Failure("Harvestable resource is not configured.");
            }

            CCS_HarvestRequest request = new CCS_HarvestRequest(
                resourceDefinition,
                nodeState,
                equippedToolType,
                nodeKey);

            CCS_HarvestResult result = harvestService.TryHarvest(request, inventoryService);

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

        private void ResetNodeFromRespawn(CCS_ResourceNodeState _)
        {
            ResetNode();
        }

        #endregion
    }
}
