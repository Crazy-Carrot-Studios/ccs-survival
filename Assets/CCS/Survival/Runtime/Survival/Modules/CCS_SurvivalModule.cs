using System;
using CCS.Core;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalModule
// CATEGORY: Survival / Runtime / Survival / Modules
// PURPOSE: Phase 1A survival vitals MonoBehaviour module implementing CCS_ISurvivalVitalsService.
// PLACEMENT: Attach to PF_CCS_Survival_BootstrapRoot with CCS_RuntimeHost (composition root). Registers CCS_ISurvivalVitalsService on Awake.
// AUTHOR: James Schilz
// CREATED: 2026-05-27
// NOTES: Skeleton only. No final UI, weather, inventory, networking, or save/load. See CCS_Survival_Phase_01_Survival_Core.md.
// =============================================================================

namespace CCS.Survival
{
    [DefaultExecutionOrder(50)]
    public sealed class CCS_SurvivalModule : MonoBehaviour, CCS_ISurvivalVitalsService
    {
        private const string LogCategory = CCS_SurvivalVitalsDiagnostics.LogCategory;

        #region Variables

        [Header("Service Registration")]
        [Tooltip("Runtime host for service registry registration. Resolves on this GameObject, then parent, when unset.")]
        [SerializeField] private CCS_RuntimeHost runtimeHost;

        [Header("Vital Maximums")]
        [Tooltip("Maximum health value.")]
        [SerializeField] private float maxHealth = 100f;

        [Tooltip("Maximum hunger value.")]
        [SerializeField] private float maxHunger = 100f;

        [Tooltip("Maximum thirst value.")]
        [SerializeField] private float maxThirst = 100f;

        [Tooltip("Maximum stamina value.")]
        [SerializeField] private float maxStamina = 100f;

        [Header("Drain and Damage Rates")]
        [Tooltip("Hunger drained per second while alive.")]
        [SerializeField] private float hungerDrainRate = 0.5f;

        [Tooltip("Thirst drained per second while alive.")]
        [SerializeField] private float thirstDrainRate = 0.75f;

        [Tooltip("Health damage per second when hunger is depleted.")]
        [SerializeField] private float starvationDamageRate = 2f;

        [Tooltip("Health damage per second when thirst is depleted.")]
        [SerializeField] private float dehydrationDamageRate = 3f;

        [Tooltip("Health damage per second when exposure is above zero.")]
        [SerializeField] private float exposureDamageRate = 1f;

        [Tooltip("Stamina recovered per second while alive.")]
        [SerializeField] private float staminaRecoveryRate = 8f;

        [Header("Death and Respawn")]
        [Tooltip("Health restored as a percent of max health on respawn.")]
        [SerializeField] private float respawnHealthPercent = 50f;

        [Header("Debug")]
        [Tooltip("When enabled, survival state changes are written to the Unity console.")]
        [SerializeField] private bool enableDebugLogs;

        private CCS_SurvivalState survivalState;
        private bool isInitialized;
        private bool isServiceRegistered;

        #endregion

        #region Events

        public event Action<CCS_SurvivalState> OnSurvivalStateChanged;

        public event Action<float> OnHealthChanged;

        public event Action<float> OnHungerChanged;

        public event Action<float> OnThirstChanged;

        public event Action<float> OnStaminaChanged;

        public event Action<float> OnTemperatureChanged;

        public event Action OnPlayerDied;

        public event Action OnPlayerRespawned;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            Initialize();
            TryRegisterSurvivalService();
            PublishSurvivalStateChanged();
        }

        private void OnDestroy()
        {
            TryUnregisterSurvivalService();
            isInitialized = false;
        }

        private void Update()
        {
            if (!survivalState.IsAlive)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            UpdateHungerAndThirst(deltaTime);
            ApplyEnvironmentalDamage(deltaTime);
            RecoverStamina(deltaTime);
            EvaluateDeathState();
        }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            survivalState = CCS_SurvivalState.CreateDefault();
            ClampStateToMaximums();
            isInitialized = true;
            PublishTemperatureChanged(0f, survivalState.BodyTemperature);
            LogDebug("Survival vitals service initialized.");
        }

        public void ApplyDamage(float amount)
        {
            if (!survivalState.IsAlive || amount <= 0f)
            {
                return;
            }

            float previousHealth = survivalState.Health;
            survivalState.Health = Mathf.Max(0f, survivalState.Health - amount);
            PublishHealthChanged(previousHealth, survivalState.Health);
            EvaluateDeathState();
        }

        public void RestoreHealth(float amount)
        {
            if (!survivalState.IsAlive || amount <= 0f)
            {
                return;
            }

            float previousHealth = survivalState.Health;
            survivalState.Health = Mathf.Min(maxHealth, survivalState.Health + amount);
            PublishHealthChanged(previousHealth, survivalState.Health);
            PublishSurvivalStateChanged();
        }

        public void ConsumeFood(float nutritionValue)
        {
            if (!survivalState.IsAlive || nutritionValue <= 0f)
            {
                return;
            }

            float previousHunger = survivalState.Hunger;
            survivalState.Hunger = Mathf.Min(maxHunger, survivalState.Hunger + nutritionValue);
            PublishHungerChanged(previousHunger, survivalState.Hunger);
            PublishSurvivalStateChanged();
        }

        public void ConsumeWater(float hydrationValue)
        {
            if (!survivalState.IsAlive || hydrationValue <= 0f)
            {
                return;
            }

            float previousThirst = survivalState.Thirst;
            survivalState.Thirst = Mathf.Min(maxThirst, survivalState.Thirst + hydrationValue);
            PublishThirstChanged(previousThirst, survivalState.Thirst);
            PublishSurvivalStateChanged();
        }

        public void Kill()
        {
            if (!survivalState.IsAlive)
            {
                return;
            }

            survivalState.Health = 0f;
            survivalState.IsAlive = false;
            PublishHealthChanged(maxHealth, survivalState.Health);
            OnPlayerDied?.Invoke();
            PublishSurvivalStateChanged();
            LogDebug("Player entered dead state (Kill).");
        }

        public void Respawn()
        {
            float previousTemperature = survivalState.BodyTemperature;
            survivalState = CCS_SurvivalState.CreateDefault();
            survivalState.Health = maxHealth * Mathf.Clamp01(respawnHealthPercent / 100f);
            ClampStateToMaximums();
            OnPlayerRespawned?.Invoke();
            PublishSurvivalStateChanged();
            PublishHealthChanged(0f, survivalState.Health);
            PublishHungerChanged(0f, survivalState.Hunger);
            PublishThirstChanged(0f, survivalState.Thirst);
            PublishStaminaChanged(0f, survivalState.Stamina);
            PublishTemperatureChanged(previousTemperature, survivalState.BodyTemperature);
            LogDebug("Player respawned with default vitals.");
        }

        public void SetBodyTemperature(float bodyTemperature)
        {
            float previousTemperature = survivalState.BodyTemperature;
            survivalState.BodyTemperature = bodyTemperature;
            PublishTemperatureChanged(previousTemperature, survivalState.BodyTemperature);
        }

        public void SetExposure(float exposureValue)
        {
            float previousExposure = survivalState.Exposure;
            survivalState.Exposure = Mathf.Max(0f, exposureValue);
            if (!Mathf.Approximately(previousExposure, survivalState.Exposure))
            {
                PublishSurvivalStateChanged();
            }
        }

        [ContextMenu("Debug/Apply Test Damage")]
        public void DebugApplyTestDamage()
        {
            EnsureInitializedForDebug();
            ApplyDamage(15f);
        }

        [ContextMenu("Debug/Restore Health")]
        public void DebugRestoreHealth()
        {
            EnsureInitializedForDebug();
            RestoreHealth(25f);
        }

        [ContextMenu("Debug/Consume Test Food")]
        public void DebugConsumeTestFood()
        {
            EnsureInitializedForDebug();
            ConsumeFood(40f);
        }

        [ContextMenu("Debug/Consume Test Water")]
        public void DebugConsumeTestWater()
        {
            EnsureInitializedForDebug();
            ConsumeWater(40f);
        }

        [ContextMenu("Debug/Kill Player")]
        public void DebugKillPlayer()
        {
            EnsureInitializedForDebug();
            Kill();
        }

        [ContextMenu("Debug/Respawn Player")]
        public void DebugRespawnPlayer()
        {
            EnsureInitializedForDebug();
            Respawn();
        }

        [ContextMenu("Debug/Apply Test Exposure")]
        public void DebugApplyTestExposure()
        {
            EnsureInitializedForDebug();
            SetExposure(1f);
        }

        [ContextMenu("Debug/Clear Exposure")]
        public void DebugClearExposure()
        {
            EnsureInitializedForDebug();
            SetExposure(0f);
        }

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_SurvivalState CurrentState => survivalState;

        public bool IsAlive => survivalState.IsAlive;

        #endregion

        #region Private Methods

        private void EnsureInitializedForDebug()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        private void ResolveRuntimeHostReference()
        {
            if (CCS_Validation.IsObjectValid(runtimeHost))
            {
                return;
            }

            runtimeHost = GetComponent<CCS_RuntimeHost>();
            if (CCS_Validation.IsObjectValid(runtimeHost))
            {
                return;
            }

            runtimeHost = GetComponentInParent<CCS_RuntimeHost>();
        }

        private void TryRegisterSurvivalService()
        {
            ResolveRuntimeHostReference();

            if (!CCS_Validation.IsObjectValid(runtimeHost))
            {
                LogDebug("No CCS_RuntimeHost found. Survival service registration skipped.");
                return;
            }

            if (!runtimeHost.IsRuntimeInitialized)
            {
                CCS_Logger.LogWarning(LogCategory, "Runtime host is not initialized. Survival service registration skipped.");
                return;
            }

            bool registered = runtimeHost.ServiceRegistry.RegisterService<CCS_ISurvivalVitalsService>(this);
            isServiceRegistered = registered;
            if (registered)
            {
                LogDebug("Registered CCS_ISurvivalVitalsService on runtime host service registry.");
            }
            else
            {
                CCS_Logger.LogWarning(LogCategory, "Failed to register CCS_ISurvivalVitalsService.");
            }
        }

        private void TryUnregisterSurvivalService()
        {
            if (!isServiceRegistered || !CCS_Validation.IsObjectValid(runtimeHost))
            {
                return;
            }

            runtimeHost.ServiceRegistry.UnregisterService<CCS_ISurvivalVitalsService>();
            isServiceRegistered = false;
        }

        private void UpdateHungerAndThirst(float deltaTime)
        {
            float previousHunger = survivalState.Hunger;
            float previousThirst = survivalState.Thirst;

            survivalState.Hunger = Mathf.Max(0f, survivalState.Hunger - (hungerDrainRate * deltaTime));
            survivalState.Thirst = Mathf.Max(0f, survivalState.Thirst - (thirstDrainRate * deltaTime));

            PublishHungerChanged(previousHunger, survivalState.Hunger);
            PublishThirstChanged(previousThirst, survivalState.Thirst);
        }

        private void ApplyEnvironmentalDamage(float deltaTime)
        {
            if (survivalState.Hunger <= 0f)
            {
                ApplyDamage(starvationDamageRate * deltaTime);
            }

            if (survivalState.Thirst <= 0f)
            {
                ApplyDamage(dehydrationDamageRate * deltaTime);
            }

            if (survivalState.Exposure > 0f)
            {
                ApplyDamage(exposureDamageRate * survivalState.Exposure * deltaTime);
            }
        }

        private void RecoverStamina(float deltaTime)
        {
            float previousStamina = survivalState.Stamina;
            survivalState.Stamina = Mathf.Min(maxStamina, survivalState.Stamina + (staminaRecoveryRate * deltaTime));
            PublishStaminaChanged(previousStamina, survivalState.Stamina);
        }

        private void EvaluateDeathState()
        {
            if (!survivalState.IsAlive)
            {
                return;
            }

            if (survivalState.Health > 0f)
            {
                return;
            }

            survivalState.Health = 0f;
            survivalState.IsAlive = false;
            OnPlayerDied?.Invoke();
            PublishSurvivalStateChanged();
            LogDebug("Player entered dead state (health depleted).");
        }

        private void ClampStateToMaximums()
        {
            survivalState.Health = Mathf.Clamp(survivalState.Health, 0f, maxHealth);
            survivalState.Hunger = Mathf.Clamp(survivalState.Hunger, 0f, maxHunger);
            survivalState.Thirst = Mathf.Clamp(survivalState.Thirst, 0f, maxThirst);
            survivalState.Stamina = Mathf.Clamp(survivalState.Stamina, 0f, maxStamina);
        }

        private void PublishSurvivalStateChanged()
        {
            OnSurvivalStateChanged?.Invoke(survivalState);
        }

        private void PublishHealthChanged(float previousValue, float newValue)
        {
            if (Mathf.Approximately(previousValue, newValue))
            {
                return;
            }

            OnHealthChanged?.Invoke(newValue);
            PublishSurvivalStateChanged();
            LogDebug($"Health changed: {previousValue:F1} -> {newValue:F1}");
        }

        private void PublishHungerChanged(float previousValue, float newValue)
        {
            if (Mathf.Approximately(previousValue, newValue))
            {
                return;
            }

            OnHungerChanged?.Invoke(newValue);
            PublishSurvivalStateChanged();
        }

        private void PublishThirstChanged(float previousValue, float newValue)
        {
            if (Mathf.Approximately(previousValue, newValue))
            {
                return;
            }

            OnThirstChanged?.Invoke(newValue);
            PublishSurvivalStateChanged();
        }

        private void PublishStaminaChanged(float previousValue, float newValue)
        {
            if (Mathf.Approximately(previousValue, newValue))
            {
                return;
            }

            OnStaminaChanged?.Invoke(newValue);
            PublishSurvivalStateChanged();
        }

        private void PublishTemperatureChanged(float previousValue, float newValue)
        {
            if (Mathf.Approximately(previousValue, newValue))
            {
                return;
            }

            OnTemperatureChanged?.Invoke(newValue);
            PublishSurvivalStateChanged();
            LogDebug($"Body temperature changed: {previousValue:F1} -> {newValue:F1}");
        }

        private void LogDebug(string message)
        {
            CCS_Logger.Log(LogCategory, message, enableDebugLogs);
        }

        #endregion
    }
}
