using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RagdollController
// CATEGORY: Modules / AI / Runtime / Components
// PURPOSE: Caches and toggles generated ragdoll rigidbodies for AI death feedback.
// PLACEMENT: AI bandit root under VisualRoot / humanoid armature.
// AUTHOR: James Schilz
// CREATED: 2026-06-26
// NOTES: Ragdoll starts disabled while alive. Enables physics collapse on death.
// =============================================================================

namespace CCS.Modules.AI
{
    public sealed class CCS_RagdollController : MonoBehaviour
    {
        private static readonly HumanBodyBones[] RagdollBones =
        {
            HumanBodyBones.Hips,
            HumanBodyBones.Spine,
            HumanBodyBones.Chest,
            HumanBodyBones.Head,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.RightHand,
            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.RightLowerLeg,
        };

        [SerializeField] private Animator animator;
        [SerializeField] private Transform ragdollRoot;
        [SerializeField] private float defaultBoneMass = 8f;
        [SerializeField] private bool enableRagdollDebugLogs;

        private readonly List<Rigidbody> ragdollBodies = new List<Rigidbody>();
        private readonly List<Collider> ragdollColliders = new List<Collider>();
        private bool ragdollBuilt;
        private bool ragdollActive;

        public bool IsRagdollActive => ragdollActive;

        private void Awake()
        {
            ResolveReferences();
            EnsureRagdollSetup();
            SetRagdollActive(false);
        }

        public void EnableRagdoll(Vector3 impulseDirection, float impulseStrength)
        {
            ResolveReferences();
            EnsureRagdollSetup();
            if (ragdollBodies.Count == 0)
            {
                if (enableRagdollDebugLogs)
                {
                    Debug.LogWarning("[AI Ragdoll] No ragdoll bodies found; death will use Animator disable only.", this);
                }

                if (animator != null)
                {
                    animator.enabled = false;
                }

                ragdollActive = true;
                return;
            }

            if (animator != null)
            {
                animator.enabled = false;
            }

            for (int i = 0; i < ragdollBodies.Count; i++)
            {
                Rigidbody body = ragdollBodies[i];
                if (body == null)
                {
                    continue;
                }

                body.isKinematic = false;
                body.useGravity = true;
                body.detectCollisions = true;
            }

            for (int i = 0; i < ragdollColliders.Count; i++)
            {
                Collider collider = ragdollColliders[i];
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }

            Rigidbody primaryBody = ragdollBodies[0];
            if (primaryBody != null && impulseDirection.sqrMagnitude > 0.0001f)
            {
                primaryBody.AddForce(impulseDirection.normalized * impulseStrength, ForceMode.Impulse);
            }

            ragdollActive = true;

            if (enableRagdollDebugLogs)
            {
                Debug.Log("[AI Ragdoll] Ragdoll enabled with " + ragdollBodies.Count + " bodies.", this);
            }
        }

        public void SetRagdollActive(bool active)
        {
            ResolveReferences();
            EnsureRagdollSetup();

            for (int i = 0; i < ragdollBodies.Count; i++)
            {
                Rigidbody body = ragdollBodies[i];
                if (body == null)
                {
                    continue;
                }

                if (active)
                {
                    body.isKinematic = false;
                    body.useGravity = true;
                    body.detectCollisions = true;
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                else
                {
                    if (!body.isKinematic)
                    {
                        body.linearVelocity = Vector3.zero;
                        body.angularVelocity = Vector3.zero;
                    }

                    body.isKinematic = true;
                    body.useGravity = false;
                    body.detectCollisions = false;
                }
            }

            for (int i = 0; i < ragdollColliders.Count; i++)
            {
                Collider collider = ragdollColliders[i];
                if (collider != null)
                {
                    collider.enabled = active;
                }
            }

            if (animator != null)
            {
                animator.enabled = !active;
            }

            ragdollActive = active;
        }

        private void EnsureRagdollSetup()
        {
            if (ragdollBuilt)
            {
                return;
            }

            ragdollBodies.Clear();
            ragdollColliders.Clear();

            if (animator == null)
            {
                ragdollBuilt = true;
                return;
            }

            for (int i = 0; i < RagdollBones.Length; i++)
            {
                Transform bone = animator.GetBoneTransform(RagdollBones[i]);
                if (bone == null)
                {
                    continue;
                }

                CapsuleCollider capsule = bone.GetComponent<CapsuleCollider>();
                if (capsule == null)
                {
                    capsule = bone.gameObject.AddComponent<CapsuleCollider>();
                    capsule.radius = 0.08f;
                    capsule.height = 0.28f;
                    capsule.direction = 1;
                }

                Rigidbody body = bone.GetComponent<Rigidbody>();
                if (body == null)
                {
                    body = bone.gameObject.AddComponent<Rigidbody>();
                }

                body.mass = defaultBoneMass;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                ragdollBodies.Add(body);
                ragdollColliders.Add(capsule);
            }

            ragdollBuilt = true;
        }

        private void ResolveReferences()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (ragdollRoot == null)
            {
                Transform visualRoot = transform.Find("VisualRoot");
                ragdollRoot = visualRoot != null ? visualRoot : transform;
            }
        }
    }
}
