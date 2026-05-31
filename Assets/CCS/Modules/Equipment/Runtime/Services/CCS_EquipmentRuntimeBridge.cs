using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_EquipmentRuntimeBridge
// CATEGORY: Modules / Equipment / Runtime / Services
// PURPOSE: Resolves gameplay services from the runtime registry for equipment systems.
// PLACEMENT: Used by development test harnesses and future equipment interactions.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public static class CCS_EquipmentRuntimeBridge
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
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out equipmentService);
        }

        #endregion
    }
}
