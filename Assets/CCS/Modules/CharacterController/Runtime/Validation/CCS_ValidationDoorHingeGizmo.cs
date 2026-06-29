using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ValidationDoorHingeGizmo
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Draws a visible hinge marker in the Scene view for test doors.
// PLACEMENT: PF_CCS_TestDoor_Single / DoorHingePivot empty object.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Editor gizmo only. No runtime door behavior.
// =============================================================================

namespace CCS.Modules.CharacterController.Validation {
    public sealed class CCS_ValidationDoorHingeGizmo : MonoBehaviour
    {
        #region Variables

        [SerializeField] private float sphereRadius = 0.06f;

        [SerializeField] private float wireRadius = 0.08f;

        #endregion

        #region Unity Callbacks

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 hingePosition = transform.position;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hingePosition, sphereRadius);

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(hingePosition, wireRadius);
        }
#endif

        #endregion
    }
}
