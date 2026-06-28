using CCS.Modules.Attributes;
using UnityEngine;
using UnityEngine.AI;

// =============================================================================

// SCRIPT: CCS_AIBanditBrain

// CATEGORY: Modules / AI / Runtime / Combat

// PURPOSE: Bandit state machine for target acquisition, movement, draw, aim, and fire.

// PLACEMENT: AI bandit root.

// AUTHOR: James Schilz

// CREATED: 2026-06-25

// NOTES: Server-side deterministic decision loop for v0.7.0 foundation.

// =============================================================================



namespace CCS.Modules.AI

{

    public sealed class CCS_AIBanditBrain : MonoBehaviour

    {

        [SerializeField] private CCS_AIBanditProfile profile;

        [SerializeField] private CCS_NetworkHealth networkHealth;

        [SerializeField] private CCS_AITargetSensor targetSensor;

        [SerializeField] private CCS_AILineOfSightSensor lineOfSightSensor;

        [SerializeField] private CCS_AIMotorController motorController;

        [SerializeField] private CCS_AIWeaponController weaponController;

        [SerializeField] private bool enableBrainDebugLogs;

        [SerializeField] private bool debugStalking;



        private CCS_AIBanditState currentState = CCS_AIBanditState.Idle;

        private Transform currentTargetTransform;

        private CCS_IDamageable currentTargetDamageable;

        private float stateEnterTime;

        private float cooldownEndTime;

        private float loseSightStartTime = -1f;



        public CCS_AIBanditState CurrentState => currentState;



        public Transform CurrentTargetTransform => currentTargetTransform;



        public CCS_AIBanditProfile Profile => profile;



        public bool IsAimingOrFiring =>

            currentState == CCS_AIBanditState.DrawWeapon

            || currentState == CCS_AIBanditState.Aim

            || currentState == CCS_AIBanditState.Fire

            || currentState == CCS_AIBanditState.Cooldown;



        public void SetProfile(CCS_AIBanditProfile nextProfile)

        {

            profile = nextProfile;

        }



        public void TickBrain(bool canThink)

        {

            if (!canThink)

            {

                return;

            }



            ResolveReferences();

            if (profile == null || targetSensor == null || motorController == null || weaponController == null)

            {

                return;

            }



            if (currentState == CCS_AIBanditState.Dead)

            {

                return;

            }



            if (networkHealth != null && networkHealth.IsDead)

            {

                ForceDeadState();

                return;

            }



            if (ApplyStalkingPriority())

            {

                LogStalkingDebugIfEnabled();

                return;

            }



            switch (currentState)

            {

                case CCS_AIBanditState.Idle:

                    TickIdle();

                    break;

                case CCS_AIBanditState.AcquireTarget:

                    TickAcquireTarget();

                    break;

                case CCS_AIBanditState.MoveToRange:

                    TickMoveToRange();

                    break;

                case CCS_AIBanditState.DrawWeapon:

                    TickDrawWeapon();

                    break;

                case CCS_AIBanditState.Aim:

                    TickAim();

                    break;

                case CCS_AIBanditState.Fire:

                    TickFire();

                    break;

                case CCS_AIBanditState.Cooldown:

                    TickCooldown();

                    break;

            }



            LogBrainDebugIfEnabled();

        }



        public void ForceDeadState()

        {

            TransitionTo(CCS_AIBanditState.Dead);

            weaponController?.SetAimHeld(false);

            motorController?.Stop();

        }



        private void TickIdle()

        {

            weaponController.SetAimHeld(false);

            motorController.Stop();

            TransitionTo(CCS_AIBanditState.AcquireTarget);

        }



        private void TickAcquireTarget()

        {

            if (networkHealth != null && (!networkHealth.IsDamageReady || networkHealth.IsDead))

            {

                TransitionTo(CCS_AIBanditState.Idle);

                return;

            }



            if (!TryRefreshLivingTarget(out Transform targetTransform, out CCS_IDamageable damageable))

            {

                weaponController.SetAimHeld(false);

                return;

            }



            currentTargetTransform = targetTransform;

            currentTargetDamageable = damageable;

            loseSightStartTime = -1f;



            if (enableBrainDebugLogs)

            {

                Debug.Log($"[AI Bandit] Target acquired: {targetTransform.name}", this);

            }



            float distance = HorizontalDistanceTo(targetTransform.position);

            if (distance <= profile.AttackRange && HasLineOfSightToTarget(targetTransform.position))

            {

                TransitionTo(CCS_AIBanditState.DrawWeapon);

                return;

            }



            TransitionTo(CCS_AIBanditState.MoveToRange);

        }



        private void TickMoveToRange()

        {

            if (!TryRefreshLivingTarget(out Transform targetTransform, out CCS_IDamageable damageable))

            {

                TransitionTo(CCS_AIBanditState.AcquireTarget);

                return;

            }



            currentTargetTransform = targetTransform;

            currentTargetDamageable = damageable;



            Vector3 targetPosition = currentTargetTransform.position;

            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit navHit, 3f, NavMesh.AllAreas))

            {

                targetPosition = navHit.position;

            }



            float distance = HorizontalDistanceTo(targetPosition);

            bool hasLineOfSight = HasLineOfSightToTarget(targetPosition);

            loseSightStartTime = hasLineOfSight ? -1f : ResolveLoseSightStartTime();



            if (distance <= profile.AttackRange && hasLineOfSight)

            {

                motorController.Stop();

                TransitionTo(CCS_AIBanditState.DrawWeapon);

                return;

            }



            weaponController.SetAimHeld(false);

            motorController.ChaseTarget(ResolveChaseTargetPosition(), profile);

        }



        private bool ApplyStalkingPriority()

        {

            if (!TryRefreshLivingTarget(out Transform targetTransform, out CCS_IDamageable damageable))

            {

                weaponController.SetAimHeld(false);

                if (currentState != CCS_AIBanditState.AcquireTarget)

                {

                    TransitionTo(CCS_AIBanditState.AcquireTarget);

                }

                return true;

            }



            currentTargetTransform = targetTransform;

            currentTargetDamageable = damageable;



            float distance = HorizontalDistanceTo(currentTargetTransform.position);

            bool hasLos = HasLineOfSightToTarget(currentTargetTransform.position);

            if (distance <= profile.AttackRange && hasLos)

            {

                return false;

            }



            weaponController.SetAimHeld(false);

            if (currentState != CCS_AIBanditState.MoveToRange)

            {

                TransitionTo(CCS_AIBanditState.MoveToRange);

            }



            motorController.ChaseTarget(ResolveChaseTargetPosition(), profile);

            return true;

        }



        private Vector3 ResolveChaseTargetPosition()

        {

            if (currentTargetTransform == null)

            {

                return transform.position;

            }



            Vector3 targetPosition = currentTargetTransform.position;

            float sampleRadius = profile != null ? profile.TargetSampleRadius : 4f;

            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit navHit, sampleRadius, NavMesh.AllAreas))

            {

                return navHit.position;

            }



            return targetPosition;

        }



        private void TickDrawWeapon()

        {

            if (!TryRefreshLivingTarget(out Transform targetTransform, out CCS_IDamageable damageable))

            {

                TransitionTo(CCS_AIBanditState.AcquireTarget);

                return;

            }



            currentTargetTransform = targetTransform;

            currentTargetDamageable = damageable;



            Vector3 targetPosition = currentTargetTransform.position;

            if (ShouldExitCombatForRangeOrSight(targetPosition))

            {

                weaponController.SetAimHeld(false);

                TransitionTo(CCS_AIBanditState.MoveToRange);

                return;

            }



            weaponController.SetAimHeld(true);

            motorController.Stop();

            motorController.RotateTowards(targetPosition, profile.RotationSpeed);



            if (Time.time - stateEnterTime >= profile.AimSettleSeconds * 0.35f)

            {

                TransitionTo(CCS_AIBanditState.Aim);

            }

        }



        private void TickAim()

        {

            if (!TryRefreshLivingTarget(out Transform targetTransform, out CCS_IDamageable damageable))

            {

                TransitionTo(CCS_AIBanditState.AcquireTarget);

                return;

            }



            currentTargetTransform = targetTransform;

            currentTargetDamageable = damageable;



            Vector3 targetPosition = currentTargetTransform.position;

            if (ShouldExitCombatForRangeOrSight(targetPosition))

            {

                weaponController.SetAimHeld(false);

                TransitionTo(CCS_AIBanditState.MoveToRange);

                return;

            }



            weaponController.SetAimHeld(true);

            motorController.Stop();

            motorController.RotateTowards(targetPosition, profile.RotationSpeed);



            if (Time.time - stateEnterTime >= profile.AimSettleSeconds)

            {

                TransitionTo(CCS_AIBanditState.Fire);

            }

        }



        private void TickFire()

        {

            if (!TryRefreshLivingTarget(out Transform targetTransform, out CCS_IDamageable damageable))

            {

                TransitionTo(CCS_AIBanditState.AcquireTarget);

                return;

            }



            currentTargetTransform = targetTransform;

            currentTargetDamageable = damageable;



            Vector3 targetPosition = currentTargetTransform.position;

            if (ShouldExitCombatForRangeOrSight(targetPosition))

            {

                weaponController.SetAimHeld(false);

                TransitionTo(CCS_AIBanditState.MoveToRange);

                return;

            }



            weaponController.SetAimHeld(true);

            motorController.Stop();

            motorController.RotateTowards(targetPosition, profile.RotationSpeed);



            Vector3 chestAimPoint = targetPosition + (Vector3.up * profile.LineOfSightHeight);

            bool fired = weaponController.TryFireAtTarget(

                currentTargetTransform,

                chestAimPoint,

                profile.ShotMaxRange,

                spreadDegrees: 0f,

                profile.VisibilityMask,

                profile.ShotDamage,

                profile.FireCooldownSeconds);



            if (fired && enableBrainDebugLogs)

            {

                Debug.Log("[AI Bandit] Fire", this);

            }



            cooldownEndTime = Time.time + profile.FireCooldownSeconds;

            TransitionTo(CCS_AIBanditState.Cooldown);

        }



        private void TickCooldown()

        {

            if (!TryRefreshLivingTarget(out Transform targetTransform, out CCS_IDamageable damageable))

            {

                weaponController.SetAimHeld(false);

                TransitionTo(CCS_AIBanditState.AcquireTarget);

                return;

            }



            currentTargetTransform = targetTransform;

            currentTargetDamageable = damageable;



            Vector3 targetPosition = currentTargetTransform.position;

            if (ShouldExitCombatForRangeOrSight(targetPosition))

            {

                weaponController.SetAimHeld(false);

                TransitionTo(CCS_AIBanditState.MoveToRange);

                return;

            }



            weaponController.SetAimHeld(true);

            motorController.RotateTowards(targetPosition, profile.RotationSpeed);



            if (Time.time < cooldownEndTime)

            {

                return;

            }



            if (HorizontalDistanceTo(targetPosition) <= profile.AttackRange

                && HasLineOfSightToTarget(targetPosition))

            {

                TransitionTo(CCS_AIBanditState.Aim);

                return;

            }



            weaponController.SetAimHeld(false);

            TransitionTo(CCS_AIBanditState.MoveToRange);

        }



        private bool TryRefreshLivingTarget(out Transform targetTransform, out CCS_IDamageable damageable)

        {

            if (targetSensor.TryRefreshLivingTarget(out targetTransform, out damageable)

                && damageable != null

                && !damageable.IsDead

                && damageable.IsDamageReady)

            {

                return true;

            }



            targetTransform = null;

            damageable = null;

            return false;

        }



        private bool ShouldExitCombatForRangeOrSight(Vector3 targetPosition)

        {

            if (HorizontalDistanceTo(targetPosition) > profile.AttackRange)

            {

                loseSightStartTime = -1f;

                return true;

            }



            if (!HasLineOfSightToTarget(targetPosition))

            {

                return true;

            }



            loseSightStartTime = -1f;

            return false;

        }



        private float ResolveLoseSightStartTime()

        {

            if (loseSightStartTime < 0f)

            {

                loseSightStartTime = Time.time;

            }



            return loseSightStartTime;

        }



        private float HorizontalDistanceTo(Vector3 targetPosition)

        {

            Vector3 delta = targetPosition - transform.position;

            delta.y = 0f;

            return delta.magnitude;

        }



        private bool HasLineOfSightToTarget(Vector3 targetPosition)

        {

            if (lineOfSightSensor == null || currentTargetTransform == null)

            {

                return false;

            }



            Vector3 aimPoint = targetPosition + (Vector3.up * profile.LineOfSightHeight);

            return lineOfSightSensor.HasLineOfSight(currentTargetTransform, aimPoint);

        }



        private void TransitionTo(CCS_AIBanditState nextState)

        {

            if (currentState == nextState)

            {

                return;

            }



            currentState = nextState;

            stateEnterTime = Time.time;

            if (nextState == CCS_AIBanditState.MoveToRange || nextState == CCS_AIBanditState.AcquireTarget)

            {

                loseSightStartTime = -1f;

            }



            if (enableBrainDebugLogs)

            {

                Debug.Log($"[AI Bandit] state -> {currentState}", this);

            }

        }



        private void LogBrainDebugIfEnabled()

        {

            LogStalkingDebugIfEnabled();

        }



        private void LogStalkingDebugIfEnabled()

        {

            if ((!enableBrainDebugLogs && !debugStalking) || profile == null || motorController == null)

            {

                return;

            }



            NavMeshAgent agent = motorController.GetComponent<NavMeshAgent>();

            float targetDistance = currentTargetTransform != null

                ? HorizontalDistanceTo(currentTargetTransform.position)

                : -1f;

            bool hasLos = currentTargetTransform != null

                && HasLineOfSightToTarget(currentTargetTransform.position);



            Debug.Log(

                "[AI Stalking Debug]\n"

                + "State: "

                + currentState

                + "\nTarget: "

                + (currentTargetTransform != null ? currentTargetTransform.name : "None")

                + "\nDistance: "

                + targetDistance.ToString("0.00")

                + "\nAttackRange: "

                + profile.AttackRange.ToString("0.00")

                + "\nLOS: "

                + hasLos

                + "\nDestination: "

                + motorController.LastDestination

                + "\nSampled Destination: "

                + motorController.LastSampledTargetDestination

                + "\nDestination Age: "

                + motorController.TimeSinceLastRepath.ToString("0.00")

                + "\nRepath Timer: "

                + motorController.TimeSinceLastRepath.ToString("0.00")

                + "\nAgent HasPath: "

                + (agent != null && agent.hasPath)

                + "\nPath Status: "

                + (agent != null ? agent.pathStatus.ToString() : "None")

                + "\nRemaining Distance: "

                + (agent != null ? agent.remainingDistance.ToString("0.00") : "0.00")

                + "\nIsStopped: "

                + (agent != null && agent.isStopped),

                this);

        }



        private void ResolveReferences()

        {

            if (targetSensor == null)

            {

                targetSensor = GetComponent<CCS_AITargetSensor>();

            }



            if (lineOfSightSensor == null)

            {

                lineOfSightSensor = GetComponent<CCS_AILineOfSightSensor>();

            }



            if (motorController == null)

            {

                motorController = GetComponent<CCS_AIMotorController>();

            }



            if (weaponController == null)

            {

                weaponController = GetComponent<CCS_AIWeaponController>();

            }



            if (profile != null && targetSensor != null)

            {

                targetSensor.Configure(profile.DetectionRange, profile.VisibilityMask);

            }



            if (profile != null && lineOfSightSensor != null)

            {

                lineOfSightSensor.Configure(profile.ShotMaxRange, profile.VisibilityMask);

            }



            if (networkHealth == null)

            {

                networkHealth = GetComponent<CCS_NetworkHealth>();

            }

        }

    }

}


