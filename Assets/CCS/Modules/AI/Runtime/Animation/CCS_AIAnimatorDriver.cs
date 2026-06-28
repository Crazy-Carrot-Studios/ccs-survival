using CCS.Modules.Attributes;
using CCS.Modules.CharacterController;
using UnityEngine;
using UnityEngine.AI;

// =============================================================================
// SCRIPT: CCS_AIAnimatorDriver
// CATEGORY: Modules / AI / Runtime / Animation
// PURPOSE: Drives AI locomotion Animator parameters from NavMesh movement and brain aim state.
// PLACEMENT: AI bandit root with Animator, NavMeshAgent, and CCS_AIBanditBrain.
// AUTHOR: James Schilz
// CREATED: 2026-06-26
// NOTES: Guarded parameter writes. Stops driving when AI is dead or ragdolling.
// =============================================================================

namespace CCS.Modules.AI
{
    [DefaultExecutionOrder(195)]
    public sealed class CCS_AIAnimatorDriver : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");
        private static readonly int SpeedNormalizedHash = Animator.StringToHash("SpeedNormalized");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int FreeFallHash = Animator.StringToHash("FreeFall");

        [SerializeField] private Animator animator;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private CCS_AIBanditBrain brain;
        [SerializeField] private CCS_AIBanditProfile profile;
        [SerializeField] private CCS_NetworkHealth networkHealth;
        [SerializeField] private bool enableAnimatorDebugLogs;

        private readonly System.Collections.Generic.HashSet<int> animatorParameterHashes =
            new System.Collections.Generic.HashSet<int>();
        private Vector3 lastPosition;
        private bool animatorParametersCached;
        private bool loggedMissingAnimator;

        private void Awake()
        {
            ResolveReferences();
            lastPosition = transform.position;
        }

        private void OnEnable()
        {
            CacheAnimatorParameters();
            lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (ShouldStopDriving())
            {
                SetLocomotionParameters(0f, 0f, grounded: true);
                return;
            }

            if (!TryResolveAnimator(out Animator resolvedAnimator))
            {
                return;
            }

            float profileMoveSpeed = profile != null ? Mathf.Max(0.1f, profile.MoveSpeed) : 2.8f;
            float horizontalSpeed = ResolveHorizontalSpeed();
            float normalizedSpeed = Mathf.Clamp01(horizontalSpeed / profileMoveSpeed);

            SetLocomotionParameters(horizontalSpeed, normalizedSpeed, grounded: true);
            lastPosition = transform.position;

            if (enableAnimatorDebugLogs && horizontalSpeed > 0.05f)
            {
                Debug.Log(
                    "[AI Animator Driver] speed="
                    + horizontalSpeed.ToString("0.00")
                    + " normalized="
                    + normalizedSpeed.ToString("0.00"),
                    this);
            }
        }

        public void StopDriving()
        {
            SetLocomotionParameters(0f, 0f, grounded: true);
        }

        private bool ShouldStopDriving()
        {
            if (networkHealth != null && networkHealth.IsDead)
            {
                return true;
            }

            return brain != null && brain.CurrentState == CCS_AIBanditState.Dead;
        }

        private float ResolveHorizontalSpeed()
        {
            ResolveReferences();
            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                Vector3 velocity = navMeshAgent.velocity;
                velocity.y = 0f;
                if (velocity.sqrMagnitude > 0.0001f)
                {
                    return velocity.magnitude;
                }

                Vector3 desiredVelocity = navMeshAgent.desiredVelocity;
                desiredVelocity.y = 0f;
                if (desiredVelocity.sqrMagnitude > 0.0001f)
                {
                    return desiredVelocity.magnitude;
                }
            }

            Vector3 delta = transform.position - lastPosition;
            delta.y = 0f;
            float deltaSpeed = Time.deltaTime > 0.0001f ? delta.magnitude / Time.deltaTime : 0f;
            return deltaSpeed;
        }

        private void SetLocomotionParameters(float horizontalSpeed, float normalizedSpeed, bool grounded)
        {
            if (!TryResolveAnimator(out _))
            {
                return;
            }

            SetAnimatorFloatIfPresent(SpeedHash, horizontalSpeed);
            SetAnimatorFloatIfPresent(MotionSpeedHash, normalizedSpeed);
            SetAnimatorFloatIfPresent(SpeedNormalizedHash, normalizedSpeed);
            SetAnimatorBoolIfPresent(GroundedHash, grounded);
            SetAnimatorBoolIfPresent(IsGroundedHash, grounded);
            SetAnimatorBoolIfPresent(JumpHash, false);
            SetAnimatorBoolIfPresent(FreeFallHash, false);
        }

        private bool TryResolveAnimator(out Animator resolvedAnimator)
        {
            ResolveReferences();
            if (animator != null && HasPlayableController(animator))
            {
                resolvedAnimator = animator;
                return true;
            }

            Animator[] animators = GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator candidate = animators[i];
                if (candidate == null || !HasPlayableController(candidate))
                {
                    continue;
                }

                animator = candidate;
                resolvedAnimator = candidate;
                loggedMissingAnimator = false;
                CacheAnimatorParameters();
                return true;
            }

            resolvedAnimator = null;
            if (!loggedMissingAnimator && enableAnimatorDebugLogs)
            {
                loggedMissingAnimator = true;
                Debug.LogWarning("[AI Animator Driver] No playable Animator found on AI bandit.", this);
            }

            return false;
        }

        private static bool HasPlayableController(Animator candidate)
        {
            return candidate != null
                && candidate.isActiveAndEnabled
                && candidate.runtimeAnimatorController != null;
        }

        private void CacheAnimatorParameters()
        {
            animatorParameterHashes.Clear();
            if (animator == null)
            {
                animatorParametersCached = false;
                return;
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                animatorParameterHashes.Add(parameters[i].nameHash);
            }

            animatorParametersCached = true;
        }

        private void SetAnimatorFloatIfPresent(int hash, float value)
        {
            if (animator == null || !animatorParametersCached || !animatorParameterHashes.Contains(hash))
            {
                return;
            }

            animator.SetFloat(hash, value);
        }

        private void SetAnimatorBoolIfPresent(int hash, bool value)
        {
            if (animator == null || !animatorParametersCached || !animatorParameterHashes.Contains(hash))
            {
                return;
            }

            animator.SetBool(hash, value);
        }

        private void ResolveReferences()
        {
            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }

            if (brain == null)
            {
                brain = GetComponent<CCS_AIBanditBrain>();
            }

            if (profile == null && brain != null)
            {
                profile = brain.Profile;
            }

            if (networkHealth == null)
            {
                networkHealth = GetComponent<CCS_NetworkHealth>();
            }
        }
    }
}
