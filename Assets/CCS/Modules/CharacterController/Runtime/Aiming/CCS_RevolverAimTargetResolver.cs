using System.IO;
using System.Text;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverAimTargetResolver
// CATEGORY: Modules / CharacterController / Runtime / Aiming
// PURPOSE: Resolves stable world-space aim target from local camera/mouse intent.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked / Model / Aiming.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Diagnostics and future-system only in v0.7.12. Does not drive IK, reticle, or fire.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(215)]
    public sealed class CCS_RevolverAimTargetResolver : MonoBehaviour, CCS_IRevolverAimTargetSource
    {
        private const float DefaultCameraRayDistance = 120f;
        private const float DefaultFallbackDistance = 80f;
        private const float DefaultTargetSmoothingTime = 0.045f;
        private const float DefaultMaxTargetSnapDistance = 8f;
        private const float DefaultLastValidTargetHoldSeconds = 0.2f;
        private const float DefaultMinimumValidDistance = 1f;
        private const float DefaultNearCameraRejectDistance = 0.35f;
        private const float ViewportCenter = 0.5f;

        [SerializeField] private CCS_RevolverAimTargetProfile aimTargetProfile;
        [SerializeField] private Camera aimCamera;

        private NetworkObject cachedNetworkObject;
        private Transform ignoreRoot;
        private Vector3 smoothedAimWorldPoint;
        private Vector3 smoothVelocity;
        private Vector3 lastValidAimWorldPoint;
        private float lastValidTargetTimestamp;
        private bool hasSmoothedAimWorldPoint;
        private bool hasLastValidAimWorldPoint;
        private bool hasValidAimTarget;
        private bool isObstructed;
        private bool usedFallbackTarget;
        private bool usedLastValidTarget;
        private bool isLocalOwnerContext;
        private bool loggedMissingCamera;
        private bool previousDebugDrawEnabled;
        private Ray lastCameraAimRay;
        private Vector3 rawAimWorldPoint;

        public bool HasValidAimTarget => hasValidAimTarget;

        public Vector3 AimWorldPoint => smoothedAimWorldPoint;

        public Vector3 AimDirection
        {
            get
            {
                if (!hasValidAimTarget || lastCameraAimRay.direction.sqrMagnitude < 0.0001f)
                {
                    return Vector3.forward;
                }

                Vector3 direction = smoothedAimWorldPoint - lastCameraAimRay.origin;
                return direction.sqrMagnitude > 0.0001f ? direction.normalized : lastCameraAimRay.direction;
            }
        }

        public float AimDistance
        {
            get
            {
                if (!hasValidAimTarget)
                {
                    return 0f;
                }

                return Vector3.Distance(lastCameraAimRay.origin, smoothedAimWorldPoint);
            }
        }

        public bool IsObstructed => isObstructed;

        public bool UsedFallbackTarget => usedFallbackTarget;

        public bool UsedLastValidTarget => usedLastValidTarget;

        public bool IsLocalOwnerContext => isLocalOwnerContext;

        public Ray LastCameraAimRay => lastCameraAimRay;

        public Vector3 RawAimWorldPoint => rawAimWorldPoint;

        public Vector3 SmoothedAimWorldPoint => smoothedAimWorldPoint;

        public Transform AimCameraTransform => aimCamera != null ? aimCamera.transform : null;

        private void Awake()
        {
            cachedNetworkObject = GetComponentInParent<NetworkObject>();
            ignoreRoot = transform.root;
            ResolveAimCamera();
        }

        private void LateUpdate()
        {
            isLocalOwnerContext = IsLocalPresentationOwner();
            if (!isLocalOwnerContext)
            {
                ResetAimTargetState();
                return;
            }

            Camera resolvedCamera = ResolveAimCamera();
            if (resolvedCamera == null)
            {
                ResetAimTargetState();
                return;
            }

            UpdateAimTarget(resolvedCamera);
            UpdateObstruction(resolvedCamera);
            DrawDebugRaysIfEnabled();
        }

        public void WriteRuntimeSnapshotReport()
        {
            string reportPath = ResolveReportPath(
                CCS_CharacterControllerConstants.AimTargetResolverRuntimeSnapshotReportPath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Aim Target Resolver Runtime Snapshot");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine("- IsLocalOwnerContext: " + isLocalOwnerContext);
            builder.AppendLine("- HasValidAimTarget: " + hasValidAimTarget);
            builder.AppendLine("- UsedFallbackTarget: " + usedFallbackTarget);
            builder.AppendLine("- UsedLastValidTarget: " + usedLastValidTarget);
            builder.AppendLine("- IsObstructed: " + isObstructed);
            builder.AppendLine("- AimDistance: " + AimDistance.ToString("0.###"));
            builder.AppendLine("- RawAimWorldPoint: " + FormatVector3(rawAimWorldPoint));
            builder.AppendLine("- SmoothedAimWorldPoint: " + FormatVector3(smoothedAimWorldPoint));
            builder.AppendLine("- CameraRayOrigin: " + FormatVector3(lastCameraAimRay.origin));
            builder.AppendLine("- CameraRayDirection: " + FormatVector3(lastCameraAimRay.direction));
            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
        }

        private void UpdateAimTarget(Camera resolvedCamera)
        {
            Vector3 cameraPosition = resolvedCamera.transform.position;
            lastCameraAimRay = resolvedCamera.ViewportPointToRay(
                new Vector3(ViewportCenter, ViewportCenter, 0f));

            float rayDistance = ResolveCameraRayDistance();
            Vector3 candidateRawPoint;
            usedFallbackTarget = false;
            usedLastValidTarget = false;

            if (TryRaycastAimTarget(lastCameraAimRay, rayDistance, out RaycastHit hit))
            {
                candidateRawPoint = hit.point;
            }
            else
            {
                candidateRawPoint = lastCameraAimRay.origin + (lastCameraAimRay.direction * ResolveFallbackDistance());
                usedFallbackTarget = true;
            }

            rawAimWorldPoint = candidateRawPoint;
            Vector3 resolvedPoint = candidateRawPoint;

            if (!IsProjectionValid(cameraPosition, candidateRawPoint))
            {
                if (ResolveHoldLastValidTargetWhenInvalid() && TryGetRecentValidTarget(out Vector3 recentValidTarget))
                {
                    resolvedPoint = recentValidTarget;
                    usedLastValidTarget = true;
                }
                else
                {
                    resolvedPoint = lastCameraAimRay.origin
                        + (lastCameraAimRay.direction * ResolveFallbackDistance());
                    usedFallbackTarget = true;
                }
            }

            if (ResolveSmoothTarget())
            {
                if (!hasSmoothedAimWorldPoint)
                {
                    smoothedAimWorldPoint = resolvedPoint;
                    smoothVelocity = Vector3.zero;
                    hasSmoothedAimWorldPoint = true;
                }
                else
                {
                    Vector3 previousSmoothed = smoothedAimWorldPoint;
                    smoothedAimWorldPoint = Vector3.SmoothDamp(
                        smoothedAimWorldPoint,
                        resolvedPoint,
                        ref smoothVelocity,
                        ResolveTargetSmoothingTime());

                    float maxSnap = ResolveMaxTargetSnapDistance();
                    Vector3 frameDelta = smoothedAimWorldPoint - previousSmoothed;
                    if (frameDelta.magnitude > maxSnap)
                    {
                        smoothedAimWorldPoint = previousSmoothed + frameDelta.normalized * maxSnap;
                        smoothVelocity = Vector3.zero;
                    }
                }
            }
            else
            {
                smoothedAimWorldPoint = resolvedPoint;
                smoothVelocity = Vector3.zero;
                hasSmoothedAimWorldPoint = true;
            }

            if (IsProjectionValid(cameraPosition, smoothedAimWorldPoint))
            {
                RememberValidTarget(smoothedAimWorldPoint);
                hasValidAimTarget = true;
            }
            else
            {
                hasValidAimTarget = false;
            }
        }

        private void UpdateObstruction(Camera resolvedCamera)
        {
            if (!hasValidAimTarget)
            {
                isObstructed = true;
                return;
            }

            Vector3 cameraPosition = resolvedCamera.transform.position;
            if (!IsProjectionValid(cameraPosition, smoothedAimWorldPoint))
            {
                isObstructed = true;
                return;
            }

            Vector3 toTarget = smoothedAimWorldPoint - cameraPosition;
            float targetDistance = toTarget.magnitude;
            if (targetDistance <= 0.001f)
            {
                isObstructed = true;
                return;
            }

            if (Physics.Raycast(
                    cameraPosition,
                    toTarget / targetDistance,
                    out RaycastHit obstructionHit,
                    targetDistance,
                    ResolveObstructionLayerMask(),
                    QueryTriggerInteraction.Ignore)
                && obstructionHit.collider != null
                && !IsIgnoredCollider(obstructionHit.collider))
            {
                isObstructed = true;
                return;
            }

            isObstructed = false;
        }

        private bool TryRaycastAimTarget(Ray ray, float maxDistance, out RaycastHit closestHit)
        {
            closestHit = default;
            RaycastHit[] hits = Physics.RaycastAll(
                ray.origin,
                ray.direction,
                maxDistance,
                ResolveAimLayerMask(),
                QueryTriggerInteraction.Ignore);

            if (hits.Length == 0)
            {
                return false;
            }

            float closestDistance = float.MaxValue;
            bool found = false;
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider == null || IsIgnoredCollider(hit.collider))
                {
                    continue;
                }

                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestHit = hit;
                    found = true;
                }
            }

            return found;
        }

        private bool IsProjectionValid(Vector3 cameraPosition, Vector3 worldPoint)
        {
            if (float.IsNaN(worldPoint.x) || float.IsNaN(worldPoint.y) || float.IsNaN(worldPoint.z))
            {
                return false;
            }

            if (float.IsInfinity(worldPoint.x) || float.IsInfinity(worldPoint.y) || float.IsInfinity(worldPoint.z))
            {
                return false;
            }

            Vector3 toPoint = worldPoint - cameraPosition;
            float distance = toPoint.magnitude;
            if (distance < ResolveNearCameraRejectDistance())
            {
                return false;
            }

            if (distance < ResolveMinimumValidDistance())
            {
                return false;
            }

            if (Vector3.Dot(cameraPosition + lastCameraAimRay.direction, toPoint) <= 0f)
            {
                return false;
            }

            Vector3 cameraForward = lastCameraAimRay.direction;
            if (Vector3.Dot(cameraForward.normalized, toPoint.normalized) <= 0f)
            {
                return false;
            }

            return true;
        }

        private void RememberValidTarget(Vector3 worldPoint)
        {
            lastValidAimWorldPoint = worldPoint;
            lastValidTargetTimestamp = Time.unscaledTime;
            hasLastValidAimWorldPoint = true;
        }

        private bool TryGetRecentValidTarget(out Vector3 worldPoint)
        {
            worldPoint = lastValidAimWorldPoint;
            if (!hasLastValidAimWorldPoint)
            {
                return false;
            }

            return Time.unscaledTime - lastValidTargetTimestamp <= ResolveLastValidTargetHoldSeconds();
        }

        private void ResetAimTargetState()
        {
            hasValidAimTarget = false;
            isObstructed = false;
            usedFallbackTarget = false;
            usedLastValidTarget = false;
            hasSmoothedAimWorldPoint = false;
            smoothVelocity = Vector3.zero;
            rawAimWorldPoint = Vector3.zero;
            smoothedAimWorldPoint = Vector3.zero;
        }

        private bool IsLocalPresentationOwner()
        {
            if (cachedNetworkObject == null)
            {
                cachedNetworkObject = GetComponentInParent<NetworkObject>();
            }

            NetworkObject networkObject = cachedNetworkObject;
            if (networkObject == null || !networkObject.IsSpawned)
            {
                return true;
            }

            return networkObject.IsOwner;
        }

        private Camera ResolveAimCamera()
        {
            if (aimCamera != null && aimCamera.isActiveAndEnabled)
            {
                return aimCamera;
            }

            if (CCS_CharacterMovementCameraContext.HasActiveCamera)
            {
                aimCamera = CCS_CharacterMovementCameraContext.ActiveCamera;
                return aimCamera;
            }

            aimCamera = Camera.main;
            if (aimCamera == null && !loggedMissingCamera)
            {
                loggedMissingCamera = true;
                Debug.LogWarning("[Revolver Aim Target Resolver] Missing active aim camera.", this);
            }

            return aimCamera;
        }

        private bool IsIgnoredCollider(Collider collider)
        {
            return ignoreRoot != null && collider.transform.IsChildOf(ignoreRoot);
        }

        private void DrawDebugRaysIfEnabled()
        {
            bool debugEnabled = ShouldDrawDebugRays();
            if (debugEnabled && !previousDebugDrawEnabled)
            {
                WriteRuntimeSnapshotReport();
            }

            previousDebugDrawEnabled = debugEnabled;
            if (!debugEnabled || !hasValidAimTarget)
            {
                return;
            }

            float duration = Time.deltaTime + 0.01f;
            Debug.DrawRay(lastCameraAimRay.origin, lastCameraAimRay.direction * ResolveCameraRayDistance(), Color.cyan, duration);
            Debug.DrawLine(lastCameraAimRay.origin, rawAimWorldPoint, usedFallbackTarget ? Color.yellow : Color.green, duration);
            Debug.DrawLine(rawAimWorldPoint, smoothedAimWorldPoint, Color.magenta, duration);
            if (isObstructed)
            {
                Debug.DrawLine(lastCameraAimRay.origin, smoothedAimWorldPoint, Color.red, duration);
            }
        }

        private bool ShouldDrawDebugRays()
        {
            if (aimTargetProfile == null || !aimTargetProfile.DrawDebugRayWhenDiagnosticsEnabled)
            {
                return false;
            }

            return CCS_AimPresentationDiagnosticsRegistry.EnableAimTargetDebugRays;
        }

        private LayerMask ResolveAimLayerMask()
        {
            return aimTargetProfile != null ? aimTargetProfile.AimLayerMask : Physics.DefaultRaycastLayers;
        }

        private LayerMask ResolveObstructionLayerMask()
        {
            return aimTargetProfile != null ? aimTargetProfile.ObstructionLayerMask : Physics.DefaultRaycastLayers;
        }

        private float ResolveCameraRayDistance()
        {
            return aimTargetProfile != null ? aimTargetProfile.CameraRayDistance : DefaultCameraRayDistance;
        }

        private float ResolveFallbackDistance()
        {
            return aimTargetProfile != null ? aimTargetProfile.FallbackDistance : DefaultFallbackDistance;
        }

        private float ResolveTargetSmoothingTime()
        {
            return aimTargetProfile != null ? aimTargetProfile.TargetSmoothingTime : DefaultTargetSmoothingTime;
        }

        private float ResolveMaxTargetSnapDistance()
        {
            return aimTargetProfile != null ? aimTargetProfile.MaxTargetSnapDistance : DefaultMaxTargetSnapDistance;
        }

        private float ResolveLastValidTargetHoldSeconds()
        {
            return aimTargetProfile != null
                ? aimTargetProfile.LastValidTargetHoldSeconds
                : DefaultLastValidTargetHoldSeconds;
        }

        private float ResolveMinimumValidDistance()
        {
            return aimTargetProfile != null ? aimTargetProfile.MinimumValidDistance : DefaultMinimumValidDistance;
        }

        private float ResolveNearCameraRejectDistance()
        {
            return aimTargetProfile != null
                ? aimTargetProfile.NearCameraRejectDistance
                : DefaultNearCameraRejectDistance;
        }

        private bool ResolveHoldLastValidTargetWhenInvalid()
        {
            return aimTargetProfile == null || aimTargetProfile.HoldLastValidTargetWhenInvalid;
        }

        private bool ResolveSmoothTarget()
        {
            return aimTargetProfile == null || aimTargetProfile.SmoothTarget;
        }

        private static string FormatVector3(Vector3 value)
        {
            return "(" + value.x.ToString("0.###") + ", " + value.y.ToString("0.###") + ", " + value.z.ToString("0.###") + ")";
        }

        private static string ResolveReportPath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
