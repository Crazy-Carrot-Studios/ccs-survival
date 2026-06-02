using CCS.Modules.Storage;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HorseSaddlebagContainer
// CATEGORY: Modules / Mounts / Runtime / Components
// PURPOSE: Saddlebag storage marker that configures a CCS_StorageContainer on the mount.
// PLACEMENT: PF_CCS_Horse alongside CCS_StorageContainer and CCS_StorageContainerInteractable.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    public sealed class CCS_HorseSaddlebagContainer : MonoBehaviour
    {
        [SerializeField] private CCS_StorageContainer storageContainer;

        public CCS_StorageContainer StorageContainer =>
            storageContainer != null ? storageContainer : storageContainer = GetComponent<CCS_StorageContainer>();

        public void ConfigureForMount(
            string containerDefinitionId,
            string configuredInstanceId,
            string label,
            int slots)
        {
            CCS_StorageContainer container = StorageContainer;
            if (container == null)
            {
                return;
            }

            container.ConfigureRuntimeInstance(containerDefinitionId, configuredInstanceId, label, slots);
        }
    }
}
