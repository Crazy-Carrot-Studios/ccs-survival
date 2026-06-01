using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentAttachmentSocket
// CATEGORY: Modules / Equipment / Runtime / Visuals
// PURPOSE: Marks a child transform as a named equipment visual attachment point.
// PLACEMENT: Child objects under CCS_EquipmentAttachmentRig on PF_CCS_Player.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Local offsets on visual definitions are applied relative to this socket.
// =============================================================================

namespace CCS.Modules.Equipment
{
    [DisallowMultipleComponent]
    public sealed class CCS_EquipmentAttachmentSocket : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_EquipmentAttachmentSocketType socketType = CCS_EquipmentAttachmentSocketType.RightHand;

        #endregion

        #region Properties

        public CCS_EquipmentAttachmentSocketType SocketType => socketType;

        public Transform SocketTransform => transform;

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.9f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, 0.08f);
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * 0.2f);
        }
#endif
    }
}
