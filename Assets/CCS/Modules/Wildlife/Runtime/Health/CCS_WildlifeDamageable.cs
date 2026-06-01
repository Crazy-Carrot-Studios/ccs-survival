using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeDamageable
// CATEGORY: Modules / Wildlife / Runtime / Health
// PURPOSE: Living wildlife health for primitive melee combat damage application.
// PLACEMENT: Same GameObject as CCS_WildlifeAgent on bootstrap test wildlife.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Death handling and carcass spawning are performed by CCS_CombatService.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_WildlifeDamageable : MonoBehaviour
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Display name used in combat notifications.")]
        [SerializeField] private string displayName = "Rabbit";

        [Tooltip("Species used to resolve health and carcass spawn rules.")]
        [SerializeField] private CCS_WildlifeAiSpecies species = CCS_WildlifeAiSpecies.Rabbit;

        [Tooltip("Maximum health for this living wildlife instance.")]
        [SerializeField] private float maxHealth = 20f;

        private CCS_WildlifeHealthState healthState = new CCS_WildlifeHealthState();
        private bool isConfigured;

        #endregion

        #region Properties

        public string DisplayName => displayName;

        public CCS_WildlifeAiSpecies Species => species;

        public float MaxHealth => maxHealth;

        public float CurrentHealth => healthState != null ? healthState.CurrentHealth : 0f;

        public bool IsDead => healthState != null && healthState.IsDead;

        #endregion

        #region Public Methods

        public void ConfigureForBootstrap(string agentDisplayName, CCS_WildlifeAiSpecies agentSpecies, float maximumHealth)
        {
            displayName = agentDisplayName;
            species = agentSpecies;
            maxHealth = maximumHealth;
            healthState.Initialize(maxHealth);
            isConfigured = true;
        }

        public float ApplyDamage(float damageAmount)
        {
            if (!isConfigured)
            {
                healthState.Initialize(maxHealth);
                isConfigured = true;
            }

            if (IsDead || damageAmount <= 0f)
            {
                return 0f;
            }

            return healthState.ApplyDamage(damageAmount);
        }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (!isConfigured && maxHealth > 0f)
            {
                healthState.Initialize(maxHealth);
                isConfigured = true;
            }
        }

        #endregion
    }
}
