using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentVisualDefinition
// CATEGORY: Modules / Equipment / Runtime / Visuals
// PURPOSE: Maps inventory item IDs to primitive visual prefabs and attachment sockets.
// PLACEMENT: Referenced by CCS_EquipmentVisualProfile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Data-driven only. No hard-coded item IDs in player scripts.
// =============================================================================

namespace CCS.Modules.Equipment
{
    [CreateAssetMenu(
        fileName = "CCS_EquipmentVisualDefinition",
        menuName = "CCS/Survival/Equipment/Equipment Visual Definition")]
    public sealed class CCS_EquipmentVisualDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Inventory item ID that triggers this equipped visual.")]
        [SerializeField] private string itemId = string.Empty;

        [Header("Visual")]
        [Tooltip("Prefab spawned when the item is equipped.")]
        [SerializeField] private GameObject visualPrefab;

        [Tooltip("Attachment socket used for this visual.")]
        [SerializeField] private CCS_EquipmentAttachmentSocketType attachmentSocket =
            CCS_EquipmentAttachmentSocketType.RightHand;

        [Header("Local Offset")]
        [SerializeField] private Vector3 localPositionOffset = Vector3.zero;

        [SerializeField] private Vector3 localEulerOffset = Vector3.zero;

        [SerializeField] private Vector3 localScale = Vector3.one;

        [Header("Behavior")]
        [Tooltip("When false, the visual may remain hidden until explicitly shown by future systems.")]
        [SerializeField] private bool hideWhenUnequipped = true;

        [Header("Future Placeholders")]
        [SerializeField] private bool supportsAimPose;

        [SerializeField] private bool supportsTwoHandIk;

        [SerializeField] private bool supportsHolsterVisual;

        #endregion

        #region Properties

        public string ItemId => itemId;

        public GameObject VisualPrefab => visualPrefab;

        public CCS_EquipmentAttachmentSocketType AttachmentSocket => attachmentSocket;

        public Vector3 LocalPositionOffset => localPositionOffset;

        public Vector3 LocalEulerOffset => localEulerOffset;

        public Vector3 LocalScale => localScale;

        public bool HideWhenUnequipped => hideWhenUnequipped;

        public bool SupportsAimPose => supportsAimPose;

        public bool SupportsTwoHandIk => supportsTwoHandIk;

        public bool SupportsHolsterVisual => supportsHolsterVisual;

        #endregion
    }
}
