using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionScanOriginGizmo
// CATEGORY: Modules / Interaction / Runtime / Components
// PURPOSE: Editor gizmo for scan-origin forward alignment and interaction volume.
// PLACEMENT: InteractionScanOrigin child on the test player prefab.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Blue/green rays = forward alignment. Yellow/cyan boxes = pickup/door volumes.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public sealed class CCS_InteractionScanOriginGizmo : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Transform playerRoot;
        [SerializeField] private float interactionHalfWidth = CCS_InteractionConstants.DefaultInteractionHalfWidth;
        [SerializeField] private float interactionHalfHeight = CCS_InteractionConstants.DefaultInteractionHalfHeight;
        [SerializeField] private float pickupVolumeDepth = CCS_InteractionConstants.DefaultStrictPickupDistance;
        [SerializeField] private float doorVolumeDepth = CCS_InteractionConstants.DefaultWalkThroughDoorStrictRange;
        [SerializeField] private float forwardRayLength = 1.5f;

        #endregion

        #region Unity Callbacks

        private void Reset()
        {
            if (transform.parent != null)
            {
                playerRoot = transform.parent;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Transform root = playerRoot != null ? playerRoot : transform.parent;
            Vector3 origin = transform.position;

            Vector3 scanForward = Flatten(transform.forward);
            if (scanForward.sqrMagnitude > Mathf.Epsilon)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(origin, scanForward * forwardRayLength);
            }

            if (root != null)
            {
                Vector3 rootForward = Flatten(root.forward);
                if (rootForward.sqrMagnitude > Mathf.Epsilon)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(origin, rootForward * forwardRayLength);
                }

                DrawInteractionVolume(root, pickupVolumeDepth, new Color(1f, 0.92f, 0.016f, 0.85f));
                DrawInteractionVolume(root, doorVolumeDepth, new Color(0f, 0.85f, 1f, 0.85f));
            }
        }
#endif

        #endregion

        #region Public Methods

        public void Configure(
            Transform configuredPlayerRoot,
            float configuredHalfWidth,
            float configuredHalfHeight,
            float configuredPickupDepth,
            float configuredDoorDepth)
        {
            playerRoot = configuredPlayerRoot;
            interactionHalfWidth = configuredHalfWidth;
            interactionHalfHeight = configuredHalfHeight;
            pickupVolumeDepth = configuredPickupDepth;
            doorVolumeDepth = configuredDoorDepth;
            forwardRayLength = Mathf.Max(configuredPickupDepth, configuredDoorDepth);
        }

        #endregion

        #region Private Methods

#if UNITY_EDITOR
        private void DrawInteractionVolume(Transform root, float depth, Color color)
        {
            if (depth <= Mathf.Epsilon)
            {
                return;
            }

            Vector3 size = new Vector3(
                interactionHalfWidth * 2f,
                interactionHalfHeight * 2f,
                depth);
            Vector3 localCenter = new Vector3(0f, 0f, depth * 0.5f);

            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = root.localToWorldMatrix;
            Gizmos.color = color;
            Gizmos.DrawWireCube(localCenter, size);
            Gizmos.matrix = previousMatrix;
        }
#endif

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            if (value.sqrMagnitude <= Mathf.Epsilon)
            {
                return Vector3.zero;
            }

            return value.normalized;
        }

        #endregion
    }
}
