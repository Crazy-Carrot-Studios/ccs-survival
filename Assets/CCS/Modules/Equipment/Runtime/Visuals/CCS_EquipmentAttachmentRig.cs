using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentAttachmentRig
// CATEGORY: Modules / Equipment / Runtime / Visuals
// PURPOSE: Discovers and exposes equipment attachment sockets on the player prefab.
// PLACEMENT: PF_CCS_Player root or EquipmentRig child object.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Socket discovery supports inactive children for bootstrap prefab authoring.
// =============================================================================

namespace CCS.Modules.Equipment
{
    [DisallowMultipleComponent]
    public sealed class CCS_EquipmentAttachmentRig : MonoBehaviour
    {
        #region Variables

        [SerializeField] private bool includeInactiveChildren = true;
        [SerializeField] private List<CCS_EquipmentAttachmentSocket> cachedSockets = new List<CCS_EquipmentAttachmentSocket>();

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            RebuildSocketCache();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            RebuildSocketCache();
        }
#endif

        #endregion

        #region Public Methods

        public void RebuildSocketCache()
        {
            cachedSockets.Clear();
            GetComponentsInChildren(includeInactiveChildren, cachedSockets);
        }

        public bool TryGetSocket(
            CCS_EquipmentAttachmentSocketType socketType,
            out CCS_EquipmentAttachmentSocket socket)
        {
            socket = null;
            for (int index = 0; index < cachedSockets.Count; index++)
            {
                CCS_EquipmentAttachmentSocket candidate = cachedSockets[index];
                if (candidate != null && candidate.SocketType == socketType)
                {
                    socket = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool HasDuplicateSocketTypes()
        {
            HashSet<CCS_EquipmentAttachmentSocketType> seen = new HashSet<CCS_EquipmentAttachmentSocketType>();
            for (int index = 0; index < cachedSockets.Count; index++)
            {
                CCS_EquipmentAttachmentSocket candidate = cachedSockets[index];
                if (candidate == null)
                {
                    continue;
                }

                if (!seen.Add(candidate.SocketType))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Properties

        public IReadOnlyList<CCS_EquipmentAttachmentSocket> Sockets => cachedSockets;

        #endregion
    }
}
