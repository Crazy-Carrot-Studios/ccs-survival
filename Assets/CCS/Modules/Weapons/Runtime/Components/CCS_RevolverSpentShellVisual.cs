using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverSpentShellVisual
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Cosmetic spent shell casing ejected during reload/extraction only.
// PLACEMENT: PF_CCS_RevolverM1879_SpentShellVisual prefab root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Revolvers keep casings in the cylinder until reload. No per-shot ejection.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public sealed class CCS_RevolverSpentShellVisual : MonoBehaviour
    {
        #region Variables

        [SerializeField] private float lifetimeSeconds = 4f;
        [SerializeField] private float gravity = 4.5f;

        private Vector3 velocity;
        private Vector3 angularVelocityDegrees;
        private float destroyTime;
        private Renderer cachedRenderer;
        private Color startColor = Color.white;

        #endregion

        #region Public Methods

        public void Eject(Vector3 initialVelocity, Vector3 initialAngularVelocityDegrees, float lifetimeOverride = -1f)
        {
            velocity = initialVelocity;
            angularVelocityDegrees = initialAngularVelocityDegrees;
            lifetimeSeconds = lifetimeOverride > 0f ? lifetimeOverride : CCS_WeaponsConstants.DefaultShellVisualLifetime;
            destroyTime = Time.time + Random.Range(lifetimeSeconds * 0.75f, lifetimeSeconds);
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
            velocity += Vector3.down * gravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            transform.Rotate(angularVelocityDegrees * Time.deltaTime, Space.Self);

            float remaining = destroyTime - Time.time;
            if (remaining <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (cachedRenderer != null && remaining < 1f)
            {
                Color color = startColor;
                color.a = Mathf.Clamp01(remaining);
                cachedRenderer.material.color = color;
            }
        }

        #endregion
    }
}
