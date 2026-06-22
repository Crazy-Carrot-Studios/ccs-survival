using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioSettings
// CATEGORY: Modules / CharacterController / Runtime / Equipment / Fitting
// PURPOSE: Default settings for the Equipment Fit Studio editor tool.
// PLACEMENT: ScriptableObject under Profiles/EquipmentFitting/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Editor tool configuration only.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [CreateAssetMenu(
        fileName = "CCS_EquipmentFitStudioSettings",
        menuName = "CCS/Character Controller/Equipment Fit Studio Settings",
        order = 23)]
    public sealed class CCS_EquipmentFitStudioSettings : ScriptableObject
    {
        #region Variables

        [SerializeField] private CCS_EquipmentSocketProfile defaultSocketProfile;

        [SerializeField] private GameObject defaultPreviewWeaponPrefab;

        [SerializeField] private string defaultWeaponId = CCS_EquipmentConstants.DefaultPreviewWeaponId;

        [SerializeField] private string defaultCharacterRigId = CCS_EquipmentConstants.DefaultCharacterRigId;

        [SerializeField] private float previewCameraFov = 35f;

        [SerializeField] private float previewCameraNearClip = 0.01f;

        [SerializeField] private float previewCameraFarClip = 100f;

        [SerializeField] private Color previewBackgroundColor = new Color(0.16f, 0.16f, 0.18f, 1f);

        [SerializeField] private float nudgePositionSmall = 0.01f;

        [SerializeField] private float nudgePositionLarge = 0.05f;

        [SerializeField] private float nudgeRotationSmall = 1f;

        [SerializeField] private float nudgeRotationLarge = 5f;

        [SerializeField] private bool autoFrameOnSelection = true;

        [SerializeField] private bool showHints = true;

        [SerializeField] private bool showGizmos = true;

        #endregion

        #region Properties

        public CCS_EquipmentSocketProfile DefaultSocketProfile => defaultSocketProfile;

        public GameObject DefaultPreviewWeaponPrefab => defaultPreviewWeaponPrefab;

        public string DefaultWeaponId => defaultWeaponId;

        public string DefaultCharacterRigId => defaultCharacterRigId;

        public float PreviewCameraFov => previewCameraFov;

        public float PreviewCameraNearClip => previewCameraNearClip;

        public float PreviewCameraFarClip => previewCameraFarClip;

        public Color PreviewBackgroundColor => previewBackgroundColor;

        public float NudgePositionSmall => nudgePositionSmall;

        public float NudgePositionLarge => nudgePositionLarge;

        public float NudgeRotationSmall => nudgeRotationSmall;

        public float NudgeRotationLarge => nudgeRotationLarge;

        public bool AutoFrameOnSelection => autoFrameOnSelection;

        public bool ShowHints => showHints;

        public bool ShowGizmos => showGizmos;

        #endregion
    }
}
