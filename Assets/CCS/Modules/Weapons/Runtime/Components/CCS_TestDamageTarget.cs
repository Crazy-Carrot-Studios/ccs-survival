using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestDamageTarget
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Simple Master Test damage target for revolver hitscan validation.
// PLACEMENT: PF_CCS_TestWeaponDamageTarget prefab in Master Test scene.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.0 test-only. Future combat can bridge into Attributes damage pipeline.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public sealed class CCS_TestDamageTarget : MonoBehaviour
    {
        #region Variables

        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool resetOnPlay = true;
        [SerializeField] private bool enableWeaponDebugLogs;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color healthyColor = new Color(0.35f, 0.75f, 0.35f, 1f);
        [SerializeField] private Color damagedColor = new Color(0.85f, 0.65f, 0.2f, 1f);
        [SerializeField] private Color deadColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        [SerializeField] private float deadScaleMultiplier = 0.85f;

        private float currentHealth;
        private Vector3 initialScale;
        private Material runtimeMaterial;

        #endregion

        #region Properties

        public float MaxHealth => maxHealth;

        public float CurrentHealth => currentHealth;

        public bool IsDead => currentHealth <= 0f;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            initialScale = transform.localScale;
            ResolveRenderer();
            CreateRuntimeMaterialIfNeeded();
        }

        private void Start()
        {
            if (resetOnPlay)
            {
                ResetHealth();
            }
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }

        #endregion

        #region Public Methods

        public void ApplyWeaponDamage(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            UpdateVisualState();

            if (enableWeaponDebugLogs)
            {
                Debug.Log(
                    $"[Weapons] Damage target hit for {amount:0.#}. Health={currentHealth:0.#}/{maxHealth:0.#}",
                    this);
            }
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            transform.localScale = initialScale;
            UpdateVisualState();
        }

        #endregion

        #region Private Methods

        private void ResolveRenderer()
        {
            if (targetRenderer != null)
            {
                return;
            }

            targetRenderer = GetComponentInChildren<Renderer>();
        }

        private void CreateRuntimeMaterialIfNeeded()
        {
            if (targetRenderer == null)
            {
                return;
            }

            runtimeMaterial = targetRenderer.material;
        }

        private void UpdateVisualState()
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            if (IsDead)
            {
                runtimeMaterial.color = deadColor;
                transform.localScale = initialScale * deadScaleMultiplier;
                return;
            }

            float healthRatio = maxHealth > 0f ? currentHealth / maxHealth : 0f;
            runtimeMaterial.color = Color.Lerp(damagedColor, healthyColor, healthRatio);
            transform.localScale = initialScale;
        }

        #endregion
    }
}
