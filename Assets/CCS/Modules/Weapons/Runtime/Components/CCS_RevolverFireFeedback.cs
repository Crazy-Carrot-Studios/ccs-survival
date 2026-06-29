using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverFireFeedback
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Cosmetic revolver fire visuals — tracer, muzzle flash, smoke; reload shell extraction.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked (MuzzlePoint or root).
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses HitscanResult.RayOrigin toward hit/aim point. Revolvers do not eject shells per shot.
//        Spent casings remain in the cylinder until reload/extraction.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [DefaultExecutionOrder(130)]
    public sealed class CCS_RevolverFireFeedback : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_RevolverController revolverController;
        [SerializeField] private CCS_PlayerEquipmentVisualController equipmentVisualController;
        [SerializeField] private Transform muzzlePoint;

        [Header("Fire Visuals")]
        [SerializeField] private GameObject bulletTracerPrefab;
        [SerializeField] private float bulletVisualScaleMultiplier = CCS_WeaponsConstants.DefaultBulletVisualScaleMultiplier;
        [SerializeField] private bool bulletTrailEnabled = true;
        [SerializeField] private float bulletTrailLifetime = CCS_WeaponsConstants.DefaultBulletTrailLifetime;
        [SerializeField] private float bulletTrailWidth = CCS_WeaponsConstants.DefaultBulletTrailWidth;
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private GameObject muzzleSmokePrefab;
        [SerializeField] private GameObject spentShellPrefab;
        [SerializeField] private float spentShellVisualScaleMultiplier =
            CCS_WeaponsConstants.DefaultSpentShellVisualScaleMultiplier;
        [SerializeField] private bool debugFireVisuals;

        [SerializeField] private float bulletTracerSpeed = CCS_WeaponsConstants.DefaultBulletVisualSpeed;
        [SerializeField] private float bulletTracerLifetime = CCS_WeaponsConstants.DefaultBulletVisualLifetime;
        [SerializeField] private float tracerDurationSeconds = 0.12f;
        [SerializeField] private float tracerWidth = 0.035f;
        [SerializeField] private Color tracerColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private bool enableLineTracerFallback = true;

        private LineRenderer lineRenderer;
        private float tracerEndTime;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
            EnsureLineRenderer();
        }

        private void OnEnable()
        {
            if (revolverController != null)
            {
                revolverController.FireResolved += HandleFireResolved;
                revolverController.RevolverReloadStarted += HandleReloadStarted;
            }
        }

        private void OnDisable()
        {
            if (revolverController != null)
            {
                revolverController.FireResolved -= HandleFireResolved;
                revolverController.RevolverReloadStarted -= HandleReloadStarted;
            }

            HideTracer();
        }

        private void Update()
        {
            if (lineRenderer == null || !lineRenderer.enabled)
            {
                return;
            }

            if (Time.time >= tracerEndTime)
            {
                HideTracer();
            }
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (revolverController == null)
            {
                revolverController = GetComponentInParent<CCS_RevolverController>();
            }

            if (equipmentVisualController == null)
            {
                equipmentVisualController = GetComponentInParent<CCS_PlayerEquipmentVisualController>();
            }

            if (muzzlePoint == null)
            {
                muzzlePoint = transform;
            }

            if (muzzlePoint == transform.root)
            {
                Transform resolvedMuzzle = FindDeepChild(transform.root, CCS_WeaponsConstants.MuzzlePointObjectName);
                if (resolvedMuzzle != null)
                {
                    muzzlePoint = resolvedMuzzle;
                }
            }
        }

        private void EnsureLineRenderer()
        {
            Transform lineHost = muzzlePoint != null ? muzzlePoint : transform;
            lineRenderer = lineHost.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = lineHost.gameObject.AddComponent<LineRenderer>();
            }

            lineRenderer.enabled = false;
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = tracerWidth;
            lineRenderer.endWidth = tracerWidth * 0.35f;
            lineRenderer.numCapVertices = 4;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = tracerColor;
            lineRenderer.endColor = tracerColor;
        }

        private void HandleFireResolved(CCS_RevolverFireResultEvent fireResultEvent)
        {
            if (fireResultEvent.WasDryFire)
            {
                return;
            }

            CCS_WeaponHitscanResult hitscan = fireResultEvent.HitscanResult;
            Vector3 start = hitscan.RayOrigin;
            Vector3 end = hitscan.DidHit
                ? hitscan.HitPoint
                : start + (hitscan.RayDirection * Mathf.Max(1f, hitscan.HitDistance));
            Vector3 shotDirection = (end - start).sqrMagnitude > 0.0001f
                ? (end - start).normalized
                : hitscan.RayDirection;

            bool tracerSpawned = SpawnBulletTracer(start, end);
            bool flashSpawned = SpawnMuzzleFlash(start, shotDirection);
            bool smokeSpawned = SpawnMuzzleSmoke(start, shotDirection);

            if (enableLineTracerFallback && !tracerSpawned)
            {
                ShowLineTracer(start, end);
            }

            if (debugFireVisuals)
            {
                string hitLabel = hitscan.DidHit && hitscan.HitObject != null
                    ? hitscan.HitObject.name
                    : "miss";
                string muzzleSource = equipmentVisualController != null
                    && equipmentVisualController.HasEquippedMuzzlePoint
                    ? "Visual"
                    : "Fallback";
                Debug.Log(
                    "[Revolver Fire Visuals] muzzle="
                    + muzzleSource
                    + " tracer="
                    + (tracerSpawned ? "Spawned" : "FallbackLine")
                    + " flash="
                    + (flashSpawned ? "Spawned" : "Missing")
                    + " smoke="
                    + (smokeSpawned ? "Spawned" : "Missing")
                    + " hit="
                    + hitLabel,
                    this);
            }
        }

        private void HandleReloadStarted()
        {
            if (revolverController == null || spentShellPrefab == null)
            {
                return;
            }

            int spentShellCount = Mathf.Clamp(
                revolverController.MaxAmmo - revolverController.CurrentAmmo,
                0,
                revolverController.MaxAmmo);
            if (spentShellCount <= 0)
            {
                return;
            }

            Transform ejectPoint = ResolveShellEjectPoint();
            Vector3 spawnPosition = ejectPoint != null ? ejectPoint.position : ResolveFallbackMuzzleOrigin();
            Vector3 ejectForward = ejectPoint != null ? ejectPoint.forward : transform.forward;
            Vector3 ejectRight = ejectPoint != null ? ejectPoint.right : transform.right;

            for (int i = 0; i < spentShellCount; i++)
            {
                SpawnSpentShell(spawnPosition, ejectForward, ejectRight);
            }

            if (debugFireVisuals)
            {
                Debug.Log(
                    "[Revolver Fire Visuals] reload shell extraction count="
                    + spentShellCount
                    + " (revolver keeps casings until reload; no per-shot ejection).",
                    this);
            }
        }

        private bool SpawnBulletTracer(Vector3 start, Vector3 end)
        {
            if (bulletTracerPrefab == null)
            {
                return false;
            }

            GameObject instance = Instantiate(bulletTracerPrefab, start, Quaternion.identity);
            float scale = Mathf.Clamp(
                bulletVisualScaleMultiplier,
                CCS_WeaponsConstants.MinBulletVisualScaleMultiplier,
                CCS_WeaponsConstants.MaxBulletVisualScaleMultiplier);
            instance.transform.localScale = Vector3.one * scale;

            CCS_RevolverBulletTracerVisual tracerVisual = instance.GetComponent<CCS_RevolverBulletTracerVisual>();
            if (tracerVisual == null)
            {
                tracerVisual = instance.AddComponent<CCS_RevolverBulletTracerVisual>();
            }

            tracerVisual.Launch(
                start,
                end,
                bulletTracerSpeed,
                bulletTracerLifetime,
                bulletTrailEnabled,
                bulletTrailLifetime,
                bulletTrailWidth);
            return true;
        }

        private bool SpawnMuzzleFlash(Vector3 position, Vector3 forward)
        {
            if (muzzleFlashPrefab == null)
            {
                return false;
            }

            GameObject instance = Instantiate(muzzleFlashPrefab, position, Quaternion.LookRotation(forward));
            CCS_RevolverMuzzleFlashVisual flashVisual = instance.GetComponent<CCS_RevolverMuzzleFlashVisual>();
            if (flashVisual == null)
            {
                flashVisual = instance.AddComponent<CCS_RevolverMuzzleFlashVisual>();
            }

            flashVisual.Play(position, forward);
            return true;
        }

        private bool SpawnMuzzleSmoke(Vector3 position, Vector3 forward)
        {
            if (muzzleSmokePrefab == null)
            {
                return false;
            }

            GameObject instance = Instantiate(muzzleSmokePrefab, position, Quaternion.LookRotation(forward));
            CCS_RevolverMuzzleSmokeVisual smokeVisual = instance.GetComponent<CCS_RevolverMuzzleSmokeVisual>();
            if (smokeVisual == null)
            {
                smokeVisual = instance.AddComponent<CCS_RevolverMuzzleSmokeVisual>();
            }

            smokeVisual.Play(position, forward);
            return true;
        }

        private void SpawnSpentShell(Vector3 spawnPosition, Vector3 ejectForward, Vector3 ejectRight)
        {
            Vector3 offset = ejectRight * Random.Range(-0.015f, 0.015f)
                + Vector3.up * Random.Range(-0.01f, 0.02f)
                + ejectForward * Random.Range(-0.01f, 0.02f);
            Vector3 velocity = ejectForward * Random.Range(0.35f, 0.75f)
                + ejectRight * Random.Range(-0.25f, 0.35f)
                + Vector3.down * Random.Range(0.05f, 0.2f);
            Vector3 angularVelocity = new Vector3(
                Random.Range(-180f, 180f),
                Random.Range(-240f, 240f),
                Random.Range(-180f, 180f));

            GameObject instance = Instantiate(
                spentShellPrefab,
                spawnPosition + offset,
                Random.rotation);
            float scale = Mathf.Clamp(
                spentShellVisualScaleMultiplier,
                CCS_WeaponsConstants.MinSpentShellVisualScaleMultiplier,
                CCS_WeaponsConstants.MaxSpentShellVisualScaleMultiplier);
            instance.transform.localScale = Vector3.one * scale;

            CCS_RevolverSpentShellVisual shellVisual = instance.GetComponent<CCS_RevolverSpentShellVisual>();
            if (shellVisual == null)
            {
                shellVisual = instance.AddComponent<CCS_RevolverSpentShellVisual>();
            }

            shellVisual.Eject(velocity, angularVelocity);
        }

        private Transform ResolveShellEjectPoint()
        {
            if (equipmentVisualController != null
                && equipmentVisualController.CurrentEquippedMuzzlePoint != null)
            {
                Transform visualRoot = equipmentVisualController.CurrentEquippedMuzzlePoint.root;
                Transform shellEject = CCS_WeaponVisualAnchorUtility.FindShellEjectPoint(visualRoot);
                if (shellEject != null)
                {
                    return shellEject;
                }
            }

            return FindDeepChild(transform.root, CCS_WeaponsConstants.ShellEjectPointObjectName);
        }

        private Vector3 ResolveFallbackMuzzleOrigin()
        {
            if (equipmentVisualController != null && equipmentVisualController.HasEquippedMuzzlePoint)
            {
                return equipmentVisualController.CurrentEquippedMuzzlePoint.position;
            }

            return muzzlePoint != null ? muzzlePoint.position : transform.position;
        }

        private void ShowLineTracer(Vector3 start, Vector3 end)
        {
            EnsureLineRenderer();
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.enabled = true;
            tracerEndTime = Time.time + tracerDurationSeconds;
        }

        private void HideTracer()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeepChild(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        #endregion
    }
}
