using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

namespace CCS.Modules.Storage
{
    [CreateAssetMenu(
        fileName = "CCS_FrontierStorageCampProfile",
        menuName = "CCS/Survival/Storage/Frontier Storage Camp Profile")]
    public sealed class CCS_FrontierStorageCampProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_StorageContainerDefinition[] frontierStorageDefinitions =
            System.Array.Empty<CCS_StorageContainerDefinition>();

        public IReadOnlyList<CCS_StorageContainerDefinition> FrontierStorageDefinitions => frontierStorageDefinitions;

        public bool TryGetStorageByPlaceableItemId(string itemId, out CCS_StorageContainerDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(itemId) || frontierStorageDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < frontierStorageDefinitions.Length; index++)
            {
                CCS_StorageContainerDefinition candidate = frontierStorageDefinitions[index];
                if (candidate?.PlaceableKitItem != null
                    && string.Equals(candidate.PlaceableKitItem.ItemId, itemId, System.StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetStorageById(string containerDefinitionId, out CCS_StorageContainerDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(containerDefinitionId) || frontierStorageDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < frontierStorageDefinitions.Length; index++)
            {
                CCS_StorageContainerDefinition candidate = frontierStorageDefinitions[index];
                if (candidate != null
                    && string.Equals(candidate.ContainerId, containerDefinitionId, System.StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
