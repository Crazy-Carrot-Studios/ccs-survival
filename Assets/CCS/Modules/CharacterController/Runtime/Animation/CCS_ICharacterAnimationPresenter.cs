// =============================================================================
// SCRIPT: CCS_ICharacterAnimationPresenter
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Presentation-only boundary between gameplay and Character Animator layers.
// PLACEMENT: Implemented by future presentation bridges. Not wired in v0.7.4.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Gameplay systems must not depend on this interface until rebuild milestones ship.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    /// <summary>
    /// Future presentation bridge for Character Controller Animator layers.
    /// Implementations translate gameplay state into visual Animator writes only.
    /// </summary>
    public interface CCS_ICharacterAnimationPresenter
    {
        void SetLocomotion(float speedNormalized, bool isGrounded, bool isSprinting);

        void SetGrounded(bool isGrounded);

        void TriggerJump();

        void SetWeaponMode(CCS_CharacterWeaponAnimationMode mode);

        void SetAimingPresentation(bool isAiming);

        void TriggerInteractionPresentation(int interactionTypeId);

        void TriggerFirePresentation();

        void TriggerReloadPresentation();
    }
}
