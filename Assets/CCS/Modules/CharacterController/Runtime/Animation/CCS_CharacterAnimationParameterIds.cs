using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterAnimationParameterIds
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Central Animator parameter name and hash contract for Character Controller.
// PLACEMENT: Referenced by locomotion and future presentation bridges. Not on GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.4 Phase 3C — active locomotion hashes only; future names are design-only.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    /// <summary>
    /// Central contract for Animator parameter identifiers on the Character Controller.
    /// Only <see cref="Active"/> hashes are written at runtime in v0.7.4.
    /// </summary>
    public static class CCS_CharacterAnimationParameterIds
    {
        /// <summary>
        /// Parameters currently wired on the locomotion-only Animator Controller (v0.7.3+).
        /// </summary>
        public static class Active
        {
            public const string SpeedNormalized = "SpeedNormalized";

            public const string IsGrounded = "IsGrounded";

            public const string IsSprinting = "IsSprinting";

            public const string JumpTrigger = "JumpTrigger";

            public static readonly int SpeedNormalizedHash = Animator.StringToHash(SpeedNormalized);

            public static readonly int IsGroundedHash = Animator.StringToHash(IsGrounded);

            public static readonly int IsSprintingHash = Animator.StringToHash(IsSprinting);

            public static readonly int JumpTriggerHash = Animator.StringToHash(JumpTrigger);
        }

        /// <summary>
        /// Planned parameter names for future weapon, interaction, and additive layers.
        /// Not registered on the Animator Controller and not written at runtime in v0.7.4.
        /// </summary>
        public static class FutureDesignOnly
        {
            // Weapon / aim presentation
            public const string WeaponMode = "WeaponMode";

            public const string IsAiming = "IsAiming";

            public const string AimPitch = "AimPitch";

            public const string AimYaw = "AimYaw";

            public const string FireTrigger = "FireTrigger";

            public const string ReloadTrigger = "ReloadTrigger";

            public const string EquipTrigger = "EquipTrigger";

            public const string UnequipTrigger = "UnequipTrigger";

            // Interaction presentation
            public const string InteractionTrigger = "InteractionTrigger";

            public const string InteractionType = "InteractionType";
        }
    }
}
