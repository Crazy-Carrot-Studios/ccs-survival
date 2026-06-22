using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponIKPoseProfile
// CATEGORY: Modules / CharacterController / Runtime / Equipment / Fitting
// PURPOSE: Stores IK target positions and weights for future weapon poses.
// PLACEMENT: ScriptableObject under Profiles/EquipmentFitting/IK/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Authoring only. Not consumed by gameplay in v0.6.7.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [CreateAssetMenu(
        fileName = "CCS_WeaponIKPoseProfile",
        menuName = "CCS/Character Controller/Weapon IK Pose Profile",
        order = 21)]
    public sealed class CCS_WeaponIKPoseProfile : ScriptableObject
    {
        #region Variables

        [SerializeField] private string profileId = string.Empty;

        [SerializeField] private string weaponId = string.Empty;

        [SerializeField] private string characterRigId = string.Empty;

        [SerializeField] private string poseId = string.Empty;

        [SerializeField] private Vector3 rightHandTargetLocalPosition = Vector3.zero;

        [SerializeField] private Vector3 rightHandTargetLocalEulerAngles = Vector3.zero;

        [SerializeField] private Vector3 rightElbowHintLocalPosition = Vector3.zero;

        [SerializeField] private Vector3 leftHandTargetLocalPosition = Vector3.zero;

        [SerializeField] private Vector3 leftHandTargetLocalEulerAngles = Vector3.zero;

        [SerializeField] private Vector3 leftElbowHintLocalPosition = Vector3.zero;

        [SerializeField] private Vector3 weaponAimTargetLocalPosition = Vector3.zero;

        [SerializeField] private Vector3 weaponAimTargetLocalEulerAngles = Vector3.zero;

        [SerializeField] private float rigWeight;

        [SerializeField] private float rightHandIKWeight;

        [SerializeField] private float leftHandIKWeight;

        [SerializeField] private float aimWeight;

        [TextArea(2, 6)]
        [SerializeField] private string notes = string.Empty;

        #endregion

        #region Properties

        public string ProfileId => profileId;

        public string WeaponId => weaponId;

        public string CharacterRigId => characterRigId;

        public string PoseId => poseId;

        public Vector3 RightHandTargetLocalPosition => rightHandTargetLocalPosition;

        public Vector3 RightHandTargetLocalEulerAngles => rightHandTargetLocalEulerAngles;

        public Vector3 RightElbowHintLocalPosition => rightElbowHintLocalPosition;

        public Vector3 LeftHandTargetLocalPosition => leftHandTargetLocalPosition;

        public Vector3 LeftHandTargetLocalEulerAngles => leftHandTargetLocalEulerAngles;

        public Vector3 LeftElbowHintLocalPosition => leftElbowHintLocalPosition;

        public Vector3 WeaponAimTargetLocalPosition => weaponAimTargetLocalPosition;

        public Vector3 WeaponAimTargetLocalEulerAngles => weaponAimTargetLocalEulerAngles;

        public float RigWeight => rigWeight;

        public float RightHandIKWeight => rightHandIKWeight;

        public float LeftHandIKWeight => leftHandIKWeight;

        public float AimWeight => aimWeight;

        public string Notes => notes;

        #endregion

        #region Public Methods

        public void ApplyIdentity(
            string profileIdValue,
            string weaponIdValue,
            string characterRigIdValue,
            string poseIdValue)
        {
            profileId = profileIdValue;
            weaponId = weaponIdValue;
            characterRigId = characterRigIdValue;
            poseId = poseIdValue;
        }

        #endregion
    }
}
