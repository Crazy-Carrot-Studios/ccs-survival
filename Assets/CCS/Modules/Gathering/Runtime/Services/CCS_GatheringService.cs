using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_GatheringService
// CATEGORY: Modules / Gathering / Runtime / Services
// PURPOSE: Registers gathering nodes, resolves rewards, and grants inventory items.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration from gathering profile.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Inventory integration uses CCS_PlayerInventoryService public APIs only.
// =============================================================================

namespace CCS.Modules.Gathering
{
    public sealed class CCS_GatheringService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_GatheringService]";

        #region Variables

        private readonly HashSet<CCS_GatheringNode> registeredNodes = new HashSet<CCS_GatheringNode>();
        private CCS_GatheringProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private bool isInitialized;

        #endregion

        #region Events

        public event GatheringNodeGatheredHandler GatheringNodeGathered;
        public event GatheringNodeDepletedHandler GatheringNodeDepleted;
        public event GatheringNodeRespawnedHandler GatheringNodeRespawned;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_GatheringProfile ActiveProfile => activeProfile;

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

        public void InitializeFromProfile(CCS_GatheringProfile profile)
        {
            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_GatheringValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            profile.BuildRewardLookup();
            activeProfile = profile;
            isInitialized = true;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public void RegisterNode(CCS_GatheringNode node)
        {
            if (node == null)
            {
                return;
            }

            registeredNodes.Add(node);
        }

        public void UnregisterNode(CCS_GatheringNode node)
        {
            if (node == null)
            {
                return;
            }

            registeredNodes.Remove(node);
        }

        public CCS_GatheringResult TryGatherNode(CCS_GatheringNode node)
        {
            if (!isInitialized || activeProfile == null)
            {
                return CCS_GatheringResult.Failure("Gathering service is unavailable.", node);
            }

            if (node == null)
            {
                return CCS_GatheringResult.Failure("Gathering node is null.");
            }

            if (!node.CanGather())
            {
                return CCS_GatheringResult.Failure("Gathering node is not available.", node);
            }

            if (!activeProfile.TryGetRewards(node.NodeType, out CCS_GatheringReward[] rewardTemplate))
            {
                return CCS_GatheringResult.Failure("Gathering rewards are not configured for this node.", node);
            }

            List<CCS_GatheringReward> grantedRewards = new List<CCS_GatheringReward>(rewardTemplate.Length);
            if (!TryGrantRewards(rewardTemplate, grantedRewards, out string grantFailureMessage))
            {
                return CCS_GatheringResult.Failure(grantFailureMessage, node);
            }

            node.Deplete(activeProfile);
            CCS_GatheringResult success = CCS_GatheringResult.Success(
                node,
                node.NodeType,
                grantedRewards,
                "Gathering completed.");

            if (activeProfile.EnableDebugLogs)
            {
                Debug.Log($"{LogPrefix} Gathered {node.NodeType} at {node.transform.position}.");
            }

            GatheringNodeGathered?.Invoke(new CCS_GatheringEventArgs(success));

            if (!node.IsAvailable)
            {
                GatheringNodeDepleted?.Invoke(new CCS_GatheringEventArgs(success));
            }

            return success;
        }

        public void NotifyNodeRespawned(CCS_GatheringNode node)
        {
            if (node == null)
            {
                return;
            }

            CCS_GatheringResult respawnResult = CCS_GatheringResult.Success(
                node,
                node.NodeType,
                null,
                "Gathering node respawned.");

            if (activeProfile != null && activeProfile.EnableDebugLogs)
            {
                Debug.Log($"{LogPrefix} {node.NodeType} respawned at {node.transform.position}.");
            }

            GatheringNodeRespawned?.Invoke(new CCS_GatheringEventArgs(respawnResult));
        }

        public CCS_GatheringNodeSaveState[] CaptureNodeStates()
        {
            if (registeredNodes.Count == 0)
            {
                return System.Array.Empty<CCS_GatheringNodeSaveState>();
            }

            System.Collections.Generic.List<CCS_GatheringNodeSaveState> captured =
                new System.Collections.Generic.List<CCS_GatheringNodeSaveState>(registeredNodes.Count);

            foreach (CCS_GatheringNode node in registeredNodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.SaveNodeId))
                {
                    continue;
                }

                captured.Add(node.CaptureSaveState());
            }

            return captured.ToArray();
        }

        public void ApplyNodeStates(CCS_GatheringNodeSaveState[] nodeStates)
        {
            if (nodeStates == null || nodeStates.Length == 0)
            {
                return;
            }

            for (int index = 0; index < nodeStates.Length; index++)
            {
                CCS_GatheringNodeSaveState record = nodeStates[index];
                if (record == null || string.IsNullOrWhiteSpace(record.nodeId))
                {
                    continue;
                }

                foreach (CCS_GatheringNode node in registeredNodes)
                {
                    if (node != null && node.MatchesSaveNodeId(record.nodeId))
                    {
                        node.ApplySaveState(record.isAvailable, record.respawnTimer);
                        break;
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private bool TryGrantRewards(
            CCS_GatheringReward[] rewardTemplate,
            List<CCS_GatheringReward> grantedRewards,
            out string failureMessage)
        {
            failureMessage = string.Empty;
            if (rewardTemplate == null || rewardTemplate.Length == 0)
            {
                failureMessage = "Gathering reward list is empty.";
                return false;
            }

            if (inventoryService != null && !inventoryService.IsInitialized)
            {
                failureMessage = "Inventory service is not initialized.";
                return false;
            }

            for (int index = 0; index < rewardTemplate.Length; index++)
            {
                CCS_GatheringReward reward = rewardTemplate[index];
                if (reward.amount <= 0 || string.IsNullOrWhiteSpace(reward.itemDefinitionId))
                {
                    failureMessage = "Gathering reward entry is invalid.";
                    return false;
                }

                if (!activeProfile.TryResolveItemDefinition(reward.itemDefinitionId, out CCS_ItemDefinition itemDefinition))
                {
                    failureMessage = $"Gathering reward item '{reward.itemDefinitionId}' could not be resolved.";
                    return false;
                }

                if (inventoryService != null)
                {
                    if (!inventoryService.CanAdd(itemDefinition, reward.amount))
                    {
                        failureMessage = "Inventory cannot hold gathered items.";
                        return false;
                    }
                }

                grantedRewards.Add(reward);
            }

            for (int index = 0; index < grantedRewards.Count; index++)
            {
                CCS_GatheringReward reward = grantedRewards[index];
                if (!activeProfile.TryResolveItemDefinition(reward.itemDefinitionId, out CCS_ItemDefinition itemDefinition))
                {
                    failureMessage = $"Gathering reward item '{reward.itemDefinitionId}' could not be resolved.";
                    return false;
                }

                if (inventoryService == null)
                {
                    continue;
                }

                int added = inventoryService.AddItem(itemDefinition, reward.amount);
                if (added < reward.amount)
                {
                    failureMessage = "Inventory cannot hold gathered items.";
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
