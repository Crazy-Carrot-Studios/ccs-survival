using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverBulletTracerVisual
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Cosmetic-only bullet/tracer that travels from muzzle toward aim point.
// PLACEMENT: PF_CCS_RevolverM1879_BulletTracerVisual prefab root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Visual-only. No colliders, damage, or gameplay authority.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public sealed class CCS_RevolverBulletTracerVisual : MonoBehaviour
    {
        #region Variables

        [SerializeField] private float moveSpeed = CCS_WeaponsConstants.DefaultBulletVisualSpeed;
        [SerializeField] private float maxLifetimeSeconds = CCS_WeaponsConstants.DefaultBulletVisualLifetime;
        [SerializeField] private bool trailEnabled = true;
        [SerializeField] private float trailLifetimeSeconds = CCS_WeaponsConstants.DefaultBulletTrailLifetime;
        [SerializeField] private float trailWidth = CCS_WeaponsConstants.DefaultBulletTrailWidth;

        private Vector3 travelDirection = Vector3.forward;
        private float targetDistance;
        private float traveledDistance;
        private float destroyTime;
        private TrailRenderer trailRenderer;

        #endregion

        #region Public Methods

        public void Launch(
            Vector3 start,
            Vector3 end,
            float speed,
            float lifetimeSeconds,
            bool enableTrail,
            float trailLifetime,
            float trailStartWidth)
        {
            transform.position = start;
            travelDirection = (end - start).sqrMagnitude > 0.0001f
                ? (end - start).normalized
                : transform.forward;
            transform.rotation = Quaternion.LookRotation(travelDirection);
            targetDistance = Vector3.Distance(start, end);
            traveledDistance = 0f;
            moveSpeed = Mathf.Max(1f, speed);
            maxLifetimeSeconds = Mathf.Max(0.05f, lifetimeSeconds);
            destroyTime = Time.time + maxLifetimeSeconds;

            ConfigureTrail(enableTrail, trailLifetime, trailStartWidth);
            if (trailRenderer != null && trailRenderer.enabled)
            {
                trailRenderer.Clear();
            }
        }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            EnsureTrailRenderer();
        }

        private void Update()
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position += travelDirection * step;
            traveledDistance += step;

            if (traveledDistance >= targetDistance || Time.time >= destroyTime)
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Private Methods

        private void EnsureTrailRenderer()
        {
            if (trailRenderer == null)
            {
                trailRenderer = GetComponent<TrailRenderer>();
            }

            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
            }
        }

        private void ConfigureTrail(bool enabled, float lifetime, float startWidth)
        {
            EnsureTrailRenderer();
            trailEnabled = enabled;
            trailLifetimeSeconds = lifetime;
            trailWidth = startWidth;

            trailRenderer.enabled = enabled;
            if (!enabled)
            {
                return;
            }

            float clampedLifetime = Mathf.Clamp(
                lifetime,
                CCS_WeaponsConstants.MinBulletTrailLifetime,
                CCS_WeaponsConstants.MaxBulletTrailLifetime);
            float clampedWidth = Mathf.Clamp(
                startWidth,
                CCS_WeaponsConstants.MinBulletTrailWidth,
                CCS_WeaponsConstants.MaxBulletTrailWidth);

            trailRenderer.time = clampedLifetime;
            trailRenderer.minVertexDistance = 0.004f;
            trailRenderer.autodestruct = false;
            trailRenderer.emitting = true;
            trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trailRenderer.receiveShadows = false;
            trailRenderer.generateLightingData = false;
            trailRenderer.alignment = LineAlignment.View;
            trailRenderer.textureMode = LineTextureMode.Stretch;

            AnimationCurve widthCurve = new AnimationCurve(
                new Keyframe(0f, clampedWidth, 0f, 0f),
                new Keyframe(0.35f, clampedWidth * 0.55f, 0f, 0f),
                new Keyframe(1f, clampedWidth * 0.05f, 0f, 0f));
            trailRenderer.widthCurve = widthCurve;
            trailRenderer.widthMultiplier = 1f;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.94f, 0.62f), 0f),
                    new GradientColorKey(new Color(1f, 0.72f, 0.28f), 0.35f),
                    new GradientColorKey(new Color(0.58f, 0.54f, 0.5f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.55f, 0.55f),
                    new GradientAlphaKey(0f, 1f)
                });
            trailRenderer.colorGradient = gradient;

            if (trailRenderer.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                    ?? Shader.Find("Particles/Standard Unlit")
                    ?? Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    trailRenderer.sharedMaterial = new Material(shader);
                }
            }
        }

        #endregion
    }
}
