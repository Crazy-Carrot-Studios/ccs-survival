using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_IndustryDefinition
// CATEGORY: Modules / Industry / Runtime / Definitions
// PURPOSE: Single resource processing recipe for an industry workstation role.
// PLACEMENT: Assets/CCS/Survival/Content/Industry/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Industry
{
    [CreateAssetMenu(
        fileName = "CCS_IndustryDefinition",
        menuName = "CCS/Survival/Industry/Industry Definition")]
    public sealed class CCS_IndustryDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string processId = "ccs.survival.industry.process.example";
        [SerializeField] private string displayName = "Industry Process";
        [SerializeField] private string description = string.Empty;

        [Header("Workstation")]
        [SerializeField] private string requiredWorkstationRoleId = CCS_IndustryWorkstationRole.SawTable;

        [Header("Inputs / Outputs")]
        [SerializeField] private List<CCS_IndustryProcessStack> inputs = new List<CCS_IndustryProcessStack>();
        [SerializeField] private List<CCS_IndustryProcessStack> outputs = new List<CCS_IndustryProcessStack>();

        [Header("Timing")]
        [SerializeField] private float processTimeSeconds;

        [Header("Future Placeholders")]
        [SerializeField] private bool isFuturePlaceholder;

        public string ProcessId => processId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public string Description => description ?? string.Empty;

        public string RequiredWorkstationRoleId => requiredWorkstationRoleId ?? string.Empty;

        public IReadOnlyList<CCS_IndustryProcessStack> Inputs => inputs;

        public IReadOnlyList<CCS_IndustryProcessStack> Outputs => outputs;

        public float ProcessTimeSeconds => processTimeSeconds < 0f ? 0f : processTimeSeconds;

        public bool IsFuturePlaceholder => isFuturePlaceholder;
    }
}
