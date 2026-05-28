using System.Collections.Generic;
using CCS.Core;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalHazardReceiver
// CATEGORY: Survival / Environment / Hazards
// PURPOSE: Applies aggregated environmental hazard pressure to survival vitals for an entity in trigger zones.
// PLACEMENT: Attach to CCS_PlayerRoot, traversal test agent, or future authority-owned avatars.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Resolves CCS_ISurvivalVitalsService once. Safe zones suppress hazard pressure.
// =============================================================================

namespace CCS.Survival.Environment.Hazards
{
    [DisallowMultipleComponent]
    public sealed class CCS_SurvivalHazardReceiver : MonoBehaviour
    {
        public const string LogPrefix = "[CCS Survival Hazard]";

        #region Variables

        [Header("Vitals")]
        [Tooltip("When enabled, hazard pressure is applied through CCS_ISurvivalVitalsService.")]
        [SerializeField] private bool applyToSurvivalVitals = true;

        [Tooltip("Optional runtime host for vitals service resolve. Falls back to one scene lookup when unset.")]
        [SerializeField] private CCS_RuntimeHost runtimeHost;

        [Header("Telemetry")]
        [Tooltip("Logs concise enter/exit messages for hazard and safe zones.")]
        [SerializeField] private bool enableHazardTelemetryLogging;

        private readonly HashSet<CCS_SurvivalHazardZone> activeHazardZones = new HashSet<CCS_SurvivalHazardZone>();
        private readonly HashSet<CCS_SurvivalSafeZone> activeSafeZones = new HashSet<CCS_SurvivalSafeZone>();
        private CCS_ISurvivalVitalsService vitalsService;
        private bool loggedMissingVitalsService;

        #endregion

        #region Properties

        public bool IsInSafeZone => activeSafeZones.Count > 0;

        public int ActiveHazardZoneCount => activeHazardZones.Count;

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
            if (!applyToSurvivalVitals || activeHazardZones.Count == 0)
            {
                return;
            }

            if (!TryResolveVitalsService() || vitalsService == null || !vitalsService.IsAlive)
            {
                return;
            }

            if (IsInSafeZone)
            {
                ApplySafeZoneRecovery(Time.deltaTime);
                return;
            }

            ApplyActiveHazardPressure(Time.deltaTime);
        }

        #endregion

        #region Public Methods

        public void RegisterHazardZone(CCS_SurvivalHazardZone hazardZone)
        {
            if (hazardZone == null || !activeHazardZones.Add(hazardZone))
            {
                return;
            }

            if (enableHazardTelemetryLogging)
            {
                Debug.Log($"{LogPrefix} Entered {hazardZone.TelemetryLabel}.");
            }
        }

        public void UnregisterHazardZone(CCS_SurvivalHazardZone hazardZone)
        {
            if (hazardZone == null || !activeHazardZones.Remove(hazardZone))
            {
                return;
            }

            if (enableHazardTelemetryLogging)
            {
                Debug.Log($"{LogPrefix} Exited {hazardZone.TelemetryLabel}.");
            }
        }

        public void RegisterSafeZone(CCS_SurvivalSafeZone safeZone)
        {
            if (safeZone == null || !activeSafeZones.Add(safeZone))
            {
                return;
            }

            if (enableHazardTelemetryLogging)
            {
                Debug.Log($"{LogPrefix} Entered {safeZone.TelemetryLabel}.");
            }
        }

        public void UnregisterSafeZone(CCS_SurvivalSafeZone safeZone)
        {
            if (safeZone == null || !activeSafeZones.Remove(safeZone))
            {
                return;
            }

            if (enableHazardTelemetryLogging)
            {
                Debug.Log($"{LogPrefix} Exited {safeZone.TelemetryLabel}.");
            }
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

        private void ApplyActiveHazardPressure(float deltaTime)
        {
            float healthDamage = 0f;
            float exposure = 0f;
            float staminaDrain = 0f;
            float temperatureChange = 0f;

            foreach (CCS_SurvivalHazardZone hazardZone in activeHazardZones)
            {
                if (hazardZone == null || !hazardZone.IsZoneEnabled)
                {
                    continue;
                }

                healthDamage += hazardZone.HealthDamagePerSecond;
                exposure += hazardZone.ExposurePerSecond;
                staminaDrain += hazardZone.StaminaDrainPerSecond;
                temperatureChange += hazardZone.TemperatureChangePerSecond;
            }

            if (healthDamage > 0f)
            {
                vitalsService.ApplyDamage(healthDamage * deltaTime);
            }

            if (exposure > 0f)
            {
                float nextExposure = vitalsService.CurrentState.Exposure + (exposure * deltaTime);
                vitalsService.SetExposure(nextExposure);
            }

            if (staminaDrain > 0f)
            {
                vitalsService.TryConsumeStamina(staminaDrain * deltaTime);
            }

            if (!Mathf.Approximately(temperatureChange, 0f))
            {
                float nextTemperature = vitalsService.CurrentState.BodyTemperature + (temperatureChange * deltaTime);
                vitalsService.SetBodyTemperature(nextTemperature);
            }
        }

        private void ApplySafeZoneRecovery(float deltaTime)
        {
            float healthRecovery = 0f;
            float exposureReduction = 0f;
            float staminaRecovery = 0f;
            bool clearExposure = false;

            foreach (CCS_SurvivalSafeZone safeZone in activeSafeZones)
            {
                if (safeZone == null || !safeZone.IsZoneEnabled)
                {
                    continue;
                }

                healthRecovery += safeZone.HealthRecoveryPerSecond;
                exposureReduction += safeZone.ExposureReductionPerSecond;
                staminaRecovery += safeZone.StaminaRecoveryPerSecond;
                if (safeZone.ClearExposureWhileInside)
                {
                    clearExposure = true;
                }
            }

            if (healthRecovery > 0f)
            {
                vitalsService.RestoreHealth(healthRecovery * deltaTime);
            }

            if (staminaRecovery > 0f)
            {
                vitalsService.RestoreStamina(staminaRecovery * deltaTime);
            }

            if (clearExposure)
            {
                vitalsService.SetExposure(0f);
            }
            else if (exposureReduction > 0f)
            {
                float nextExposure = Mathf.Max(
                    0f,
                    vitalsService.CurrentState.Exposure - (exposureReduction * deltaTime));
                vitalsService.SetExposure(nextExposure);
            }
        }

        private void LogOnceMissingVitalsService()
        {
            if (loggedMissingVitalsService)
            {
                return;
            }

            loggedMissingVitalsService = true;
            Debug.LogWarning($"{LogPrefix} No CCS_ISurvivalVitalsService found for hazard receiver on '{name}'.");
        }

        #endregion
    }
}
