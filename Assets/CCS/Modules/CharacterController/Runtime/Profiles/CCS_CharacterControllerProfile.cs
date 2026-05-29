using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerProfile
// CATEGORY: Modules / CharacterController / Runtime / Profiles
// PURPOSE: Project tuning profile for character movement, capsule, camera, and stamina hooks.
// PLACEMENT: Assets/CCS/Survival/Profiles/CharacterController/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Configuration only. No Rigidbody movement. Root motion OFF on Animator.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [CreateAssetMenu(
        fileName = "CCS_CharacterControllerProfile",
        menuName = "CCS/Survival/Character Controller/Controller Profile")]
    public sealed class CCS_CharacterControllerProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Movement")]
        [Tooltip("Locomotion speeds, jump, gravity, and CharacterController capsule settings.")]
        [SerializeField] private CCS_CharacterMovementProfile movement = new CCS_CharacterMovementProfile();

        [Header("Camera Look")]
        [Tooltip("Mouse/gamepad look sensitivity and pitch clamp.")]
        [SerializeField] private CCS_CharacterCameraProfile camera = new CCS_CharacterCameraProfile();

        #endregion

        #region Properties

        public CCS_CharacterMovementProfile Movement => movement;

        public CCS_CharacterCameraProfile Camera => camera;

        #endregion
    }
}
