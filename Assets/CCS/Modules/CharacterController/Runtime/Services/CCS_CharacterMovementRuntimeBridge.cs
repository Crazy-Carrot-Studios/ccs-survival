using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_CharacterMovementRuntimeBridge
// CATEGORY: Modules / CharacterController / Runtime / Services
// PURPOSE: Resolves character movement service from the runtime registry.
// PLACEMENT: Used by player composition and future gameplay systems.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Safe when runtime host or movement service is unavailable.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CharacterMovementRuntimeBridge
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

        public static bool TryGetCharacterMovementService(out CCS_CharacterMovementService movementService)
        {
            movementService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost.ServiceRegistry == null)
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out movementService);
        }

        #endregion
    }
}
