// =============================================================================
// SCRIPT: CCS_WildlifeAiSnapshot
// CATEGORY: Modules / Wildlife / Runtime / AI
// PURPOSE: Read-only wildlife AI state snapshot for HUD debug and diagnostics.
// PLACEMENT: Created by CCS_WildlifeAgent and aggregated by CCS_WildlifeAiService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No combat or health data in 0.9.7 foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public readonly struct CCS_WildlifeAiSnapshot
    {
        #region Variables

        public static readonly CCS_WildlifeAiSnapshot Empty = new CCS_WildlifeAiSnapshot(string.Empty, CCS_WildlifeAiState.Idle);

        #endregion

        #region Properties

        public string AgentDisplayName { get; }

        public CCS_WildlifeAiState CurrentState { get; }

        public string DebugLine => string.IsNullOrWhiteSpace(AgentDisplayName)
            ? string.Empty
            : $"{AgentDisplayName} {CurrentState}";

        #endregion

        #region Public Methods

        public CCS_WildlifeAiSnapshot(string agentDisplayName, CCS_WildlifeAiState currentState)
        {
            AgentDisplayName = agentDisplayName ?? string.Empty;
            CurrentState = currentState;
        }

        #endregion
    }
}
