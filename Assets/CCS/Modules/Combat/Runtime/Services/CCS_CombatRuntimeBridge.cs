using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_CombatRuntimeBridge
// CATEGORY: Modules / Combat / Runtime / Services
// PURPOSE: Resolves combat and equipment services from the runtime registry.
// PLACEMENT: Used by CCS_PlayerCombatDriver and wildlife bootstrap helpers.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.Combat
{
    public static class CCS_CombatRuntimeBridge
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

        public static bool TryGetCombatService(out CCS_CombatService combatService)
        {
            combatService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out combatService);
        }

        #endregion
    }
}
