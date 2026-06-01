using CCS.Core;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeRuntimeBridge
// CATEGORY: Modules / Wildlife / Runtime / Services
// PURPOSE: Resolves gameplay services from the runtime registry for wildlife harvesting.
// PLACEMENT: Used by CCS_HarvestableWildlife and development test harnesses.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public static class CCS_WildlifeRuntimeBridge
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

        public static bool TryGetHarvestService(out CCS_WildlifeHarvestService harvestService)
        {
            harvestService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out harvestService);
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

        public static bool TryGetInteractionService(out CCS_InteractionService interactionService)
        {
            interactionService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out interactionService);
        }

        public static bool TryGetAiService(out CCS_WildlifeAiService aiService)
        {
            aiService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out aiService);
        }

        public static bool TryResolvePlayerTransform(out Transform playerTransform)
        {
            playerTransform = null;
            UnityEngine.CharacterController[] characterControllers =
                CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<UnityEngine.CharacterController>();
            if (characterControllers == null || characterControllers.Length == 0)
            {
                return false;
            }

            for (int index = 0; index < characterControllers.Length; index++)
            {
                UnityEngine.CharacterController characterController = characterControllers[index];
                if (characterController == null)
                {
                    continue;
                }

                if (string.Equals(
                        characterController.gameObject.name,
                        "PF_CCS_Player",
                        System.StringComparison.Ordinal))
                {
                    playerTransform = characterController.transform;
                    return true;
                }
            }

            playerTransform = characterControllers[0].transform;
            return playerTransform != null;
        }

        #endregion
    }
}
