using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalStatDecayDefinition
// CATEGORY: Survival / Runtime / SurvivalCore / Profiles
// PURPOSE: Per-second change rates for survival stat decay, recovery, and exposure drift.
// PLACEMENT: Listed on CCS_SurvivalCoreProfile. Applied in CCS_SurvivalCoreService.TickSurvival.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Placeholder rates for 0.3.7 — not tuned for final gameplay pressure.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    [Serializable]
    public sealed class CCS_SurvivalStatDecayDefinition
    {
        #region Variables

        [Tooltip("Stat channel affected by this decay/recovery rule.")]
        [SerializeField] private CCS_SurvivalStatType statType = CCS_SurvivalStatType.Hunger;

        [Tooltip("When true, changePerSecond subtracts from current (drain). When false, adds (gain).")]
        [SerializeField] private bool subtractPerSecond = true;

        [Tooltip("Magnitude of change per second (non-negative).")]
        [SerializeField] private float changePerSecond;

        [Tooltip("Optional temperature comfort target for exposure drift (Temperature stat only).")]
        [SerializeField] private float temperatureComfortTarget = 50f;

        [Tooltip("When true and stat is Temperature, drift toward temperatureComfortTarget instead of linear subtract/add.")]
        [SerializeField] private bool useTemperatureComfortDrift;

        #endregion

        #region Properties

        public CCS_SurvivalStatType StatType => statType;

        public bool SubtractPerSecond => subtractPerSecond;

        public float ChangePerSecond => changePerSecond;

        public float TemperatureComfortTarget => temperatureComfortTarget;

        public bool UseTemperatureComfortDrift => useTemperatureComfortDrift;

        #endregion
    }
}
