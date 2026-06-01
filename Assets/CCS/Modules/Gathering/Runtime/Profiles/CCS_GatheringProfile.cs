using System;
using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_GatheringProfile
// CATEGORY: Modules / Gathering / Runtime / Profiles
// PURPOSE: Tuning profile for primitive gathering nodes, rewards, and respawn rules.
// PLACEMENT: Assets/CCS/Survival/Profiles/Gathering/ (project shell configuration).
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: No tool requirements or animation systems in 0.9.9 foundation.
// =============================================================================

namespace CCS.Modules.Gathering
{
    [CreateAssetMenu(
        fileName = "CCS_GatheringProfile",
        menuName = "CCS/Survival/Gathering/Gathering Profile")]
    public sealed class CCS_GatheringProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Interaction")]
        [Tooltip("Maximum distance used by gathering interactables for player validation.")]
        [SerializeField] private float nodeInteractionDistance = 3f;

        [Tooltip("Seconds required to complete a gather action. Zero gathers instantly.")]
        [SerializeField] private float gatherDurationSeconds;

        [Header("Respawn")]
        [Tooltip("When enabled, depleted gathering nodes respawn after the configured delay.")]
        [SerializeField] private bool respawnEnabled = true;

        [Tooltip("Seconds before a depleted node becomes available again.")]
        [SerializeField] private float respawnDelaySeconds = 30f;

        [Header("Rewards")]
        [Tooltip("Item definitions used to resolve reward itemDefinitionId strings.")]
        [SerializeField] private CCS_ItemDefinition[] rewardItemCatalog;

        [Tooltip("Reward tables keyed by gathering node type.")]
        [SerializeField] private CCS_GatheringNodeRewardSettings[] nodeRewardSettings =
        {
            new CCS_GatheringNodeRewardSettings
            {
                nodeType = CCS_GatheringNodeType.SmallTree,
                rewards = new[]
                {
                    new CCS_GatheringReward
                    {
                        resourceType = CCS_GatheringResourceType.Stick,
                        itemDefinitionId = "ccs.survival.item.resource.stick",
                        amount = 2
                    },
                    new CCS_GatheringReward
                    {
                        resourceType = CCS_GatheringResourceType.Wood,
                        itemDefinitionId = "ccs.survival.item.resource.wood",
                        amount = 1
                    }
                }
            },
            new CCS_GatheringNodeRewardSettings
            {
                nodeType = CCS_GatheringNodeType.Rock,
                rewards = new[]
                {
                    new CCS_GatheringReward
                    {
                        resourceType = CCS_GatheringResourceType.Stone,
                        itemDefinitionId = "ccs.survival.item.resource.stone",
                        amount = 2
                    }
                }
            },
            new CCS_GatheringNodeRewardSettings
            {
                nodeType = CCS_GatheringNodeType.Bush,
                rewards = new[]
                {
                    new CCS_GatheringReward
                    {
                        resourceType = CCS_GatheringResourceType.PlantFiber,
                        itemDefinitionId = "ccs.survival.item.resource.fiber",
                        amount = 2
                    },
                    new CCS_GatheringReward
                    {
                        resourceType = CCS_GatheringResourceType.Stick,
                        itemDefinitionId = "ccs.survival.item.resource.stick",
                        amount = 1
                    }
                }
            }
        };

        private Dictionary<string, CCS_ItemDefinition> rewardDefinitionsById;

        #endregion

        #region Properties

        public float NodeInteractionDistance => nodeInteractionDistance;

        public float GatherDurationSeconds => gatherDurationSeconds;

        public bool RespawnEnabled => respawnEnabled;

        public float RespawnDelaySeconds => respawnDelaySeconds;

        public IReadOnlyList<CCS_GatheringNodeRewardSettings> NodeRewardSettings => nodeRewardSettings;

        #endregion

        #region Public Methods

        public void BuildRewardLookup()
        {
            rewardDefinitionsById = new Dictionary<string, CCS_ItemDefinition>(StringComparer.OrdinalIgnoreCase);
            if (rewardItemCatalog == null)
            {
                return;
            }

            for (int index = 0; index < rewardItemCatalog.Length; index++)
            {
                CCS_ItemDefinition itemDefinition = rewardItemCatalog[index];
                if (itemDefinition == null || string.IsNullOrWhiteSpace(itemDefinition.ItemId))
                {
                    continue;
                }

                rewardDefinitionsById[itemDefinition.ItemId] = itemDefinition;
            }
        }

        public bool TryGetRewards(
            CCS_GatheringNodeType nodeType,
            out CCS_GatheringReward[] rewards)
        {
            rewards = null;
            if (nodeRewardSettings == null)
            {
                return false;
            }

            for (int index = 0; index < nodeRewardSettings.Length; index++)
            {
                CCS_GatheringNodeRewardSettings settings = nodeRewardSettings[index];
                if (settings.nodeType != nodeType)
                {
                    continue;
                }

                rewards = settings.rewards;
                return rewards != null && rewards.Length > 0;
            }

            return false;
        }

        public bool TryResolveItemDefinition(string itemDefinitionId, out CCS_ItemDefinition itemDefinition)
        {
            itemDefinition = null;
            if (string.IsNullOrWhiteSpace(itemDefinitionId))
            {
                return false;
            }

            if (rewardDefinitionsById == null)
            {
                BuildRewardLookup();
            }

            return rewardDefinitionsById != null
                && rewardDefinitionsById.TryGetValue(itemDefinitionId, out itemDefinition)
                && itemDefinition != null;
        }

        #endregion
    }

    [Serializable]
    public struct CCS_GatheringNodeRewardSettings
    {
        [Tooltip("Gathering node type that uses this reward table.")]
        public CCS_GatheringNodeType nodeType;

        [Tooltip("Rewards granted when this node type is gathered.")]
        public CCS_GatheringReward[] rewards;
    }
}
