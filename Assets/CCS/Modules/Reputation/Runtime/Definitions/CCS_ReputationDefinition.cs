using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReputationDefinition
// CATEGORY: Modules / Reputation / Runtime / Definitions
// PURPOSE: ScriptableObject definition for a reputation standing track.
// PLACEMENT: Assets/CCS/Survival/Content/Reputation/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Generic reputation template; western naming lives in Survival content assets.
// =============================================================================

namespace CCS.Modules.Reputation
{
    [CreateAssetMenu(
        fileName = "CCS_ReputationDefinition",
        menuName = "CCS/Survival/Reputation/Reputation Definition")]
    public sealed class CCS_ReputationDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable reverse-DNS reputation definition id.")]
        [SerializeField] private string reputationDefinitionId = string.Empty;

        [Tooltip("Player-facing reputation label.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Reputation scope for this standing track.")]
        [SerializeField] private CCS_ReputationScopeType scopeType = CCS_ReputationScopeType.Settlement;

        [Tooltip("Target id for the scope (settlement id, region id, service id, etc.).")]
        [SerializeField] private string targetId = string.Empty;

        [Header("Range")]
        [Tooltip("Minimum reputation value.")]
        [SerializeField] private int minValue = -100;

        [Tooltip("Maximum reputation value.")]
        [SerializeField] private int maxValue = 100;

        [Tooltip("Starting value when no save data exists.")]
        [SerializeField] private int defaultValue;

        [Header("Rules")]
        [Tooltip("When false, standing changes are ignored.")]
        [SerializeField] private bool enabled = true;

        [Tooltip("Future collateral or law hook placeholder.")]
        [SerializeField] private string futureHookPlaceholder = string.Empty;

        public string ReputationDefinitionId => reputationDefinitionId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public CCS_ReputationScopeType ScopeType => scopeType;

        public string TargetId => targetId ?? string.Empty;

        public int MinValue => minValue;

        public int MaxValue => maxValue;

        public int DefaultValue => defaultValue;

        public bool Enabled => enabled;

        public string FutureHookPlaceholder => futureHookPlaceholder ?? string.Empty;
    }
}
