using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverMuzzleFlashVisual
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Short-lived cosmetic muzzle flash at revolver barrel.
// PLACEMENT: PF_CCS_RevolverM1879_MuzzleFlash prefab root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Visual-only. No gameplay logic.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public sealed class CCS_RevolverMuzzleFlashVisual : MonoBehaviour
    {
        #region Variables

        [SerializeField] private float lifetimeSeconds = 0.06f;
        [SerializeField] private float startScale = 0.04f;
        [SerializeField] private float endScale = 0.09f;

        private float spawnTime;
        private Renderer cachedRenderer;
        private Color startColor = new Color(1f, 0.85f, 0.35f, 1f);

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
                : Random.Range(CCS_WeaponsConstants.MuzzleFlashLifetimeMin, CCS_WeaponsConstants.MuzzleFlashLifetimeMax);
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
                color.a = Mathf.Lerp(1f, 0f, normalized);
                cachedRenderer.material.color = color;
            }
        }

        #endregion
    }
}
