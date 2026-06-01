using CCS.Modules.CharacterController;
using CCS.Modules.Combat;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerCombatDriver
// CATEGORY: Survival / Runtime / Player
// PURPOSE: Drives primary melee attacks through CCS_CombatService from the player camera.
// PLACEMENT: PF_CCS_Player alongside CCS_InteractionPlayerDriver.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Uses Gameplay/PrimaryAction input. No crosshair or world damage numbers.
// =============================================================================

namespace CCS.Survival.Player
{
    [DefaultExecutionOrder(225)]
    public sealed class CCS_PlayerCombatDriver : MonoBehaviour
    {
        #region Variables

        [Header("Combat Attack")]
        [Tooltip("Camera used for forward melee sphere casts. Defaults to child camera.")]
        [SerializeField] private Camera combatCamera;

        private CCS_CharacterInputActionProvider inputProvider;
        private CCS_CombatService combatService;
        private bool serviceResolved;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            inputProvider = GetComponent<CCS_CharacterInputActionProvider>();

            if (combatCamera == null)
            {
                combatCamera = GetComponentInChildren<Camera>();
            }
        }

        private void Update()
        {
            if (!serviceResolved)
            {
                serviceResolved = CCS_CombatRuntimeBridge.TryGetCombatService(out combatService)
                    && combatService != null
                    && combatService.IsInitialized;
            }

            if (!serviceResolved || combatCamera == null || inputProvider == null)
            {
                return;
            }

            if (!inputProvider.PrimaryActionPressedThisFrame)
            {
                return;
            }

            Transform cameraTransform = combatCamera.transform;
            combatService.TryMeleeAttack(cameraTransform.position, cameraTransform.forward);
        }

        #endregion
    }
}
