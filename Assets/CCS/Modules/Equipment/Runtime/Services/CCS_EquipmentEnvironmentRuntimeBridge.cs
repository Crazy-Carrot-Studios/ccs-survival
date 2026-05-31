using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_EquipmentEnvironmentRuntimeBridge
// CATEGORY: Modules / Equipment / Runtime / Services
// PURPOSE: Resolves equipment service from the runtime registry for environment systems.
// PLACEMENT: Used by environment effects and future survival integration tools.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public static class CCS_EquipmentEnvironmentRuntimeBridge
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

        public static bool TryGetEquipmentService(out CCS_PlayerEquipmentService equipmentService)
        {
            equipmentService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost.ServiceRegistry == null)
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out equipmentService);
        }

        #endregion
    }
}
