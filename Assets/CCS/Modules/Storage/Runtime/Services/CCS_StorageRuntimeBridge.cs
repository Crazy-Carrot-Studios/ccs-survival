using CCS.Core;
using CCS.Modules.Inventory;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_StorageRuntimeBridge
// CATEGORY: Modules / Storage / Runtime / Services
// PURPOSE: Resolves storage and inventory services from the runtime service registry.
// PLACEMENT: Used by interactables, playtest helpers, and development harnesses.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.Storage
{
    public static class CCS_StorageRuntimeBridge
    {
        #region Public Methods

        public static bool TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
        {
            runtimeHost = null;
            CCS_RuntimeHost[] runtimeHosts = CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_RuntimeHost>();
            if (runtimeHosts == null || runtimeHosts.Length == 0)
            {
                return false;
            }

            runtimeHost = runtimeHosts[0];
            return runtimeHost != null;
        }

        public static bool TryGetStorageService(out CCS_StorageService storageService)
        {
            storageService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out storageService);
        }

        public static bool TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
        {
            inventoryService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out inventoryService);
        }

        #endregion
    }
}
