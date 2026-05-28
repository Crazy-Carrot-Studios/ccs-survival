using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalHazardProfile
// CATEGORY: Survival / Environment / Hazards
// PURPOSE: Reusable tuning preset for environmental hazard zones.
// PLACEMENT: Assets/CCS/Survival/Settings/Hazards/ — assign on CCS_SurvivalHazardZone when desired.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Zones may override profile values in the Inspector.
// =============================================================================

namespace CCS.Survival.Environment.Hazards
{
    [CreateAssetMenu(fileName = "CCS_SurvivalHazardProfile", menuName = "CCS/Survival/Hazard Profile")]
    public sealed class CCS_SurvivalHazardProfile : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Hazard category used for gizmo color and telemetry labels.")]
        [SerializeField] private CCS_SurvivalHazardType hazardType = CCS_SurvivalHazardType.GenericDamage;

        [Header("Pressure Rates")]
        [Tooltip("Health damage applied per second while inside the zone.")]
        [SerializeField] private float healthDamagePerSecond;

        [Tooltip("Exposure added per second while inside the zone (feeds existing exposure damage on vitals module).")]
        [SerializeField] private float exposurePerSecond;

        [Tooltip("Stamina drained per second while inside the zone.")]
        [SerializeField] private float staminaDrainPerSecond;

        [Tooltip("Body temperature change per second (negative cools, positive heats).")]
        [SerializeField] private float temperatureChangePerSecond;

        #endregion

        #region Properties

        public CCS_SurvivalHazardType HazardType => hazardType;

        public float HealthDamagePerSecond => healthDamagePerSecond;

        public float ExposurePerSecond => exposurePerSecond;

        public float StaminaDrainPerSecond => staminaDrainPerSecond;

        public float TemperatureChangePerSecond => temperatureChangePerSecond;

        #endregion
    }
}
