using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentSocketRegistry
// CATEGORY: Modules / CharacterController / Runtime / Equipment / Sockets
// PURPOSE: Discovers equipment socket anchors and exposes lookup by socket ID.
// PLACEMENT: Player root alongside other composition components.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Lookup service only. Not an inventory or equipment system.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_EquipmentSocketRegistry : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_EquipmentSocketProfile equipmentSocketProfile;

        private readonly Dictionary<string, CCS_EquipmentSocketAnchor> socketAnchorsById =
            new Dictionary<string, CCS_EquipmentSocketAnchor>();

        private bool isInitialized;

        #endregion

        #region Properties

        public CCS_EquipmentSocketProfile EquipmentSocketProfile => equipmentSocketProfile;

        public IReadOnlyCollection<CCS_EquipmentSocketAnchor> SocketAnchors => socketAnchorsById.Values;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            RefreshSocketRegistry();
        }

        #endregion

        #region Public Methods

        public void SetEquipmentSocketProfile(CCS_EquipmentSocketProfile profile)
        {
            equipmentSocketProfile = profile;
            RefreshSocketRegistry();
        }

        public void RefreshSocketRegistry()
        {
            socketAnchorsById.Clear();
            CCS_EquipmentSocketAnchor[] anchors = GetComponentsInChildren<CCS_EquipmentSocketAnchor>(true);
            for (int i = 0; i < anchors.Length; i++)
            {
                CCS_EquipmentSocketAnchor anchor = anchors[i];
                if (anchor == null || string.IsNullOrEmpty(anchor.SocketId))
                {
                    continue;
                }

                if (socketAnchorsById.ContainsKey(anchor.SocketId))
                {
                    Debug.LogError(
                        "[Equipment Sockets] Duplicate socket ID '"
                        + anchor.SocketId
                        + "' on "
                        + anchor.name,
                        anchor);
                    continue;
                }

                socketAnchorsById.Add(anchor.SocketId, anchor);
            }

            isInitialized = true;
        }

        public bool TryGetSocket(string socketId, out Transform socketTransform)
        {
            socketTransform = null;
            if (string.IsNullOrEmpty(socketId))
            {
                return false;
            }

            if (!isInitialized)
            {
                RefreshSocketRegistry();
            }

            if (!socketAnchorsById.TryGetValue(socketId, out CCS_EquipmentSocketAnchor anchor) || anchor == null)
            {
                return false;
            }

            socketTransform = anchor.SocketTransform;
            return socketTransform != null;
        }

        public bool TryGetSocketAnchor(string socketId, out CCS_EquipmentSocketAnchor anchor)
        {
            anchor = null;
            if (string.IsNullOrEmpty(socketId))
            {
                return false;
            }

            if (!isInitialized)
            {
                RefreshSocketRegistry();
            }

            return socketAnchorsById.TryGetValue(socketId, out anchor) && anchor != null;
        }

        #endregion
    }
}
