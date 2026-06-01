using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlaytestProfile
// CATEGORY: Modules / Playtesting / Runtime / Profiles
// PURPOSE: Bootstrap-only manual playtest harness tuning and default checklist.
// PLACEMENT: Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Intended for development bootstrap scenes only.
// =============================================================================

namespace CCS.Modules.Playtesting
{
    [CreateAssetMenu(
        fileName = "CCS_PlaytestProfile",
        menuName = "CCS/Survival/Playtesting/Playtest Profile")]
    public sealed class CCS_PlaytestProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Harness")]
        [Tooltip("When false, playtest service and HUD remain inactive.")]
        [SerializeField] private bool enableHarness = true;

        [Tooltip("Emit categorized playtest debug logs.")]
        [SerializeField] private bool showDebugLogs = true;

        [Tooltip("When true, step states reset whenever play mode starts.")]
        [SerializeField] private bool resetStepStateOnPlayStart = true;

        [Header("Checklist")]
        [Tooltip("Ordered manual playtest steps for the bootstrap survival loop.")]
        [SerializeField] private List<CCS_PlaytestStepDefinition> stepDefinitions = new List<CCS_PlaytestStepDefinition>();

        #endregion

        #region Properties

        public bool EnableHarness => enableHarness;

        public bool ShowDebugLogs => showDebugLogs;

        public bool ResetStepStateOnPlayStart => resetStepStateOnPlayStart;

        public IReadOnlyList<CCS_PlaytestStepDefinition> StepDefinitions => stepDefinitions;

        #endregion
    }
}
