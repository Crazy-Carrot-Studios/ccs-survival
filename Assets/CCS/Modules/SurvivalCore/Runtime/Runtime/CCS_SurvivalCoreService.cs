using System;
using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.EnvironmentEffects;
using CCS.Survival;
using CCS.Survival.Development;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreService
// CATEGORY: Survival / Runtime / SurvivalCore / Runtime
// PURPOSE: Runtime owner of survival core stat states, decay tick, snapshots, and events.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Environment Effects integration at 0.7.3. No Health damage or death systems.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public sealed class CCS_SurvivalCoreService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_SurvivalCoreService]";

        #region Variables

        private readonly Dictionary<CCS_SurvivalStatType, CCS_SurvivalStatState> statStates =
            new Dictionary<CCS_SurvivalStatType, CCS_SurvivalStatState>(8);

        private readonly HashSet<CCS_SurvivalStatType> depletedStats = new HashSet<CCS_SurvivalStatType>();

        private readonly CCS_SurvivalDiagnosticsService diagnosticsService;
        private CCS_SurvivalCoreProfile activeProfile;
        private CCS_EnvironmentEffectsService environmentEffectsService;
        private CCS_SurvivalEnvironmentInfluence currentEnvironmentInfluence = CCS_SurvivalEnvironmentInfluence.Empty;
        private bool isInitialized;
        private bool staminaDrainActive;

        #endregion

        #region Events

        public event SurvivalStatChangedHandler StatChanged;

        public event SurvivalStatDepletedHandler StatDepleted;

        public event SurvivalStatRestoredHandler StatRestored;

        public event SurvivalCoreInitializedHandler SurvivalCoreInitialized;

        public event SurvivalEnvironmentInfluenceChangedHandler EnvironmentInfluenceChanged;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_SurvivalCoreProfile ActiveProfile => activeProfile;

        public CCS_SurvivalEnvironmentInfluence CurrentEnvironmentInfluence => currentEnvironmentInfluence;

        public bool StaminaDrainActive
        {
            get => staminaDrainActive;
            set => staminaDrainActive = value;
        }

        #endregion

        #region Public Methods

        public CCS_SurvivalCoreService(CCS_SurvivalDiagnosticsService diagnosticsService = null)
        {
            this.diagnosticsService = diagnosticsService;
        }

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            LogDiagnostic(CCS_SurvivalDiagnosticsSeverity.Info, "Survival core service marked initialized.");
        }

        public CCS_Result InitializeFromProfile(CCS_SurvivalCoreProfile profile)
        {
            if (profile == null)
            {
                return CCS_Result.Failure(CCS_CoreErrorCode.ValidationFailed, "Survival core profile is null.");
            }

            CCS_SurvivalValidationResult validation = CCS_SurvivalCoreValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                return validation.ToCoreResult();
            }

            statStates.Clear();
            depletedStats.Clear();
            activeProfile = profile;

            for (int index = 0; index < profile.StatDefinitions.Count; index++)
            {
                CCS_SurvivalStatDefinition definition = profile.StatDefinitions[index];
                statStates[definition.StatType] = new CCS_SurvivalStatState(
                    definition.StatType,
                    definition.MinValue,
                    definition.MaxValue,
                    definition.StartingValue);
            }

            isInitialized = true;
            RefreshEnvironmentInfluence(true);
            SurvivalCoreInitialized?.Invoke(activeProfile);
            LogDiagnostic(
                CCS_SurvivalDiagnosticsSeverity.Info,
                $"Survival core initialized from profile: {profile.ProfileDisplayName}");

            return CCS_Result.Success();
        }

        public void BindEnvironmentEffectsService(CCS_EnvironmentEffectsService service)
        {
            UnbindEnvironmentEffectsService();
            environmentEffectsService = service;

            if (environmentEffectsService == null || !environmentEffectsService.IsInitialized)
            {
                return;
            }

            environmentEffectsService.EnvironmentChanged += HandleEnvironmentEffectsChanged;
            environmentEffectsService.TemperatureChanged += HandleEnvironmentEffectsChanged;
            environmentEffectsService.WetnessChanged += HandleEnvironmentEffectsChanged;
            environmentEffectsService.ExposureChanged += HandleEnvironmentEffectsChanged;
            RefreshEnvironmentInfluence(true);
        }

        public void Tick(float deltaTime)
        {
            TickSurvival(deltaTime);
        }

        public void TickSurvival(float deltaTime)
        {
            if (!isInitialized || activeProfile == null || deltaTime <= 0f)
            {
                return;
            }

            ApplyDecayDefinitions(deltaTime);
            ApplyProfileHungerDrain(deltaTime);
            RefreshEnvironmentInfluence(true);
            ApplyEnvironmentInfluence(deltaTime);
            ApplyHealthPlaceholders(deltaTime);
            ApplyStaminaPlaceholders(deltaTime);
        }

        public bool TryGetSnapshot(CCS_SurvivalStatType statType, out CCS_SurvivalStatSnapshot snapshot)
        {
            snapshot = default;

            if (!statStates.TryGetValue(statType, out CCS_SurvivalStatState state))
            {
                return false;
            }

            snapshot = state.ToSnapshot();
            return true;
        }

        public IReadOnlyList<CCS_SurvivalStatSnapshot> GetAllSnapshots()
        {
            List<CCS_SurvivalStatSnapshot> snapshots = new List<CCS_SurvivalStatSnapshot>(statStates.Count);

            foreach (KeyValuePair<CCS_SurvivalStatType, CCS_SurvivalStatState> entry in statStates)
            {
                snapshots.Add(entry.Value.ToSnapshot());
            }

            return snapshots;
        }

        public CCS_Result TryApplyModifier(CCS_SurvivalStatType statType, CCS_SurvivalStatModifier modifier)
        {
            if (!statStates.TryGetValue(statType, out CCS_SurvivalStatState state))
            {
                return CCS_Result.Failure(
                    CCS_CoreErrorCode.ValidationFailed,
                    $"Stat not found: {statType}");
            }

            CCS_SurvivalStatSnapshot previous = state.ToSnapshot();
            state.ApplyModifier(modifier);
            NotifyStatChanged(previous, state);
            return CCS_Result.Success();
        }

        #endregion

        #region Private Methods

        private void HandleEnvironmentEffectsChanged(CCS_EnvironmentEffectsEventArgs eventArgs)
        {
            RefreshEnvironmentInfluence(true);
        }

        private void RefreshEnvironmentInfluence(bool raiseEvents)
        {
            if (activeProfile == null)
            {
                return;
            }

            if (environmentEffectsService == null || !environmentEffectsService.IsInitialized)
            {
                if (raiseEvents
                    && CCS_SurvivalEnvironmentInfluenceUtility.HasMeaningfulChange(
                        currentEnvironmentInfluence,
                        CCS_SurvivalEnvironmentInfluence.Empty))
                {
                    currentEnvironmentInfluence = CCS_SurvivalEnvironmentInfluence.Empty;
                    EnvironmentInfluenceChanged?.Invoke(
                        new CCS_SurvivalEnvironmentEventArgs(currentEnvironmentInfluence));
                }
                else
                {
                    currentEnvironmentInfluence = CCS_SurvivalEnvironmentInfluence.Empty;
                }

                return;
            }

            CCS_EnvironmentSnapshot environmentSnapshot = environmentEffectsService.GetSnapshot();
            CCS_SurvivalEnvironmentInfluence nextInfluence =
                CCS_SurvivalEnvironmentInfluenceUtility.Calculate(environmentSnapshot, activeProfile);

            if (raiseEvents
                && CCS_SurvivalEnvironmentInfluenceUtility.HasMeaningfulChange(
                    currentEnvironmentInfluence,
                    nextInfluence))
            {
                currentEnvironmentInfluence = nextInfluence;
                EnvironmentInfluenceChanged?.Invoke(
                    new CCS_SurvivalEnvironmentEventArgs(currentEnvironmentInfluence));
                return;
            }

            currentEnvironmentInfluence = nextInfluence;
        }

        private void ApplyEnvironmentInfluence(float deltaTime)
        {
            if (environmentEffectsService == null || !environmentEffectsService.IsInitialized)
            {
                return;
            }

            CCS_SurvivalEnvironmentInfluence influence = currentEnvironmentInfluence;
            ApplyTemperatureInfluence(influence.CalculatedTemperatureDelta, deltaTime);
            ApplyFatigueInfluence(influence.CalculatedFatigueDelta, deltaTime);
            ApplyThirstInfluence(influence.CalculatedThirstDelta, deltaTime);
        }

        private void ApplyTemperatureInfluence(float deltaPerSecond, float deltaTime)
        {
            if (!statStates.TryGetValue(CCS_SurvivalStatType.Temperature, out CCS_SurvivalStatState state))
            {
                return;
            }

            float delta = deltaPerSecond * deltaTime;
            if (Math.Abs(delta) <= CCS_SurvivalStatUtility.DepletionEpsilon)
            {
                return;
            }

            CCS_SurvivalStatSnapshot previous = state.ToSnapshot();
            float clampedValue = Mathf.Clamp(
                state.CurrentValue + delta,
                activeProfile.MinimumTemperatureClamp,
                activeProfile.MaximumTemperatureClamp);
            state.SetCurrent(clampedValue);
            NotifyStatChanged(previous, state);
        }

        private void ApplyFatigueInfluence(float deltaPerSecond, float deltaTime)
        {
            if (!statStates.TryGetValue(CCS_SurvivalStatType.Fatigue, out CCS_SurvivalStatState state))
            {
                return;
            }

            float delta = deltaPerSecond * deltaTime;
            if (Math.Abs(delta) <= CCS_SurvivalStatUtility.DepletionEpsilon)
            {
                return;
            }

            CCS_SurvivalStatSnapshot previous = state.ToSnapshot();
            state.ApplyDelta(delta);
            NotifyStatChanged(previous, state);
        }

        private void ApplyThirstInfluence(float deltaPerSecond, float deltaTime)
        {
            if (!statStates.TryGetValue(CCS_SurvivalStatType.Thirst, out CCS_SurvivalStatState state))
            {
                return;
            }

            float delta = deltaPerSecond * deltaTime;
            if (Math.Abs(delta) <= CCS_SurvivalStatUtility.DepletionEpsilon)
            {
                return;
            }

            CCS_SurvivalStatSnapshot previous = state.ToSnapshot();
            state.ApplyDelta(delta);
            NotifyStatChanged(previous, state);
        }

        private void ApplyProfileHungerDrain(float deltaTime)
        {
            if (activeProfile == null
                || activeProfile.HungerDrainPerSecond <= 0f
                || !statStates.TryGetValue(CCS_SurvivalStatType.Hunger, out CCS_SurvivalStatState state))
            {
                return;
            }

            float delta = activeProfile.HungerDrainPerSecond * deltaTime;
            if (delta <= CCS_SurvivalStatUtility.DepletionEpsilon)
            {
                return;
            }

            CCS_SurvivalStatSnapshot previous = state.ToSnapshot();
            state.ApplyDelta(-delta);
            NotifyStatChanged(previous, state);
        }

        private void ApplyDecayDefinitions(float deltaTime)
        {
            if (activeProfile.DecayDefinitions == null)
            {
                return;
            }

            for (int index = 0; index < activeProfile.DecayDefinitions.Count; index++)
            {
                CCS_SurvivalStatDecayDefinition decayDefinition = activeProfile.DecayDefinitions[index];
                if (decayDefinition.StatType == CCS_SurvivalStatType.Hunger)
                {
                    continue;
                }

                if (!statStates.TryGetValue(decayDefinition.StatType, out CCS_SurvivalStatState state))
                {
                    continue;
                }

                CCS_SurvivalStatSnapshot previous = state.ToSnapshot();
                float delta = decayDefinition.ChangePerSecond * deltaTime;

                if (decayDefinition.UseTemperatureComfortDrift
                    && decayDefinition.StatType == CCS_SurvivalStatType.Temperature)
                {
                    float target = decayDefinition.TemperatureComfortTarget;
                    float drift = delta * Math.Sign(target - state.CurrentValue);
                    state.ApplyDelta(drift);
                }
                else if (decayDefinition.SubtractPerSecond)
                {
                    state.ApplyDelta(-delta);
                }
                else
                {
                    state.ApplyDelta(delta);
                }

                NotifyStatChanged(previous, state);
            }
        }

        private void ApplyHealthPlaceholders(float deltaTime)
        {
            if (!statStates.TryGetValue(CCS_SurvivalStatType.Health, out CCS_SurvivalStatState state))
            {
                return;
            }

            float netChange = 0f;
            if (activeProfile.PassiveHealthHealPerSecond > 0f)
            {
                netChange += activeProfile.PassiveHealthHealPerSecond * deltaTime;
            }

            if (activeProfile.PassiveHealthDamagePerSecond > 0f)
            {
                netChange -= activeProfile.PassiveHealthDamagePerSecond * deltaTime;
            }

            if (Math.Abs(netChange) <= CCS_SurvivalStatUtility.DepletionEpsilon)
            {
                return;
            }

            CCS_SurvivalStatSnapshot previous = state.ToSnapshot();
            state.ApplyDelta(netChange);
            NotifyStatChanged(previous, state);
        }

        private void ApplyStaminaPlaceholders(float deltaTime)
        {
            if (!statStates.TryGetValue(CCS_SurvivalStatType.Stamina, out CCS_SurvivalStatState state))
            {
                return;
            }

            float changePerSecond = staminaDrainActive
                ? -activeProfile.StaminaDrainPerSecond
                : activeProfile.StaminaRecoveryPerSecond;

            if (Math.Abs(changePerSecond) <= CCS_SurvivalStatUtility.DepletionEpsilon)
            {
                return;
            }

            CCS_SurvivalStatSnapshot previous = state.ToSnapshot();
            state.ApplyDelta(changePerSecond * deltaTime);
            NotifyStatChanged(previous, state);
        }

        private void NotifyStatChanged(CCS_SurvivalStatSnapshot previous, CCS_SurvivalStatState state)
        {
            CCS_SurvivalStatSnapshot current = state.ToSnapshot();

            if (Math.Abs(previous.CurrentValue - current.CurrentValue) <= CCS_SurvivalStatUtility.DepletionEpsilon)
            {
                return;
            }

            CCS_SurvivalStatChangedEventArgs eventArgs =
                new CCS_SurvivalStatChangedEventArgs(previous, current);

            StatChanged?.Invoke(eventArgs);
            LogDiagnostic(
                CCS_SurvivalDiagnosticsSeverity.Info,
                $"{current.StatType} changed {previous.CurrentValue:F2} -> {current.CurrentValue:F2}");

            bool wasDepleted = depletedStats.Contains(current.StatType);
            bool isDepleted = state.IsAtOrBelowMin();

            if (!wasDepleted && isDepleted)
            {
                depletedStats.Add(current.StatType);
                StatDepleted?.Invoke(current.StatType, current);
                LogDiagnostic(
                    CCS_SurvivalDiagnosticsSeverity.Warning,
                    $"{current.StatType} depleted at {current.CurrentValue:F2}");
            }
            else if (wasDepleted && !isDepleted)
            {
                depletedStats.Remove(current.StatType);
                StatRestored?.Invoke(current.StatType, current);
                LogDiagnostic(
                    CCS_SurvivalDiagnosticsSeverity.Info,
                    $"{current.StatType} restored to {current.CurrentValue:F2}");
            }
        }

        private void UnbindEnvironmentEffectsService()
        {
            if (environmentEffectsService == null)
            {
                return;
            }

            environmentEffectsService.EnvironmentChanged -= HandleEnvironmentEffectsChanged;
            environmentEffectsService.TemperatureChanged -= HandleEnvironmentEffectsChanged;
            environmentEffectsService.WetnessChanged -= HandleEnvironmentEffectsChanged;
            environmentEffectsService.ExposureChanged -= HandleEnvironmentEffectsChanged;
            environmentEffectsService = null;
        }

        private void LogDiagnostic(CCS_SurvivalDiagnosticsSeverity severity, string message)
        {
            if (diagnosticsService == null || !diagnosticsService.IsInitialized)
            {
                return;
            }

            diagnosticsService.ReportMessage(
                CCS_SurvivalRuntimeConstants.SurvivalCoreModuleId,
                message,
                severity);
        }

        #endregion
    }
}
