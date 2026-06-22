using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponAttachmentFitProfile
// CATEGORY: Modules / CharacterController / Runtime / Equipment / Fitting
// PURPOSE: Stores weapon/character/socket-specific fit offsets for future visuals.
// PLACEMENT: ScriptableObject under Profiles/EquipmentFitting/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Authoring only. Not consumed by gameplay in v0.6.7.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [CreateAssetMenu(
        fileName = "CCS_WeaponAttachmentFitProfile",
        menuName = "CCS/Character Controller/Weapon Attachment Fit Profile",
        order = 20)]
    public sealed class CCS_WeaponAttachmentFitProfile : ScriptableObject
    {
        #region Variables

        [SerializeField] private string profileId = string.Empty;

        [SerializeField] private string weaponId = string.Empty;

        [SerializeField] private string characterRigId = string.Empty;

        [SerializeField] private string socketId = string.Empty;

        [SerializeField] private Vector3 socketLocalPosition = Vector3.zero;

        [SerializeField] private Vector3 socketLocalEulerAngles = Vector3.zero;

        [SerializeField] private Vector3 socketLocalScale = Vector3.one;

        [TextArea(2, 6)]
        [SerializeField] private string notes = string.Empty;

        #endregion

        #region Properties

        public string ProfileId => profileId;

        public string WeaponId => weaponId;

        public string CharacterRigId => characterRigId;

        public string SocketId => socketId;

        public Vector3 SocketLocalPosition => socketLocalPosition;

        public Vector3 SocketLocalEulerAngles => socketLocalEulerAngles;

        public Vector3 SocketLocalScale => socketLocalScale;

        public string Notes => notes;

        #endregion

        #region Public Methods

        public void ApplySocketTransform(string profileIdValue, Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            profileId = profileIdValue;
            socketLocalPosition = position;
            socketLocalEulerAngles = eulerAngles;
            socketLocalScale = scale;
        }

        public void SetIdentity(
            string profileIdValue,
            string weaponIdValue,
            string characterRigIdValue,
            string socketIdValue)
        {
            profileId = profileIdValue;
            weaponId = weaponIdValue;
            characterRigId = characterRigIdValue;
            socketId = socketIdValue;
        }

        #endregion
    }
}
