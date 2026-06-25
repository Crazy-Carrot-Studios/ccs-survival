using CCS.Modules.Attributes;
using UnityEngine;

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
        [SerializeField] private CCS_AITargetSensor targetSensor;
        [SerializeField] private CCS_AILineOfSightSensor lineOfSightSensor;
        [SerializeField] private CCS_AIMotorController motorController;
        [SerializeField] private CCS_AIWeaponController weaponController;
        [SerializeField] private bool enableBrainDebugLogs;

        private CCS_AIBanditState currentState = CCS_AIBanditState.Idle;
        private Transform currentTargetTransform;
        private CCS_IDamageable currentTargetDamageable;
        private float stateEnterTime;
        private float cooldownEndTime;

        public CCS_AIBanditState CurrentState => currentState;

        public Transform CurrentTargetTransform => currentTargetTransform;

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
            if (!TryResolveLivingTarget(out Transform targetTransform, out CCS_IDamageable damageable))
            {
                TransitionTo(CCS_AIBanditState.Idle);
                return;
            }

            currentTargetTransform = targetTransform;
            currentTargetDamageable = damageable;

            if (enableBrainDebugLogs)
            {
                Debug.Log($"[AI Bandit] Target acquired: {targetTransform.name}", this);
            }

            TransitionTo(CCS_AIBanditState.MoveToRange);
        }

        private void TickMoveToRange()
        {
            if (!ValidateTargetStillAlive())
            {
                TransitionTo(CCS_AIBanditState.AcquireTarget);
                return;
            }

            Vector3 targetPosition = currentTargetTransform.position;
            float distance = HorizontalDistanceTo(targetPosition);
            bool hasLineOfSight = HasLineOfSightToTarget(targetPosition);

            if (distance <= profile.AttackRange && hasLineOfSight)
            {
                motorController.Stop();
                TransitionTo(CCS_AIBanditState.DrawWeapon);
                return;
            }

            weaponController.SetAimHeld(false);
            motorController.MoveTowards(
                targetPosition,
                profile.MoveSpeed,
                profile.RotationSpeed,
                profile.AttackRange,
                profile.MinimumPreferredRange);
        }

        private void TickDrawWeapon()
        {
            if (!ValidateTargetStillAlive())
            {
                TransitionTo(CCS_AIBanditState.AcquireTarget);
                return;
            }

            Vector3 targetPosition = currentTargetTransform.position;
            float distance = HorizontalDistanceTo(targetPosition);
            if (distance > profile.AttackRange || !HasLineOfSightToTarget(targetPosition))
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
            if (!ValidateTargetStillAlive())
            {
                TransitionTo(CCS_AIBanditState.AcquireTarget);
                return;
            }

            Vector3 targetPosition = currentTargetTransform.position;
            float distance = HorizontalDistanceTo(targetPosition);
            if (distance > profile.AttackRange || !HasLineOfSightToTarget(targetPosition))
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
            if (!ValidateTargetStillAlive())
            {
                TransitionTo(CCS_AIBanditState.AcquireTarget);
                return;
            }

            Vector3 targetPosition = currentTargetTransform.position;
            if (HorizontalDistanceTo(targetPosition) > profile.AttackRange
                || !HasLineOfSightToTarget(targetPosition))
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
            if (Time.time < cooldownEndTime)
            {
                if (ValidateTargetStillAlive())
                {
                    motorController.RotateTowards(currentTargetTransform.position, profile.RotationSpeed);
                }

                return;
            }

            if (!ValidateTargetStillAlive())
            {
                weaponController.SetAimHeld(false);
                TransitionTo(CCS_AIBanditState.AcquireTarget);
                return;
            }

            Vector3 targetPosition = currentTargetTransform.position;
            if (HorizontalDistanceTo(targetPosition) <= profile.AttackRange
                && HasLineOfSightToTarget(targetPosition))
            {
                TransitionTo(CCS_AIBanditState.Aim);
                return;
            }

            weaponController.SetAimHeld(false);
            TransitionTo(CCS_AIBanditState.MoveToRange);
        }

        private bool TryResolveLivingTarget(out Transform targetTransform, out CCS_IDamageable damageable)
        {
            if (targetSensor.TryAcquireTarget(out targetTransform, out damageable)
                && damageable != null
                && !damageable.IsDead)
            {
                return true;
            }

            targetTransform = null;
            damageable = null;
            return false;
        }

        private bool ValidateTargetStillAlive()
        {
            return currentTargetTransform != null
                && currentTargetDamageable != null
                && !currentTargetDamageable.IsDead;
        }

        private float HorizontalDistanceTo(Vector3 targetPosition)
        {
            Vector3 delta = targetPosition - transform.position;
            delta.y = 0f;
            return delta.magnitude;
        }

        private Vector3 ResolveLineOfSightOrigin()
        {
            return transform.position + (Vector3.up * profile.LineOfSightHeight);
        }

        private bool HasLineOfSightToTarget(Vector3 targetPosition)
        {
            if (lineOfSightSensor == null || currentTargetTransform == null)
            {
                return false;
            }

            Vector3 aimPoint = targetPosition + (Vector3.up * profile.LineOfSightHeight);
            bool hasLos = lineOfSightSensor.HasLineOfSight(currentTargetTransform, aimPoint);
            return hasLos;
        }

        private void TransitionTo(CCS_AIBanditState nextState)
        {
            if (currentState == nextState)
            {
                return;
            }

            currentState = nextState;
            stateEnterTime = Time.time;
            if (enableBrainDebugLogs)
            {
                Debug.Log($"[AI Bandit] state -> {currentState}", this);
            }
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
        }
    }
}
