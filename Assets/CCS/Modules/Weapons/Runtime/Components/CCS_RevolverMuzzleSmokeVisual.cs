using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverMuzzleSmokeVisual
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Short-lived cosmetic muzzle smoke puff at revolver barrel.
// PLACEMENT: PF_CCS_RevolverM1879_MuzzleSmoke prefab root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Visual-only. Subtle test harness puff, not final VFX.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public sealed class CCS_RevolverMuzzleSmokeVisual : MonoBehaviour
    {
        #region Variables

        [SerializeField] private float lifetimeSeconds = 0.9f;
        [SerializeField] private float startScale = 0.03f;
        [SerializeField] private float endScale = 0.12f;
        [SerializeField] private Vector3 driftVelocity = new Vector3(0f, 0.15f, 0.05f);

        private float spawnTime;
        private Renderer cachedRenderer;
        private Color startColor = new Color(0.65f, 0.65f, 0.65f, 0.55f);

        #endregion

        #region Public Methods

        public void Play(Vector3 position, Vector3 forward, float lifetimeOverride = -1f)
        {
            transform.SetPositionAndRotation(
                position,
                forward.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(forward) : transform.rotation);
            spawnTime = Time.time;
            lifetimeSeconds = lifetimeOverride > 0f
                ? lifetimeOverride
                : Random.Range(CCS_WeaponsConstants.MuzzleSmokeLifetimeMin, CCS_WeaponsConstants.MuzzleSmokeLifetimeMax);
            transform.localScale = Vector3.one * startScale;
            cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer != null)
            {
                startColor = cachedRenderer.material.color;
            }
        }

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            float elapsed = Time.time - spawnTime;
            float normalized = lifetimeSeconds <= 0f ? 1f : elapsed / lifetimeSeconds;
            transform.position += transform.TransformDirection(driftVelocity) * Time.deltaTime;

            if (normalized >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            float scale = Mathf.Lerp(startScale, endScale, normalized);
            transform.localScale = Vector3.one * scale;

            if (cachedRenderer != null)
            {
                Color color = startColor;
                color.a = Mathf.Lerp(startColor.a, 0f, normalized);
                cachedRenderer.material.color = color;
            }
        }

        #endregion
    }
}
