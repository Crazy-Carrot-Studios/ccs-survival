using CCS.Modules.Storage;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WagonCargoContainer
// CATEGORY: Modules / Vehicles / Runtime / Components
// PURPOSE: Wagon cargo storage marker that configures CCS_StorageContainer on the vehicle.
// PLACEMENT: PF_CCS_FrontierWagon alongside CCS_StorageContainer and interactable.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Vehicles
{
    public sealed class CCS_WagonCargoContainer : MonoBehaviour
    {
        [SerializeField] private CCS_StorageContainer storageContainer;

        public CCS_StorageContainer StorageContainer =>
            storageContainer != null ? storageContainer : storageContainer = GetComponent<CCS_StorageContainer>();

        public void ConfigureForVehicle(
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
