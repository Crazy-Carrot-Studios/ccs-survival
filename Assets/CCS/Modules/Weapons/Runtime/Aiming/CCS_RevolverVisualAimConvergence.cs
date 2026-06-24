using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverVisualAimConvergence
// CATEGORY: Modules / Weapons / Runtime / Aiming
// PURPOSE: Rotates equipped revolver aim convergence root toward reticle aim point.
// PLACEMENT: CCS_RUNTIME_Revolver_AimConvergenceRoot under equipped attachment root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Experimental visual-only correction. Default OFF — rotating the gun can break saved hand fit.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [DefaultExecutionOrder(125)]
    public sealed class CCS_RevolverVisualAimConvergence : MonoBehaviour
    {
        #region Variables

        private Transform equippedVisualRoot;
        private Transform muzzlePoint;
        private bool convergenceActive;
        private CCS_RevolverVisualAimConvergenceSettings settings = CCS_RevolverVisualAimConvergenceSettings.Default;
        private CCS_WeaponAimSolution lastAimSolution;
        private bool hasLastAimSolution;

        #endregion

        #region Properties

        public float LastVisualBarrelErrorDegrees { get; private set; }

        public bool HasLastAimSolution => hasLastAimSolution;

        public CCS_WeaponAimSolution LastAimSolution => lastAimSolution;

        public Transform MuzzlePoint => muzzlePoint;

        #endregion

        #region Public Methods

        public void ApplySettings(CCS_RevolverVisualAimConvergenceSettings nextSettings)
        {
            settings = nextSettings;
        }

        public void BindEquippedVisual(Transform visualRoot)
        {
            equippedVisualRoot = visualRoot;
            muzzlePoint = visualRoot != null
                ? CCS_WeaponMuzzlePointUtility.FindMuzzlePoint(visualRoot)
                : null;
        }

        public void SetConvergenceActive(bool active)
        {
            if (!active && convergenceActive)
            {
                ResetConvergenceRotation();
            }

            convergenceActive = active;
        }

        public void ResetConvergenceRotation()
        {
            transform.localRotation = Quaternion.identity;
            LastVisualBarrelErrorDegrees = 0f;
        }

        public void TickConvergence(
            Camera aimCamera,
            Vector2 reticleViewportPoint,
            Transform fallbackMuzzle,
            float maxRange,
            LayerMask hitMask,
            Transform ignoreRoot,
            bool drawDebug)
        {
            if (!convergenceActive || !settings.EnableVisualAimConvergence || muzzlePoint == null || aimCamera == null)
            {
                hasLastAimSolution = false;
                LastVisualBarrelErrorDegrees = 0f;
                return;
            }

            lastAimSolution = CCS_WeaponAimResolver.Resolve(
                aimCamera,
                reticleViewportPoint,
                muzzlePoint,
                fallbackMuzzle,
                maxRange,
                hitMask,
                ignoreRoot);
            hasLastAimSolution = true;

            Vector3 desiredDirection = lastAimSolution.MuzzleToAimDirection;
            if (desiredDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 currentDirection = muzzlePoint.forward;
            LastVisualBarrelErrorDegrees = Vector3.Angle(currentDirection, desiredDirection);

            float strength = lastAimSolution.Distance <= settings.NearTargetDistance
                ? Mathf.Clamp01(lastAimSolution.Distance / Mathf.Max(0.01f, settings.NearTargetDistance))
                : 1f;

            if (LastVisualBarrelErrorDegrees <= 0.05f)
            {
                if (drawDebug)
                {
                    DrawConvergenceDebug(lastAimSolution, desiredDirection);
                }

                return;
            }

            Quaternion worldCorrection = Quaternion.FromToRotation(currentDirection, desiredDirection);
            Quaternion targetWorldRotation = worldCorrection * transform.rotation;
            float blend = Mathf.Clamp01(settings.ConvergenceSpeed * Time.deltaTime * strength);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetWorldRotation, blend);
            transform.localRotation = ClampLocalRotation(
                transform.localRotation,
                settings.MaxYawCorrectionDegrees,
                settings.MaxPitchCorrectionDegrees,
                settings.MaxRollCorrectionDegrees);

            LastVisualBarrelErrorDegrees = Vector3.Angle(muzzlePoint.forward, desiredDirection);

            if (drawDebug)
            {
                DrawConvergenceDebug(lastAimSolution, desiredDirection);
            }
        }

        #endregion

        #region Private Methods

        private void DrawConvergenceDebug(CCS_WeaponAimSolution aimSolution, Vector3 desiredDirection)
        {
            Debug.DrawRay(
                aimSolution.CameraRayOrigin,
                aimSolution.CameraRayDirection * aimSolution.Distance,
                Color.green);
            Debug.DrawRay(aimSolution.MuzzleOrigin, desiredDirection * aimSolution.Distance, Color.blue);
            Debug.DrawRay(aimSolution.MuzzleOrigin, muzzlePoint.forward * 0.35f, Color.yellow);
            DrawDebugPoint(aimSolution.AimPoint, Color.red, 0.05f);
            DrawDebugPoint(aimSolution.MuzzleOrigin, Color.cyan, 0.04f);
        }

        private static void DrawDebugPoint(Vector3 position, Color color, float size)
        {
            Debug.DrawLine(position - Vector3.right * size, position + Vector3.right * size, color);
            Debug.DrawLine(position - Vector3.up * size, position + Vector3.up * size, color);
            Debug.DrawLine(position - Vector3.forward * size, position + Vector3.forward * size, color);
        }

        private static Quaternion ClampLocalRotation(
            Quaternion localRotation,
            float maxYawDegrees,
            float maxPitchDegrees,
            float maxRollDegrees)
        {
            Vector3 euler = localRotation.eulerAngles;
            euler.x = NormalizeAngle(euler.x);
            euler.y = NormalizeAngle(euler.y);
            euler.z = NormalizeAngle(euler.z);
            euler.x = Mathf.Clamp(euler.x, -maxPitchDegrees, maxPitchDegrees);
            euler.y = Mathf.Clamp(euler.y, -maxYawDegrees, maxYawDegrees);
            euler.z = Mathf.Clamp(euler.z, -maxRollDegrees, maxRollDegrees);
            return Quaternion.Euler(euler);
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f)
            {
                angle -= 360f;
            }

            if (angle < -180f)
            {
                angle += 360f;
            }

            return angle;
        }

        #endregion
    }
}
