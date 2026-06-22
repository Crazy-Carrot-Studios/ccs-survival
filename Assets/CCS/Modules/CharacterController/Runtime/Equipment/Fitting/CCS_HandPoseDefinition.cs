using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HandPoseDefinition
// CATEGORY: Modules / CharacterController / Runtime / Equipment / Fitting
// PURPOSE: Foundation asset for future finger pose tuning.
// PLACEMENT: ScriptableObject under Profiles/EquipmentFitting/HandPoses/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Authoring only. Runtime finger posing not wired in v0.6.7.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public enum CCS_HandPoseSide
    {
        Right = 0,
        Left = 1,
    }

    [CreateAssetMenu(
        fileName = "CCS_HandPoseDefinition",
        menuName = "CCS/Character Controller/Hand Pose Definition",
        order = 22)]
    public sealed class CCS_HandPoseDefinition : ScriptableObject
    {
        #region Variables

        [SerializeField] private string poseId = string.Empty;

        [SerializeField] private string weaponId = string.Empty;

        [SerializeField] private string characterRigId = string.Empty;

        [SerializeField] private CCS_HandPoseSide handSide = CCS_HandPoseSide.Right;

        [SerializeField] private float thumbCurl;

        [SerializeField] private float indexCurl;

        [SerializeField] private float middleCurl;

        [SerializeField] private float ringCurl;

        [SerializeField] private float littleCurl;

        [SerializeField] private float fingerSpread;

        [SerializeField] private Vector3 wristLocalEulerOffset = Vector3.zero;

        [SerializeField] private List<Vector3> fingerBoneRotations = new List<Vector3>();

        [SerializeField] private float blendInSeconds = 0.15f;

        [SerializeField] private float blendOutSeconds = 0.15f;

        [TextArea(2, 6)]
        [SerializeField] private string notes = string.Empty;

        #endregion

        #region Properties

        public string PoseId => poseId;

        public string WeaponId => weaponId;

        public string CharacterRigId => characterRigId;

        public CCS_HandPoseSide HandSide => handSide;

        public float ThumbCurl => thumbCurl;

        public float IndexCurl => indexCurl;

        public float MiddleCurl => middleCurl;

        public float RingCurl => ringCurl;

        public float LittleCurl => littleCurl;

        public float FingerSpread => fingerSpread;

        public Vector3 WristLocalEulerOffset => wristLocalEulerOffset;

        public IReadOnlyList<Vector3> FingerBoneRotations => fingerBoneRotations;

        public float BlendInSeconds => blendInSeconds;

        public float BlendOutSeconds => blendOutSeconds;

        public string Notes => notes;

        #endregion
    }
}
