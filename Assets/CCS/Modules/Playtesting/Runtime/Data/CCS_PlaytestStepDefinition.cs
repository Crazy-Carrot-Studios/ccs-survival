using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlaytestStepDefinition
// CATEGORY: Modules / Playtesting / Runtime / Data
// PURPOSE: Serializable manual playtest checklist step configuration.
// PLACEMENT: Stored on CCS_PlaytestProfile step list.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Optional target ids support event-driven auto-pass rules.
// =============================================================================

namespace CCS.Modules.Playtesting
{
    [Serializable]
    public sealed class CCS_PlaytestStepDefinition
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS step id for logging and event payloads.")]
        [SerializeField] private string stepId = string.Empty;

        [Tooltip("Short checklist label shown in the playtest HUD.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Step archetype used for auto-completion hooks.")]
        [SerializeField] private CCS_PlaytestStepType stepType = CCS_PlaytestStepType.Spawn;

        [Header("Instructions")]
        [Tooltip("Player-facing dev instruction text for the active step.")]
        [SerializeField] private string instructionText = string.Empty;

        [Header("Optional Targets")]
        [Tooltip("Optional item definition id filter for gather/eat/equip steps.")]
        [SerializeField] private string targetItemId = string.Empty;

        [Tooltip("Optional world object id filter for future scene-specific steps.")]
        [SerializeField] private string targetObjectId = string.Empty;

        [Tooltip("Optional required count for gather or harvest style steps.")]
        [SerializeField] private int requiredCount = 1;

        [Tooltip("Optional timeout in seconds before the step may be marked failed. Zero disables timeout.")]
        [SerializeField] private float timeoutSeconds;

        #endregion

        #region Properties

        public string StepId => stepId;

        public string DisplayName => displayName;

        public CCS_PlaytestStepType StepType => stepType;

        public string InstructionText => instructionText;

        public string TargetItemId => targetItemId;

        public string TargetObjectId => targetObjectId;

        public int RequiredCount => requiredCount < 1 ? 1 : requiredCount;

        public float TimeoutSeconds => timeoutSeconds;

        #endregion
    }
}
