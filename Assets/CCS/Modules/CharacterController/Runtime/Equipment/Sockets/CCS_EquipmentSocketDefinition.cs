using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentSocketDefinition
// CATEGORY: Modules / CharacterController / Runtime / Equipment / Sockets
// PURPOSE: Defines one equipment socket's bone parent, offsets, and item filters.
// PLACEMENT: ScriptableObject asset under Profiles/EquipmentSockets/Sockets/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Metadata only. Equipment visuals attach zeroed under socket transforms.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [CreateAssetMenu(
        fileName = "CCS_EquipmentSocketDefinition",
        menuName = "CCS/Character Controller/Equipment Socket Definition",
        order = 10)]
    public sealed class CCS_EquipmentSocketDefinition : ScriptableObject
    {
        #region Variables

        [SerializeField] private string socketId = string.Empty;

        [SerializeField] private HumanBodyBones parentBone = HumanBodyBones.Hips;

        [SerializeField] private HumanBodyBones fallbackParentBone = HumanBodyBones.LastBone;

        [SerializeField] private List<string> allowedItemTypes = new List<string>();

        [SerializeField] private Vector3 localPosition = Vector3.zero;

        [SerializeField] private Vector3 localEulerAngles = Vector3.zero;

        [SerializeField] private Vector3 localScale = Vector3.one;

        [SerializeField] private int priority = 100;

        [SerializeField] private List<string> blocksOtherSockets = new List<string>();

        #endregion

        #region Properties

        public string SocketId => socketId;

        public HumanBodyBones ParentBone => parentBone;

        public HumanBodyBones FallbackParentBone => fallbackParentBone;

        public IReadOnlyList<string> AllowedItemTypes => allowedItemTypes;

        public Vector3 LocalPosition => localPosition;

        public Vector3 LocalEulerAngles => localEulerAngles;

        public Vector3 LocalScale => localScale;

        public int Priority => priority;

        public IReadOnlyList<string> BlocksOtherSockets => blocksOtherSockets;

        public bool HasFallbackParentBone => fallbackParentBone != HumanBodyBones.LastBone;

        #endregion
    }
}
