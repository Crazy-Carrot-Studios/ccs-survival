using CCS.Modules.Interaction;
using CCS.Modules.Storage;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MountWorldActor
// CATEGORY: Modules / Mounts / Runtime / Components
// PURPOSE: World representation of an owned mount with movement and interaction hooks.
// PLACEMENT: PF_CCS_Horse root.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CCS_MountWorldActor : MonoBehaviour
    {
        [SerializeField] private string mountInstanceId = string.Empty;
        [SerializeField] private CCS_MountDefinition mountDefinition;
        [SerializeField] private CCS_HorseSaddlebagContainer saddlebagContainer;
        [SerializeField] private CCS_MountInteractable mountInteractable;
        [SerializeField] private Transform wagonHitchPoint;
        [SerializeField] private float followStopDistance = 2.5f;
        [SerializeField] private float callMoveSpeed = 6f;

        private CharacterController characterController;
        private Vector3 followTarget;
        private bool hasFollowTarget;

        public string MountInstanceId => mountInstanceId ?? string.Empty;

        public CCS_MountDefinition MountDefinition => mountDefinition;

        public CCS_HorseSaddlebagContainer Saddlebag => saddlebagContainer;

        public CCS_StorageContainer SaddlebagContainer =>
            saddlebagContainer != null ? saddlebagContainer.StorageContainer : null;

        public float FollowStopDistance => followStopDistance < 0.5f ? 2.5f : followStopDistance;

        public Transform WagonHitchPoint
        {
            get
            {
                if (wagonHitchPoint != null)
                {
                    return wagonHitchPoint;
                }

                return transform;
            }
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (saddlebagContainer == null)
            {
                saddlebagContainer = GetComponentInChildren<CCS_HorseSaddlebagContainer>();
            }

            if (mountInteractable == null)
            {
                mountInteractable = GetComponent<CCS_MountInteractable>();
            }
        }

        public void Configure(
            string instanceId,
            CCS_MountDefinition definition,
            CCS_HorseSaddlebagContainer saddlebag,
            string saddlebagInstanceId,
            int saddlebagSlots)
        {
            mountInstanceId = instanceId ?? string.Empty;
            mountDefinition = definition;
            saddlebagContainer = saddlebag;

            if (saddlebagContainer != null)
            {
                saddlebagContainer.ConfigureForMount(
                    CCS_MountContentIds.HorseSaddlebagContainerId,
                    saddlebagInstanceId,
                    "Saddlebag",
                    saddlebagSlots);
            }

            if (mountInteractable != null)
            {
                mountInteractable.BindMountInstanceId(mountInstanceId);
            }
        }

        public void SetFollowTarget(Vector3 target, bool active)
        {
            followTarget = target;
            hasFollowTarget = active;
        }

        public void TickLocomotion(
            CCS_MountState state,
            Vector3 riderPlanarInput,
            float walkSpeed,
            float sprintSpeed,
            bool sprintHeld,
            float deltaTime)
        {
            if (characterController == null || deltaTime <= 0f)
            {
                return;
            }

            switch (state)
            {
                case CCS_MountState.Mounted:
                    MovePlanar(riderPlanarInput, sprintHeld ? sprintSpeed : walkSpeed, deltaTime);
                    break;
                case CCS_MountState.Following:
                case CCS_MountState.Returning:
                    if (hasFollowTarget)
                    {
                        MoveTowards(followTarget, callMoveSpeed, deltaTime);
                    }

                    break;
                case CCS_MountState.Waiting:
                case CCS_MountState.Idle:
                default:
                    break;
            }
        }

        private void MovePlanar(Vector3 planarInput, float speed, float deltaTime)
        {
            if (planarInput.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Vector3 direction = planarInput.normalized;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction, Vector3.up),
                    12f * deltaTime);
            }

            characterController.Move(direction * (speed * deltaTime));
        }

        private void MoveTowards(Vector3 target, float speed, float deltaTime)
        {
            Vector3 toTarget = target - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            if (distance <= FollowStopDistance)
            {
                return;
            }

            Vector3 direction = toTarget / distance;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                10f * deltaTime);
            float step = Mathf.Min(speed * deltaTime, distance - FollowStopDistance);
            characterController.Move(direction * step);
        }
    }
}
