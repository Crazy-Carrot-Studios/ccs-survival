using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraProfileSet
// CATEGORY: Modules / CharacterController / Runtime / Profiles
// PURPOSE: Groups camera profiles and selects the active default mode.
// PLACEMENT: ScriptableObject asset under Profiles/Camera/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.9 defaults to ThirdPersonSurvival; FirstPersonAim used while firearm aiming.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [CreateAssetMenu(
        fileName = "CCS_DefaultCharacterCameraProfileSet",
        menuName = "CCS/Character Controller/Camera Profile Set",
        order = 2)]
    public sealed class CCS_CharacterCameraProfileSet : ScriptableObject
    {
        #region Variables

        [Header("Default")]
        [Tooltip("Active default camera profile.")]
        [SerializeField] private CCS_CharacterCameraProfile defaultProfile;

        [Header("Future Slots")]
        [SerializeField] private CCS_CharacterCameraProfile thirdPersonSurvivalProfile;

        [SerializeField] private CCS_CharacterCameraProfile firstPersonProfile;

        [SerializeField] private CCS_CharacterCameraProfile firstPersonAimProfile;

        [SerializeField] private CCS_CharacterCameraProfile aimOverShoulderProfile;

        [SerializeField] private CCS_CharacterCameraProfile topDownProfile;

        [SerializeField] private CCS_CharacterCameraProfile horseProfile;

        [SerializeField] private CCS_CharacterCameraProfile vehicleProfile;

        #endregion

        #region Properties

        public CCS_CharacterCameraProfile DefaultProfile => defaultProfile;

        public CCS_CharacterCameraProfile ThirdPersonSurvivalProfile => thirdPersonSurvivalProfile;

        public CCS_CharacterCameraProfile FirstPersonProfile => firstPersonProfile;

        public CCS_CharacterCameraProfile FirstPersonAimProfile => firstPersonAimProfile;

        public CCS_CharacterCameraProfile AimOverShoulderProfile => aimOverShoulderProfile;

        public CCS_CharacterCameraProfile TopDownProfile => topDownProfile;

        public CCS_CharacterCameraProfile HorseProfile => horseProfile;

        public CCS_CharacterCameraProfile VehicleProfile => vehicleProfile;

        #endregion

        #region Public Methods

        public CCS_CharacterCameraProfile ResolveActiveProfile(CCS_CharacterCameraMode cameraMode)
        {
            switch (cameraMode)
            {
                case CCS_CharacterCameraMode.ThirdPersonSurvival:
                    return thirdPersonSurvivalProfile != null ? thirdPersonSurvivalProfile : defaultProfile;
                case CCS_CharacterCameraMode.FirstPerson:
                case CCS_CharacterCameraMode.FirstPersonBodyAware:
                    return firstPersonProfile != null ? firstPersonProfile : defaultProfile;
                case CCS_CharacterCameraMode.FirstPersonAim:
                    return firstPersonAimProfile != null ? firstPersonAimProfile : defaultProfile;
                case CCS_CharacterCameraMode.AimOverShoulder:
                    return aimOverShoulderProfile != null ? aimOverShoulderProfile : defaultProfile;
                case CCS_CharacterCameraMode.TopDown:
                    return topDownProfile != null ? topDownProfile : defaultProfile;
                case CCS_CharacterCameraMode.Horse:
                    return horseProfile != null ? horseProfile : defaultProfile;
                case CCS_CharacterCameraMode.Vehicle:
                    return vehicleProfile != null ? vehicleProfile : defaultProfile;
                default:
                    return defaultProfile;
            }
        }

        #endregion
    }
}
