using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentSocketAnchor
// CATEGORY: Modules / CharacterController / Runtime / Equipment / Sockets
// PURPOSE: Metadata marker on a bone-parented equipment socket transform.
// PLACEMENT: Child of humanoid bone or approved test fallback anchor.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Does not spawn equipment or own inventory state.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_EquipmentSocketAnchor : MonoBehaviour
    {
        #region Variables

        [SerializeField] private string socketId = string.Empty;

        [SerializeField] private HumanBodyBones parentBone = HumanBodyBones.LastBone;

        [SerializeField] private List<string> allowedItemTypes = new List<string>();

        [SerializeField] private int priority;

        [SerializeField] private List<string> blocksOtherSockets = new List<string>();

        [SerializeField] private bool isFallbackSocket;

        [SerializeField] private CCS_EquipmentSocketParentMode parentMode = CCS_EquipmentSocketParentMode.RealHumanoidBone;

        #endregion

        #region Properties

        public string SocketId => socketId;

        public HumanBodyBones ParentBone => parentBone;

        public IReadOnlyList<string> AllowedItemTypes => allowedItemTypes;

        public int Priority => priority;

        public IReadOnlyList<string> BlocksOtherSockets => blocksOtherSockets;

        public bool IsFallbackSocket => isFallbackSocket;

        public CCS_EquipmentSocketParentMode ParentMode => parentMode;

        public Transform SocketTransform => transform;

        #endregion

        #region Public Methods

        public void Configure(
            CCS_EquipmentSocketDefinition definition,
            CCS_EquipmentSocketParentMode socketParentMode,
            bool fallbackSocket)
        {
            if (definition == null)
            {
                return;
            }

            socketId = definition.SocketId;
            parentBone = definition.ParentBone;
            allowedItemTypes = new List<string>(definition.AllowedItemTypes);
            priority = definition.Priority;
            blocksOtherSockets = new List<string>(definition.BlocksOtherSockets);
            parentMode = socketParentMode;
            isFallbackSocket = fallbackSocket;
        }

        #endregion
    }
}
