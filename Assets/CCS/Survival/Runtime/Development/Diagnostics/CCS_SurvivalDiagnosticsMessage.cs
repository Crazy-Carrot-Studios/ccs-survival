using System;

// =============================================================================
// SCRIPT: CCS_SurvivalDiagnosticsMessage
// CATEGORY: Survival / Runtime / Development / Diagnostics
// PURPOSE: Immutable diagnostic message payload for survival development diagnostics.
// PLACEMENT: Created by CCS_SurvivalDiagnosticsService or future module reporters.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Severity (Info/Warning/Error) is primary for module reports. State tracks lifecycle.
// =============================================================================

namespace CCS.Survival.Development
{
    public readonly struct CCS_SurvivalDiagnosticsMessage
    {
        #region Public Methods

        public CCS_SurvivalDiagnosticsMessage(
            string sourceSystemId,
            string message,
            CCS_SurvivalDiagnosticsSeverity severity,
            CCS_SurvivalDiagnosticsState state,
            DateTime utcTimestamp)
        {
            SourceSystemId = NormalizeSource(sourceSystemId);
            Message = string.IsNullOrWhiteSpace(message) ? "No diagnostic detail provided." : message.Trim();
            Severity = severity;
            State = state;
            UtcTimestamp = utcTimestamp;
        }

        public static CCS_SurvivalDiagnosticsMessage Create(
            string sourceSystemId,
            string message,
            CCS_SurvivalDiagnosticsSeverity severity)
        {
            return new CCS_SurvivalDiagnosticsMessage(
                sourceSystemId,
                message,
                severity,
                MapSeverityToState(severity),
                DateTime.UtcNow);
        }

        public static CCS_SurvivalDiagnosticsMessage Create(
            string sourceSystemId,
            string message,
            CCS_SurvivalDiagnosticsState state)
        {
            return new CCS_SurvivalDiagnosticsMessage(
                sourceSystemId,
                message,
                MapStateToSeverity(state),
                state,
                DateTime.UtcNow);
        }

        #endregion

        #region Properties

        public string SourceSystemId { get; }

        public string Message { get; }

        public CCS_SurvivalDiagnosticsSeverity Severity { get; }

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

        private static CCS_SurvivalDiagnosticsSeverity MapStateToSeverity(CCS_SurvivalDiagnosticsState state)
        {
            switch (state)
            {
                case CCS_SurvivalDiagnosticsState.Warning:
                    return CCS_SurvivalDiagnosticsSeverity.Warning;
                case CCS_SurvivalDiagnosticsState.Error:
                    return CCS_SurvivalDiagnosticsSeverity.Error;
                default:
                    return CCS_SurvivalDiagnosticsSeverity.Info;
            }
        }

        private static CCS_SurvivalDiagnosticsState MapSeverityToState(CCS_SurvivalDiagnosticsSeverity severity)
        {
            switch (severity)
            {
                case CCS_SurvivalDiagnosticsSeverity.Warning:
                    return CCS_SurvivalDiagnosticsState.Warning;
                case CCS_SurvivalDiagnosticsSeverity.Error:
                    return CCS_SurvivalDiagnosticsState.Error;
                default:
                    return CCS_SurvivalDiagnosticsState.Ready;
            }
        }

        #endregion
    }
}
