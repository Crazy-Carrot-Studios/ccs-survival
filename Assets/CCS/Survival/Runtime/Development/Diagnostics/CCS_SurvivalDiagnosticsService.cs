using System;
using System.Collections.Generic;
using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalDiagnosticsService
// CATEGORY: Survival / Runtime / Development / Diagnostics
// PURPOSE: Lightweight runtime diagnostics hub for module/system status without direct coupling.
// PLACEMENT: Registered manually by future development bootstrap wiring. Not a singleton.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Event-driven message feed with Info/Warning/Error severity. No gameplay references.
// =============================================================================

namespace CCS.Survival.Development
{
    public sealed class CCS_SurvivalDiagnosticsService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_SurvivalDiagnosticsService]";

        #region Variables

        private readonly List<CCS_SurvivalDiagnosticsMessage> messages = new List<CCS_SurvivalDiagnosticsMessage>(32);
        private readonly Dictionary<string, CCS_SurvivalDiagnosticsState> systemStates =
            new Dictionary<string, CCS_SurvivalDiagnosticsState>(16, StringComparer.Ordinal);

        private bool isInitialized;

        #endregion

        #region Events

        public event Action<CCS_SurvivalDiagnosticsMessage> MessageAdded;

        public event Action<string, CCS_SurvivalDiagnosticsState> SystemStateChanged;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            CCS_Logger.Log(LogPrefix, "Development diagnostics service initialized.");
        }

        public void ReportMessage(CCS_SurvivalDiagnosticsMessage message)
        {
            messages.Add(message);
            systemStates[message.SourceSystemId] = message.State;
            LogMessage(message);
            MessageAdded?.Invoke(message);
            SystemStateChanged?.Invoke(message.SourceSystemId, message.State);
        }

        public void ReportMessage(
            string sourceSystemId,
            string message,
            CCS_SurvivalDiagnosticsSeverity severity)
        {
            ReportMessage(CCS_SurvivalDiagnosticsMessage.Create(sourceSystemId, message, severity));
        }

        public void ReportMessage(string sourceSystemId, string message, CCS_SurvivalDiagnosticsState state)
        {
            ReportMessage(CCS_SurvivalDiagnosticsMessage.Create(sourceSystemId, message, state));
        }

        public void SetSystemState(string sourceSystemId, CCS_SurvivalDiagnosticsState state, string detailMessage)
        {
            if (string.IsNullOrWhiteSpace(sourceSystemId))
            {
                CCS_Logger.LogWarning(LogPrefix, "SetSystemState ignored because sourceSystemId was empty.");
                return;
            }

            string normalizedSource = sourceSystemId.Trim();
            systemStates[normalizedSource] = state;
            CCS_Logger.Log(LogPrefix, $"{normalizedSource} state -> {state}. {detailMessage}");
            SystemStateChanged?.Invoke(normalizedSource, state);

            if (!string.IsNullOrWhiteSpace(detailMessage))
            {
                ReportMessage(normalizedSource, detailMessage, state);
            }
        }

        public bool TryGetSystemState(string sourceSystemId, out CCS_SurvivalDiagnosticsState state)
        {
            state = CCS_SurvivalDiagnosticsState.Unknown;

            if (string.IsNullOrWhiteSpace(sourceSystemId))
            {
                return false;
            }

            return systemStates.TryGetValue(sourceSystemId.Trim(), out state);
        }

        public IReadOnlyList<CCS_SurvivalDiagnosticsMessage> GetMessages()
        {
            return messages;
        }

        public void Clear()
        {
            messages.Clear();
            systemStates.Clear();
            CCS_Logger.Log(LogPrefix, "Diagnostics messages and system states cleared.");
        }

        #endregion

        #region Private Methods

        private static void LogMessage(CCS_SurvivalDiagnosticsMessage message)
        {
            string formatted =
                $"{message.SourceSystemId} [{message.Severity}] [{message.State}] {message.Message}";

            switch (message.Severity)
            {
                case CCS_SurvivalDiagnosticsSeverity.Error:
                    CCS_Logger.LogWarning(LogPrefix, formatted);
                    break;
                case CCS_SurvivalDiagnosticsSeverity.Warning:
                    CCS_Logger.LogWarning(LogPrefix, formatted);
                    break;
                default:
                    CCS_Logger.Log(LogPrefix, formatted);
                    break;
            }
        }

        #endregion
    }
}
