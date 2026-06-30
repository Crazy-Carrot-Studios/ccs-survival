using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterAnimationParameterIds
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Central Animator parameter name and hash contract for Character Controller.
// PLACEMENT: Referenced by locomotion and future presentation bridges. Not on GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.8 adds single-revolver aim presentation parameters on SingleRevolverUpperBody layer.
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
        /// Parameters currently wired on the player Animator Controller (locomotion + single-revolver aim presentation).
        /// </summary>
        public static class Active
        {
            public const string SpeedNormalized = "SpeedNormalized";

            public const string IsGrounded = "IsGrounded";

            public const string IsSprinting = "IsSprinting";

            public const string JumpTrigger = "JumpTrigger";

            public const string IsAiming = "IsAiming";

            public const string RevolverDrawTrigger = "RevolverDrawTrigger";

            public const string RevolverHolsterTrigger = "RevolverHolsterTrigger";

            public static readonly int SpeedNormalizedHash = Animator.StringToHash(SpeedNormalized);

            public static readonly int IsGroundedHash = Animator.StringToHash(IsGrounded);

            public static readonly int IsSprintingHash = Animator.StringToHash(IsSprinting);

            public static readonly int JumpTriggerHash = Animator.StringToHash(JumpTrigger);

            public static readonly int IsAimingHash = Animator.StringToHash(IsAiming);

            public static readonly int RevolverDrawTriggerHash = Animator.StringToHash(RevolverDrawTrigger);

            public static readonly int RevolverHolsterTriggerHash = Animator.StringToHash(RevolverHolsterTrigger);
        }

        /// <summary>
        /// Planned parameter names for future weapon, interaction, and additive layers.
        /// Not registered on the Animator Controller and not written at runtime in v0.7.4.
        /// </summary>
        public static class FutureDesignOnly
        {
            // Weapon / aim presentation
            public const string WeaponMode = "WeaponMode";

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
