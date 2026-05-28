using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalVitalsTestIsolationSettings
// CATEGORY: Survival / Runtime / Survival / Debug
// PURPOSE: Dev-only toggles that isolate global vitals ticking during traversal validation.
// PLACEMENT: Serialized on CCS_SurvivalModule. Normal gameplay unchanged when master toggle is off.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Applied only while traversal validation is active and enableTraversalValidationIsolation is on.
// =============================================================================

namespace CCS.Survival
{
    [Serializable]
    public sealed class CCS_SurvivalVitalsTestIsolationSettings
    {
        [Tooltip("Master switch. When off, traversal validation does not change global vitals ticking.")]
        [SerializeField] private bool enableTraversalValidationIsolation;

        [Tooltip("Pauses hunger/thirst drain, stamina recovery, and environmental damage while traversal validation is active.")]
        [SerializeField] private bool pauseGlobalVitalsTickDuringTraversalTest = true;

        [Tooltip("When global tick is not fully paused, skips starvation/dehydration/exposure damage only.")]
        [SerializeField] private bool disableEnvironmentalDamageDuringTraversalTest = true;

        [Tooltip("Suppresses CCS_SurvivalModule vitals debug logs while traversal validation is active.")]
        [SerializeField] private bool suppressVitalsDebugLogsDuringTraversalTest = true;

        [Tooltip("Resets vitals to profile defaults when traversal validation starts.")]
        [SerializeField] private bool resetVitalsOnTestStart = true;

        public bool EnableTraversalValidationIsolation => enableTraversalValidationIsolation;

        public bool PauseGlobalVitalsTickDuringTraversalTest => pauseGlobalVitalsTickDuringTraversalTest;

        public bool DisableEnvironmentalDamageDuringTraversalTest =>
            disableEnvironmentalDamageDuringTraversalTest;

        public bool SuppressVitalsDebugLogsDuringTraversalTest => suppressVitalsDebugLogsDuringTraversalTest;

        public bool ResetVitalsOnTestStart => resetVitalsOnTestStart;
    }
}
