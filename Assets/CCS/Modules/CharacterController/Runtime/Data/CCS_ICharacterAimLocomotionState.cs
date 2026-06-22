// =============================================================================
// SCRIPT: CCS_ICharacterAimLocomotionState
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Read-only aim locomotion state for decoupled gameplay modules.
// PLACEMENT: Interface. Implemented by CCS_CharacterAimLocomotionController.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Weapons and HUD read this without owning movement or camera aim logic.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_ICharacterAimLocomotionState
    {
        bool IsAimMovementActive { get; }
    }
}
