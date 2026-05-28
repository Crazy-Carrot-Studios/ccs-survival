using System.Collections.Generic;
using CCS.Core;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalVitalsZoneReceiver
// CATEGORY: Survival / Environment / VitalsZones
// PURPOSE: Aggregates overlapping vitals modifier zones and applies pressure through CCS_ISurvivalVitalsService.
// PLACEMENT: Attach to CCS_PlayerRoot, traversal test agent, or future authority-owned avatars.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Separate from CCS_SurvivalHazardReceiver. No duplicate vitals state.
// =============================================================================

namespace CCS.Survival.Environment.VitalsZones
{
    [DisallowMultipleComponent]
    public sealed class CCS_SurvivalVitalsZoneReceiver : MonoBehaviour
    {
        public const string LogPrefix = "[CCS Survival Vitals Zone]";

        #region Variables

        private const float MaxExposureCap = 3f;

        [Header("Vitals")]
        [Tooltip("When enabled, modifier pressure is applied through CCS_ISurvivalVitalsService.")]
        [SerializeField] private bool applyToSurvivalVitals = true;

        [Tooltip("Optional runtime host for vitals service resolve. Falls back to one scene lookup when unset.")]
        [SerializeField] private CCS_RuntimeHost runtimeHost;

        [Header("Telemetry")]
        [Tooltip("Logs concise enter/exit messages for vitals modifier zones.")]
        [SerializeField] private bool enableVitalsZoneTelemetryLogging;

        private readonly HashSet<CCS_SurvivalVitalsModifierZone> activeModifierZones =
            new HashSet<CCS_SurvivalVitalsModifierZone>();

        private readonly List<CCS_SurvivalVitalsModifierType> summaryTypeBuffer = new List<CCS_SurvivalVitalsModifierType>(8);

        private CCS_ISurvivalVitalsService vitalsService;
        private bool loggedMissingVitalsService;

        #endregion

        #region Properties

        public int ActiveModifierZoneCount => activeModifierZones.Count;

        public bool AppliesToSurvivalVitals => applyToSurvivalVitals;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (applyToSurvivalVitals)
            {
                TryResolveVitalsService();
            }
        }

        private void Update()
        {
            if (!applyToSurvivalVitals || activeModifierZones.Count == 0)
            {
                return;
            }

            if (!TryResolveVitalsService() || vitalsService == null || !vitalsService.IsAlive)
            {
                return;
            }

            ApplyActiveModifierPressure(Time.deltaTime);
        }

        #endregion

        #region Public Methods

        public void RegisterModifierZone(CCS_SurvivalVitalsModifierZone modifierZone)
        {
            if (modifierZone == null || !activeModifierZones.Add(modifierZone))
            {
                return;
            }

            if (enableVitalsZoneTelemetryLogging)
            {
                Debug.Log($"{LogPrefix} Entered {modifierZone.TelemetryLabel}.");
            }
        }

        public void UnregisterModifierZone(CCS_SurvivalVitalsModifierZone modifierZone)
        {
            if (modifierZone == null || !activeModifierZones.Remove(modifierZone))
            {
                return;
            }

            if (enableVitalsZoneTelemetryLogging)
            {
                Debug.Log($"{LogPrefix} Exited {modifierZone.TelemetryLabel}.");
            }
        }

        public string GetActiveModifierSummary()
        {
            summaryTypeBuffer.Clear();

            foreach (CCS_SurvivalVitalsModifierZone modifierZone in activeModifierZones)
            {
                if (modifierZone == null || !modifierZone.IsZoneEnabled)
                {
                    continue;
                }

                CCS_SurvivalVitalsModifierType modifierType = modifierZone.ModifierType;
                if (!summaryTypeBuffer.Contains(modifierType))
                {
                    summaryTypeBuffer.Add(modifierType);
                }
            }

            if (summaryTypeBuffer.Count == 0)
            {
                return "None";
            }

            if (summaryTypeBuffer.Count == 1)
            {
                return FormatModifierTypeLabel(summaryTypeBuffer[0]);
            }

            if (summaryTypeBuffer.Count == 2)
            {
                return $"{FormatModifierTypeLabel(summaryTypeBuffer[0])} + {FormatModifierTypeLabel(summaryTypeBuffer[1])}";
            }

            return "Multiple";
        }

        #endregion

        #region Private Methods

        private bool TryResolveVitalsService()
        {
            if (vitalsService != null)
            {
                return true;
            }

            if (CCS_Validation.IsObjectValid(runtimeHost) && runtimeHost.IsRuntimeInitialized)
            {
                if (runtimeHost.ServiceRegistry.TryGetService(out CCS_ISurvivalVitalsService resolvedService))
                {
                    vitalsService = resolvedService;
                    loggedMissingVitalsService = false;
                    return true;
                }
            }

            vitalsService = FindFirstObjectByType<CCS_SurvivalModule>();
            if (vitalsService != null)
            {
                loggedMissingVitalsService = false;
                return true;
            }

            LogOnceMissingVitalsService();
            return false;
        }

        private void ApplyActiveModifierPressure(float deltaTime)
        {
            foreach (CCS_SurvivalVitalsModifierZone modifierZone in activeModifierZones)
            {
                if (modifierZone == null || !modifierZone.IsZoneEnabled)
                {
                    continue;
                }

                ApplyZoneModifier(modifierZone, deltaTime);
            }
        }

        private void ApplyZoneModifier(CCS_SurvivalVitalsModifierZone modifierZone, float deltaTime)
        {
            float amount = modifierZone.RatePerSecond * deltaTime;
            if (amount <= 0f)
            {
                return;
            }

            switch (modifierZone.ModifierType)
            {
                case CCS_SurvivalVitalsModifierType.HungerDrain:
                    vitalsService.DrainHunger(amount);
                    ClampHunger(modifierZone);
                    break;
                case CCS_SurvivalVitalsModifierType.HungerRestore:
                    vitalsService.RestoreHunger(amount);
                    ClampHunger(modifierZone);
                    break;
                case CCS_SurvivalVitalsModifierType.ThirstDrain:
                    vitalsService.DrainThirst(amount);
                    ClampThirst(modifierZone);
                    break;
                case CCS_SurvivalVitalsModifierType.ThirstRestore:
                    vitalsService.RestoreThirst(amount);
                    ClampThirst(modifierZone);
                    break;
                case CCS_SurvivalVitalsModifierType.StaminaDrain:
                    vitalsService.DrainStamina(amount);
                    ClampStamina(modifierZone);
                    break;
                case CCS_SurvivalVitalsModifierType.StaminaRestore:
                    vitalsService.RestoreStamina(amount);
                    ClampStamina(modifierZone);
                    break;
                case CCS_SurvivalVitalsModifierType.ExposureIncrease:
                    vitalsService.AddExposure(amount);
                    ClampExposure(modifierZone);
                    break;
                case CCS_SurvivalVitalsModifierType.ExposureRecovery:
                    vitalsService.ReduceExposure(amount);
                    ClampExposure(modifierZone);
                    break;
                case CCS_SurvivalVitalsModifierType.TemperatureIncrease:
                    vitalsService.ModifyBodyTemperature(amount);
                    ClampTemperature(modifierZone);
                    break;
                case CCS_SurvivalVitalsModifierType.TemperatureDecrease:
                    vitalsService.ModifyBodyTemperature(-amount);
                    ClampTemperature(modifierZone);
                    break;
                case CCS_SurvivalVitalsModifierType.HealthDrain:
                    vitalsService.ApplyDamage(amount);
                    ClampHealth(modifierZone);
                    break;
                case CCS_SurvivalVitalsModifierType.HealthRestore:
                    vitalsService.RestoreHealth(amount);
                    ClampHealth(modifierZone);
                    break;
            }
        }

        private void ClampHunger(CCS_SurvivalVitalsModifierZone modifierZone)
        {
            if (!TryGetClampBounds(modifierZone, out float minClamp, out float maxClamp))
            {
                return;
            }

            CCS_SurvivalState state = vitalsService.CurrentState;
            float clamped = Mathf.Clamp(state.Hunger, minClamp, maxClamp);
            if (!Mathf.Approximately(clamped, state.Hunger))
            {
                vitalsService.SetHunger(clamped);
            }
        }

        private void ClampThirst(CCS_SurvivalVitalsModifierZone modifierZone)
        {
            if (!TryGetClampBounds(modifierZone, out float minClamp, out float maxClamp))
            {
                return;
            }

            CCS_SurvivalState state = vitalsService.CurrentState;
            float clamped = Mathf.Clamp(state.Thirst, minClamp, maxClamp);
            if (!Mathf.Approximately(clamped, state.Thirst))
            {
                vitalsService.SetThirst(clamped);
            }
        }

        private void ClampStamina(CCS_SurvivalVitalsModifierZone modifierZone)
        {
            if (!TryGetClampBounds(modifierZone, out float minClamp, out float maxClamp))
            {
                return;
            }

            CCS_SurvivalState state = vitalsService.CurrentState;
            float clamped = Mathf.Clamp(state.Stamina, minClamp, maxClamp);
            if (!Mathf.Approximately(clamped, state.Stamina))
            {
                vitalsService.SetStamina(clamped);
            }
        }

        private void ClampExposure(CCS_SurvivalVitalsModifierZone modifierZone)
        {
            float exposure = vitalsService.CurrentState.Exposure;
            float clamped = Mathf.Min(MaxExposureCap, exposure);
            if (modifierZone.MinVitalClamp >= 0f)
            {
                clamped = Mathf.Max(modifierZone.MinVitalClamp, clamped);
            }

            if (modifierZone.MaxVitalClamp >= 0f)
            {
                clamped = Mathf.Min(modifierZone.MaxVitalClamp, clamped);
            }

            if (!Mathf.Approximately(clamped, exposure))
            {
                vitalsService.SetExposure(clamped);
            }
        }

        private void ClampTemperature(CCS_SurvivalVitalsModifierZone modifierZone)
        {
            if (!TryGetClampBounds(modifierZone, out float minClamp, out float maxClamp))
            {
                return;
            }

            float temperature = vitalsService.CurrentState.BodyTemperature;
            float clamped = Mathf.Clamp(temperature, minClamp, maxClamp);
            if (!Mathf.Approximately(clamped, temperature))
            {
                vitalsService.SetBodyTemperature(clamped);
            }
        }

        private void ClampHealth(CCS_SurvivalVitalsModifierZone modifierZone)
        {
            if (!TryGetClampBounds(modifierZone, out float minClamp, out float maxClamp))
            {
                return;
            }

            CCS_SurvivalState state = vitalsService.CurrentState;
            float clamped = Mathf.Clamp(state.Health, minClamp, maxClamp);
            if (!Mathf.Approximately(clamped, state.Health))
            {
                vitalsService.SetHealth(clamped);
            }
        }

        private static bool TryGetClampBounds(
            CCS_SurvivalVitalsModifierZone modifierZone,
            out float minClamp,
            out float maxClamp)
        {
            minClamp = modifierZone.MinVitalClamp;
            maxClamp = modifierZone.MaxVitalClamp;
            return minClamp >= 0f || maxClamp >= 0f;
        }

        private static string FormatModifierTypeLabel(CCS_SurvivalVitalsModifierType modifierType)
        {
            return modifierType switch
            {
                CCS_SurvivalVitalsModifierType.HungerDrain => "HungerDrain",
                CCS_SurvivalVitalsModifierType.HungerRestore => "HungerRestore",
                CCS_SurvivalVitalsModifierType.ThirstDrain => "ThirstDrain",
                CCS_SurvivalVitalsModifierType.ThirstRestore => "ThirstRestore",
                CCS_SurvivalVitalsModifierType.StaminaDrain => "StaminaDrain",
                CCS_SurvivalVitalsModifierType.StaminaRestore => "StaminaRestore",
                CCS_SurvivalVitalsModifierType.ExposureIncrease => "ExposureIncrease",
                CCS_SurvivalVitalsModifierType.ExposureRecovery => "ExposureRecovery",
                CCS_SurvivalVitalsModifierType.TemperatureIncrease => "TempUp",
                CCS_SurvivalVitalsModifierType.TemperatureDecrease => "TempDown",
                CCS_SurvivalVitalsModifierType.HealthDrain => "HealthDrain",
                CCS_SurvivalVitalsModifierType.HealthRestore => "HealthRestore",
                _ => modifierType.ToString()
            };
        }

        private void LogOnceMissingVitalsService()
        {
            if (loggedMissingVitalsService)
            {
                return;
            }

            loggedMissingVitalsService = true;
            Debug.LogWarning($"{LogPrefix} No CCS_ISurvivalVitalsService found for vitals zone receiver on '{name}'.");
        }

        #endregion
    }
}
