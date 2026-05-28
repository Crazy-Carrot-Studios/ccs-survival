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
// NOTES: Requires CCS_SurvivalVitalsProfile (Assets → Create → CCS → Survival → Survival Vitals Profile). See CCS_Survival_Phase_01_Survival_Core.md.
// =============================================================================

namespace CCS.Survival
{
    [DefaultExecutionOrder(50)]
    public sealed class CCS_SurvivalModule : MonoBehaviour, CCS_ISurvivalVitalsService, CCS_ISurvivalVitalsTestModeService
    {
        private const string LogCategory = CCS_SurvivalVitalsDiagnostics.LogCategory;

        #region Variables

        [Header("Survival Vitals Profile")]
        [Tooltip("Required tuning profile for this module. Create via Assets → Create → CCS → Survival → Survival Vitals Profile, then assign CCS_DefaultSurvivalVitalsProfile (Assets/CCS/Survival/Settings/Survival/) or your own asset.")]
        [SerializeField] private CCS_SurvivalVitalsProfile survivalVitalsProfile;

        [Header("Service Registration")]
        [Tooltip("Runtime host for service registry registration. Resolves on this GameObject, then parent, when unset.")]
        [SerializeField] private CCS_RuntimeHost runtimeHost;

        [Header("Debug")]
        [Tooltip("When enabled, survival state changes are written to the Unity console.")]
        [SerializeField] private bool enableDebugLogs;

        [Tooltip("Minimum whole-health step required before writing another health-changed debug log.")]
        [SerializeField] private float healthDebugLogStep = 5f;

        [Tooltip("Minimum body-temperature step (Celsius) required before writing another temperature-changed debug log.")]
        [SerializeField] private float temperatureDebugLogStep = 0.5f;

        [Tooltip("Minimum exposure step required before writing another exposure-changed debug log.")]
        [SerializeField] private float exposureDebugLogStep = 0.5f;

        [Header("Traversal Validation (Dev)")]
        [Tooltip("Optional vitals isolation while CCS_TraversalTestAgent validation is active.")]
        [SerializeField] private CCS_SurvivalVitalsTestIsolationSettings vitalsTestIsolation =
            new CCS_SurvivalVitalsTestIsolationSettings();

        private CCS_SurvivalState survivalState;
        private bool isInitialized;
        private bool isServiceRegistered;
        private bool isUsingFallbackProfile;

        private float maxHealth;
        private float maxHunger;
        private float maxThirst;
        private float maxStamina;
        private float defaultBodyTemperature;
        private float hungerDrainRate;
        private float thirstDrainRate;
        private float starvationDamageRate;
        private float dehydrationDamageRate;
        private float exposureDamageRate;
        private float staminaRecoveryRate;
        private float respawnHealthPercent;
        private float meaningfulChangePrecision;
        private float lastLoggedHealthValue = float.NaN;
        private float lastLoggedTemperatureValue = float.NaN;
        private float lastLoggedExposureValue = float.NaN;
        private bool isTraversalValidationActive;
        private bool isTestModeServiceRegistered;
        private bool isSubscribedToTraversalValidationEvents;

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
            ResolveVitalsTuning();
            Initialize();
            TryRegisterSurvivalServices();
            TrySubscribeToTraversalValidationEvents();
            PublishSurvivalStateChanged();
        }

        private void OnDestroy()
        {
            TryUnsubscribeFromTraversalValidationEvents();
            TryUnregisterSurvivalServices();
            isInitialized = false;
        }

        private void Update()
        {
            if (!survivalState.IsAlive)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            bool traversalIsolationActive = IsTraversalIsolationActive();

            if (!traversalIsolationActive
                || !vitalsTestIsolation.PauseGlobalVitalsTickDuringTraversalTest)
            {
                if (!traversalIsolationActive)
                {
                    UpdateHungerAndThirst(deltaTime);
                    ApplyEnvironmentalDamage(deltaTime);
                    RecoverStamina(deltaTime);
                }
                else
                {
                    UpdateHungerAndThirst(deltaTime);
                    if (!vitalsTestIsolation.DisableEnvironmentalDamageDuringTraversalTest)
                    {
                        ApplyEnvironmentalDamage(deltaTime);
                    }

                    RecoverStamina(deltaTime);
                }
            }

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
            survivalState.BodyTemperature = defaultBodyTemperature;
            ClampStateToMaximums();
            lastLoggedHealthValue = float.NaN;
            lastLoggedTemperatureValue = float.NaN;
            lastLoggedExposureValue = float.NaN;
            isInitialized = true;
            PublishTemperatureChanged(0f, survivalState.BodyTemperature);
            LogDebug(isUsingFallbackProfile
                ? "Survival vitals service initialized with fallback tuning values."
                : "Survival vitals service initialized from vitals profile.");
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
            RestoreThirst(hydrationValue);
        }

        public void DrainHunger(float amount)
        {
            if (!survivalState.IsAlive || amount <= 0f)
            {
                return;
            }

            float previousHunger = survivalState.Hunger;
            survivalState.Hunger = Mathf.Max(0f, survivalState.Hunger - amount);
            PublishHungerChanged(previousHunger, survivalState.Hunger);
            PublishSurvivalStateChanged();
        }

        public void RestoreHunger(float amount)
        {
            ConsumeFood(amount);
        }

        public void DrainThirst(float amount)
        {
            if (!survivalState.IsAlive || amount <= 0f)
            {
                return;
            }

            float previousThirst = survivalState.Thirst;
            survivalState.Thirst = Mathf.Max(0f, survivalState.Thirst - amount);
            PublishThirstChanged(previousThirst, survivalState.Thirst);
            PublishSurvivalStateChanged();
        }

        public void RestoreThirst(float amount)
        {
            if (!survivalState.IsAlive || amount <= 0f)
            {
                return;
            }

            float previousThirst = survivalState.Thirst;
            survivalState.Thirst = Mathf.Min(maxThirst, survivalState.Thirst + amount);
            PublishThirstChanged(previousThirst, survivalState.Thirst);
            PublishSurvivalStateChanged();
        }

        public void DrainStamina(float amount)
        {
            if (!survivalState.IsAlive || amount <= 0f)
            {
                return;
            }

            float previousStamina = survivalState.Stamina;
            survivalState.Stamina = Mathf.Max(0f, survivalState.Stamina - amount);
            PublishStaminaChanged(previousStamina, survivalState.Stamina);
            PublishSurvivalStateChanged();
        }

        public void AddExposure(float amount)
        {
            if (!survivalState.IsAlive || amount <= 0f)
            {
                return;
            }

            SetExposure(survivalState.Exposure + amount);
        }

        public void ReduceExposure(float amount)
        {
            if (!survivalState.IsAlive || amount <= 0f)
            {
                return;
            }

            SetExposure(Mathf.Max(0f, survivalState.Exposure - amount));
        }

        public void ModifyBodyTemperature(float delta)
        {
            if (!survivalState.IsAlive || Mathf.Approximately(delta, 0f))
            {
                return;
            }

            SetBodyTemperature(survivalState.BodyTemperature + delta);
        }

        public void SetHunger(float hungerValue)
        {
            if (!isInitialized)
            {
                return;
            }

            float previousHunger = survivalState.Hunger;
            survivalState.Hunger = Mathf.Clamp(hungerValue, 0f, maxHunger);
            PublishHungerChanged(previousHunger, survivalState.Hunger);
            PublishSurvivalStateChanged();
        }

        public void SetThirst(float thirstValue)
        {
            if (!isInitialized)
            {
                return;
            }

            float previousThirst = survivalState.Thirst;
            survivalState.Thirst = Mathf.Clamp(thirstValue, 0f, maxThirst);
            PublishThirstChanged(previousThirst, survivalState.Thirst);
            PublishSurvivalStateChanged();
        }

        public void SetStamina(float staminaValue)
        {
            if (!isInitialized)
            {
                return;
            }

            float previousStamina = survivalState.Stamina;
            survivalState.Stamina = Mathf.Clamp(staminaValue, 0f, maxStamina);
            PublishStaminaChanged(previousStamina, survivalState.Stamina);
            PublishSurvivalStateChanged();
        }

        public void SetHealth(float healthValue)
        {
            if (!isInitialized)
            {
                return;
            }

            float previousHealth = survivalState.Health;
            survivalState.Health = Mathf.Clamp(healthValue, 0f, maxHealth);
            PublishHealthChanged(previousHealth, survivalState.Health);
            EvaluateDeathState();
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
            lastLoggedHealthValue = float.NaN;
            PublishHealthChanged(maxHealth, survivalState.Health);
            OnPlayerDied?.Invoke();
            PublishSurvivalStateChanged();
            LogDebug("Player entered dead state (Kill).");
        }

        public void Respawn()
        {
            float previousTemperature = survivalState.BodyTemperature;
            survivalState = CCS_SurvivalState.CreateDefault();
            survivalState.BodyTemperature = defaultBodyTemperature;
            survivalState.Health = maxHealth * Mathf.Clamp01(respawnHealthPercent / 100f);
            lastLoggedHealthValue = float.NaN;
            lastLoggedTemperatureValue = float.NaN;
            lastLoggedExposureValue = float.NaN;
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
            if (!HasMeaningfulVitalChange(previousExposure, survivalState.Exposure))
            {
                return;
            }

            PublishSurvivalStateChanged();
            TryLogExposureChanged(previousExposure, survivalState.Exposure);
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

        public void NotifyTraversalValidationActive(bool isActive)
        {
            ApplyTraversalValidationState(isActive);
        }

        #endregion

        #region Properties

        public bool IsTraversalValidationActive => isTraversalValidationActive;

        public bool IsTraversalVitalsIsolationActive => IsTraversalIsolationActive();

        public bool IsInitialized => isInitialized;

        public CCS_SurvivalState CurrentState => survivalState;

        public float CurrentStamina => survivalState.Stamina;

        public bool IsAlive => survivalState.IsAlive;

        public bool HasStamina(float requiredAmount)
        {
            if (!isInitialized || !survivalState.IsAlive || requiredAmount <= 0f)
            {
                return requiredAmount <= 0f;
            }

            return survivalState.Stamina >= requiredAmount;
        }

        public bool TryConsumeStamina(float amount)
        {
            if (!isInitialized || !survivalState.IsAlive || amount <= 0f)
            {
                return amount <= 0f;
            }

            if (survivalState.Stamina < amount)
            {
                return false;
            }

            float previousStamina = survivalState.Stamina;
            survivalState.Stamina = Mathf.Max(0f, survivalState.Stamina - amount);
            PublishStaminaChanged(previousStamina, survivalState.Stamina);
            return true;
        }

        public void RestoreStamina(float amount)
        {
            if (!isInitialized || amount <= 0f)
            {
                return;
            }

            float previousStamina = survivalState.Stamina;
            survivalState.Stamina = Mathf.Min(maxStamina, survivalState.Stamina + amount);
            PublishStaminaChanged(previousStamina, survivalState.Stamina);
        }

        #endregion

        #region Private Methods

        private void EnsureInitializedForDebug()
        {
            if (!isInitialized)
            {
                ResolveVitalsTuning();
                Initialize();
            }
        }

        private void ResolveVitalsTuning()
        {
            if (CCS_Validation.IsObjectValid(survivalVitalsProfile))
            {
                survivalVitalsProfile.ValidateAndClamp();
                ApplyProfileValues(survivalVitalsProfile);
                isUsingFallbackProfile = false;
                return;
            }

            CCS_Logger.LogWarning(
                LogCategory,
                "CCS_SurvivalVitalsProfile is not assigned. Using safe fallback tuning values. " +
                "Create one via Assets → Create → CCS → Survival → Survival Vitals Profile and assign " +
                "CCS_DefaultSurvivalVitalsProfile on this component.");

            ApplyFallbackValues();
            isUsingFallbackProfile = true;
        }

        private void ApplyProfileValues(CCS_SurvivalVitalsProfile profile)
        {
            maxHealth = profile.MaxHealth;
            maxHunger = profile.MaxHunger;
            maxThirst = profile.MaxThirst;
            maxStamina = profile.MaxStamina;
            defaultBodyTemperature = profile.DefaultBodyTemperature;
            hungerDrainRate = profile.HungerDrainRate;
            thirstDrainRate = profile.ThirstDrainRate;
            starvationDamageRate = profile.StarvationDamageRate;
            dehydrationDamageRate = profile.DehydrationDamageRate;
            exposureDamageRate = profile.ExposureDamageRate;
            staminaRecoveryRate = profile.StaminaRecoveryRate;
            respawnHealthPercent = profile.RespawnHealthPercent;
            meaningfulChangePrecision = profile.MeaningfulChangePrecision;
        }

        private void ApplyFallbackValues()
        {
            maxHealth = 100f;
            maxHunger = 100f;
            maxThirst = 100f;
            maxStamina = 100f;
            defaultBodyTemperature = 37f;
            hungerDrainRate = 8f;
            thirstDrainRate = 10f;
            starvationDamageRate = 4f;
            dehydrationDamageRate = 5f;
            exposureDamageRate = 2f;
            staminaRecoveryRate = 8f;
            respawnHealthPercent = 50f;
            meaningfulChangePrecision = 0.1f;
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

        private void TryRegisterSurvivalServices()
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

            bool registeredVitals = runtimeHost.ServiceRegistry.RegisterService<CCS_ISurvivalVitalsService>(this);
            isServiceRegistered = registeredVitals;
            if (registeredVitals)
            {
                LogDebug("Registered CCS_ISurvivalVitalsService on runtime host service registry.");
            }
            else
            {
                CCS_Logger.LogWarning(LogCategory, "Failed to register CCS_ISurvivalVitalsService.");
            }

            bool registeredTestMode =
                runtimeHost.ServiceRegistry.RegisterService<CCS_ISurvivalVitalsTestModeService>(this);
            isTestModeServiceRegistered = registeredTestMode;
            if (registeredTestMode)
            {
                LogDebug("Registered CCS_ISurvivalVitalsTestModeService on runtime host service registry.");
            }
            else
            {
                CCS_Logger.LogWarning(LogCategory, "Failed to register CCS_ISurvivalVitalsTestModeService.");
            }
        }

        private void TryUnregisterSurvivalServices()
        {
            if (!CCS_Validation.IsObjectValid(runtimeHost))
            {
                return;
            }

            if (isTestModeServiceRegistered)
            {
                runtimeHost.ServiceRegistry.UnregisterService<CCS_ISurvivalVitalsTestModeService>();
                isTestModeServiceRegistered = false;
            }

            if (!isServiceRegistered)
            {
                return;
            }

            runtimeHost.ServiceRegistry.UnregisterService<CCS_ISurvivalVitalsService>();
            isServiceRegistered = false;
        }

        private void TrySubscribeToTraversalValidationEvents()
        {
            ResolveRuntimeHostReference();

            if (isSubscribedToTraversalValidationEvents
                || !CCS_Validation.IsObjectValid(runtimeHost)
                || !runtimeHost.IsRuntimeInitialized)
            {
                return;
            }

            runtimeHost.EventDispatcher.Subscribe<CCS_SurvivalTraversalValidationLifecycleEvent>(
                OnTraversalValidationLifecycleEvent);
            isSubscribedToTraversalValidationEvents = true;
        }

        private void TryUnsubscribeFromTraversalValidationEvents()
        {
            if (!isSubscribedToTraversalValidationEvents || !CCS_Validation.IsObjectValid(runtimeHost))
            {
                return;
            }

            runtimeHost.EventDispatcher.Unsubscribe<CCS_SurvivalTraversalValidationLifecycleEvent>(
                OnTraversalValidationLifecycleEvent);
            isSubscribedToTraversalValidationEvents = false;
        }

        private void OnTraversalValidationLifecycleEvent(CCS_SurvivalTraversalValidationLifecycleEvent lifecycleEvent)
        {
            ApplyTraversalValidationState(lifecycleEvent.IsActive);
        }

        private void ApplyTraversalValidationState(bool isActive)
        {
            if (isTraversalValidationActive == isActive)
            {
                return;
            }

            isTraversalValidationActive = isActive;

            if (!isActive || !vitalsTestIsolation.EnableTraversalValidationIsolation)
            {
                return;
            }

            if (vitalsTestIsolation.ResetVitalsOnTestStart)
            {
                ResetVitalsForTraversalValidation();
            }
        }

        private bool IsTraversalIsolationActive()
        {
            return isTraversalValidationActive && vitalsTestIsolation.EnableTraversalValidationIsolation;
        }

        private bool AreVitalsDebugLogsEnabled()
        {
            if (!enableDebugLogs)
            {
                return false;
            }

            if (IsTraversalIsolationActive() && vitalsTestIsolation.SuppressVitalsDebugLogsDuringTraversalTest)
            {
                return false;
            }

            return true;
        }

        private void ResetVitalsForTraversalValidation()
        {
            if (!isInitialized)
            {
                return;
            }

            survivalState = CCS_SurvivalState.CreateDefault();
            survivalState.BodyTemperature = defaultBodyTemperature;
            ClampStateToMaximums();
            lastLoggedHealthValue = float.NaN;
            lastLoggedTemperatureValue = float.NaN;
            lastLoggedExposureValue = float.NaN;
            PublishSurvivalStateChanged();
            LogDebug("Vitals reset for traversal validation (dev test isolation).");
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
            if (!HasMeaningfulVitalChange(previousValue, newValue))
            {
                return;
            }

            OnHealthChanged?.Invoke(newValue);
            PublishSurvivalStateChanged();
            TryLogHealthChanged(previousValue, newValue);
        }

        private void PublishHungerChanged(float previousValue, float newValue)
        {
            if (!HasMeaningfulVitalChange(previousValue, newValue))
            {
                return;
            }

            OnHungerChanged?.Invoke(newValue);
            PublishSurvivalStateChanged();
        }

        private void PublishThirstChanged(float previousValue, float newValue)
        {
            if (!HasMeaningfulVitalChange(previousValue, newValue))
            {
                return;
            }

            OnThirstChanged?.Invoke(newValue);
            PublishSurvivalStateChanged();
        }

        private void PublishStaminaChanged(float previousValue, float newValue)
        {
            if (!HasMeaningfulVitalChange(previousValue, newValue))
            {
                return;
            }

            OnStaminaChanged?.Invoke(newValue);
            PublishSurvivalStateChanged();
        }

        private void PublishTemperatureChanged(float previousValue, float newValue)
        {
            if (!HasMeaningfulVitalChange(previousValue, newValue))
            {
                return;
            }

            OnTemperatureChanged?.Invoke(newValue);
            PublishSurvivalStateChanged();
            TryLogTemperatureChanged(previousValue, newValue);
        }

        private float RoundVitalForDisplay(float value)
        {
            return Mathf.Round(value / meaningfulChangePrecision) * meaningfulChangePrecision;
        }

        private bool HasMeaningfulVitalChange(float previousValue, float newValue)
        {
            return !Mathf.Approximately(RoundVitalForDisplay(previousValue), RoundVitalForDisplay(newValue));
        }

        private void LogDebug(string message)
        {
            CCS_Logger.Log(LogCategory, message, AreVitalsDebugLogsEnabled());
        }

        private void TryLogHealthChanged(float previousValue, float newValue)
        {
            TryLogSteppedVitalChange(
                previousValue,
                newValue,
                ref lastLoggedHealthValue,
                Mathf.Max(1f, healthDebugLogStep),
                "Health changed");
        }

        private void TryLogTemperatureChanged(float previousValue, float newValue)
        {
            TryLogSteppedVitalChange(
                previousValue,
                newValue,
                ref lastLoggedTemperatureValue,
                Mathf.Max(0.1f, temperatureDebugLogStep),
                "Body temperature changed");
        }

        private void TryLogExposureChanged(float previousValue, float newValue)
        {
            TryLogSteppedVitalChange(
                previousValue,
                newValue,
                ref lastLoggedExposureValue,
                Mathf.Max(0.1f, exposureDebugLogStep),
                "Exposure changed");
        }

        private void TryLogSteppedVitalChange(
            float previousValue,
            float newValue,
            ref float lastLoggedValue,
            float debugLogStep,
            string logLabel)
        {
            if (!AreVitalsDebugLogsEnabled())
            {
                return;
            }

            float roundedNew = RoundVitalForDisplay(newValue);
            if (!float.IsNaN(lastLoggedValue) && Mathf.Abs(roundedNew - lastLoggedValue) < debugLogStep)
            {
                return;
            }

            lastLoggedValue = roundedNew;
            LogDebug(
                $"{logLabel}: {RoundVitalForDisplay(previousValue):F1} -> {RoundVitalForDisplay(newValue):F1}");
        }

        #endregion
    }
}
