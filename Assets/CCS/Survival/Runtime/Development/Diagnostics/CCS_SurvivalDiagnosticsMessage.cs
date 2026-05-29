using System;

// =============================================================================
// SCRIPT: CCS_SurvivalDiagnosticsMessage
// CATEGORY: Survival / Runtime / Development / Diagnostics
// PURPOSE: Immutable diagnostic message payload for survival development diagnostics.
// PLACEMENT: Created by CCS_SurvivalDiagnosticsService or future module reporters.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Avoid gameplay references. Source should be a stable system or module id string.
// =============================================================================

namespace CCS.Survival.Development
{
    public readonly struct CCS_SurvivalDiagnosticsMessage
    {
        #region Public Methods

        public CCS_SurvivalDiagnosticsMessage(
            string sourceSystemId,
            string message,
            CCS_SurvivalDiagnosticsState state,
            DateTime utcTimestamp)
        {
            SourceSystemId = NormalizeSource(sourceSystemId);
            Message = string.IsNullOrWhiteSpace(message) ? "No diagnostic detail provided." : message.Trim();
            State = state;
            UtcTimestamp = utcTimestamp;
        }

        public static CCS_SurvivalDiagnosticsMessage Create(
            string sourceSystemId,
            string message,
            CCS_SurvivalDiagnosticsState state)
        {
            return new CCS_SurvivalDiagnosticsMessage(sourceSystemId, message, state, DateTime.UtcNow);
        }

        #endregion

        #region Properties

        public string SourceSystemId { get; }

        public string Message { get; }

        public CCS_SurvivalDiagnosticsState State { get; }

        public DateTime UtcTimestamp { get; }

        #endregion

        #region Private Methods

        private static string NormalizeSource(string sourceSystemId)
        {
            if (string.IsNullOrWhiteSpace(sourceSystemId))
            {
                return "ccs.survival.development.unknown";
            }

            return sourceSystemId.Trim();
        }

        #endregion
    }
}
