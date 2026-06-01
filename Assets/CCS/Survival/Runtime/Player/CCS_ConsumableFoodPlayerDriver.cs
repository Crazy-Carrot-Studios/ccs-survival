using CCS.Modules.Cooking;
using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_ConsumableFoodPlayerDriver
// CATEGORY: Survival / Runtime / Player
// PURPOSE: Lets the player consume configured food items and restore hunger.
// PLACEMENT: PF_CCS_Player alongside CCS_InteractionPlayerDriver.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Uses F to consume the first available configured food item.
// =============================================================================

namespace CCS.Survival.Player
{
    [DefaultExecutionOrder(221)]
    public sealed class CCS_ConsumableFoodPlayerDriver : MonoBehaviour
    {
        #region Variables

        private CCS_ConsumableFoodService consumableFoodService;
        private bool serviceResolved;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!ResolveService())
            {
                return;
            }

            if (Keyboard.current == null || !Keyboard.current.fKey.wasPressedThisFrame)
            {
                return;
            }

            consumableFoodService.TryConsumeFirstAvailableFood();
        }

        #endregion

        #region Private Methods

        private bool ResolveService()
        {
            if (serviceResolved && consumableFoodService != null)
            {
                return true;
            }

            serviceResolved = CCS_CookingRuntimeBridge.TryGetConsumableFoodService(out consumableFoodService)
                && consumableFoodService != null
                && consumableFoodService.IsInitialized;

            return serviceResolved;
        }

        #endregion
    }
}
