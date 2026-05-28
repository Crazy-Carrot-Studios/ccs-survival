using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalVitalsProfile
// CATEGORY: Survival / Runtime / Survival / Data
// PURPOSE: ScriptableObject tuning profile for Phase 1 survival vitals (health, hunger, thirst, stamina, damage rates).
// PLACEMENT: Create asset via Assets → Create → CCS → Survival → Survival Vitals Profile under Assets/CCS/Survival/Settings/Survival/.
// AUTHOR: James Schilz
// CREATED: 2026-05-27
// NOTES: Primary tuning source for CCS_SurvivalModule. Assign CCS_DefaultSurvivalVitalsProfile on bootstrap prefab.
// =============================================================================

namespace CCS.Survival
{
    [CreateAssetMenu(
        fileName = "CCS_SurvivalVitalsProfile",
        menuName = "CCS/Survival/Survival Vitals Profile")]
    public sealed class CCS_SurvivalVitalsProfile : ScriptableObject
    {
        #region Variables

        [Header("Vital Maximums")]
        [Tooltip("Maximum health value.")]
        [SerializeField] private float maxHealth = 100f;

        [Tooltip("Maximum hunger value.")]
        [SerializeField] private float maxHunger = 100f;

        [Tooltip("Maximum thirst value.")]
        [SerializeField] private float maxThirst = 100f;

        [Tooltip("Maximum stamina value.")]
        [SerializeField] private float maxStamina = 100f;

        [Tooltip("Default body temperature when vitals are initialized or respawned.")]
        [SerializeField] private float defaultBodyTemperature = 37f;

        [Header("Drain and Damage Rates")]
        [Tooltip("Hunger drained per second while alive.")]
        [SerializeField] private float hungerDrainRate = 8f;

        [Tooltip("Thirst drained per second while alive.")]
        [SerializeField] private float thirstDrainRate = 10f;

        [Tooltip("Health damage per second when hunger is depleted.")]
        [SerializeField] private float starvationDamageRate = 4f;

        [Tooltip("Health damage per second when thirst is depleted.")]
        [SerializeField] private float dehydrationDamageRate = 5f;

        [Tooltip("Health damage per second when exposure is above zero.")]
        [SerializeField] private float exposureDamageRate = 2f;

        [Tooltip("Stamina recovered per second while alive.")]
        [SerializeField] private float staminaRecoveryRate = 8f;

        [Header("Death and Respawn")]
        [Tooltip("Health restored as a percent of max health on respawn.")]
        [SerializeField] private float respawnHealthPercent = 50f;

        [Header("Event and Log Precision")]
        [Tooltip("Minimum vital change step used for meaningful event/log publishing (reduces per-frame spam).")]
        [SerializeField] private float meaningfulChangePrecision = 0.1f;

        #endregion

        #region Properties

        public float MaxHealth => maxHealth;

        public float MaxHunger => maxHunger;

        public float MaxThirst => maxThirst;

        public float MaxStamina => maxStamina;

        public float DefaultBodyTemperature => defaultBodyTemperature;

        public float HungerDrainRate => hungerDrainRate;

        public float ThirstDrainRate => thirstDrainRate;

        public float StarvationDamageRate => starvationDamageRate;

        public float DehydrationDamageRate => dehydrationDamageRate;

        public float ExposureDamageRate => exposureDamageRate;

        public float StaminaRecoveryRate => staminaRecoveryRate;

        public float RespawnHealthPercent => respawnHealthPercent;

        public float MeaningfulChangePrecision => meaningfulChangePrecision;

        #endregion

        #region Public Methods

        public void ValidateAndClamp()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            maxHunger = Mathf.Max(1f, maxHunger);
            maxThirst = Mathf.Max(1f, maxThirst);
            maxStamina = Mathf.Max(1f, maxStamina);
            defaultBodyTemperature = Mathf.Clamp(defaultBodyTemperature, 30f, 45f);
            hungerDrainRate = Mathf.Max(0f, hungerDrainRate);
            thirstDrainRate = Mathf.Max(0f, thirstDrainRate);
            starvationDamageRate = Mathf.Max(0f, starvationDamageRate);
            dehydrationDamageRate = Mathf.Max(0f, dehydrationDamageRate);
            exposureDamageRate = Mathf.Max(0f, exposureDamageRate);
            staminaRecoveryRate = Mathf.Max(0f, staminaRecoveryRate);
            respawnHealthPercent = Mathf.Clamp(respawnHealthPercent, 1f, 100f);
            meaningfulChangePrecision = Mathf.Clamp(meaningfulChangePrecision, 0.01f, 10f);
        }

        #endregion
    }
}
