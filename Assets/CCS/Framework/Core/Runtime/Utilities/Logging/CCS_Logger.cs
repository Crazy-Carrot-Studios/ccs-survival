using UnityEngine;

// =============================================================================
// SCRIPT: CCS_Logger
// CATEGORY: Core / Runtime / Utilities
// PURPOSE: Centralized CCS log formatting and output helpers.
// PLACEMENT: Static runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Uses UnityEngine.Debug only. No file logging or Editor references.
// =============================================================================

namespace CCS.Core
{
    public static class CCS_Logger
    {
        #region Public Methods

        public static void Log(string category, string message, bool isEnabled = true)
        {
            if (!isEnabled)
            {
                return;
            }

            Debug.Log(FormatMessage(category, message));
        }

        public static void LogWarning(string category, string message)
        {
            Debug.LogWarning(FormatMessage(category, message));
        }

        public static void LogError(string category, string message)
        {
            Debug.LogError(FormatMessage(category, message));
        }

        public static string FormatMessage(string category, string message)
        {
            string safeCategory = string.IsNullOrEmpty(category) ? "Core" : category;
            string safeMessage = message ?? string.Empty;
            return $"[CCS {safeCategory}] {safeMessage}";
        }

        #endregion
    }
}
