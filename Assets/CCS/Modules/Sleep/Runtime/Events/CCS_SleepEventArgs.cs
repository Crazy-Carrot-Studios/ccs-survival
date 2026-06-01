using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SleepEventArgs
// CATEGORY: Modules / Sleep / Runtime / Events
// PURPOSE: Event payload for sleep spot and legacy request notifications.
// PLACEMENT: Raised by CCS_SleepService event handlers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.3 includes world sleep spot identity fields.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public sealed class CCS_SleepEventArgs
    {
        #region Constructors

        public CCS_SleepEventArgs(CCS_SleepResult result, CCS_SleepSnapshot snapshot, string message)
        {
            Result = result;
            Snapshot = snapshot ?? CCS_SleepSnapshot.Empty;
            Message = message ?? string.Empty;
            Success = result != null && result.IsSuccess;
        }

        public CCS_SleepEventArgs(
            CCS_SleepSpot sleepSpot,
            bool success,
            string message,
            CCS_SleepResult result = null,
            CCS_SleepSnapshot snapshot = null)
        {
            SleepSpotId = sleepSpot != null ? sleepSpot.SleepSpotId : string.Empty;
            InstanceId = sleepSpot != null ? sleepSpot.InstanceId : string.Empty;
            DisplayName = sleepSpot != null ? sleepSpot.DisplayName : string.Empty;
            WorldPosition = sleepSpot != null ? sleepSpot.transform.position : Vector3.zero;
            Success = success;
            Message = message ?? string.Empty;
            Result = result;
            Snapshot = snapshot ?? CCS_SleepSnapshot.Empty;
        }

        #endregion

        #region Properties

        public CCS_SleepResult Result { get; }

        public CCS_SleepSnapshot Snapshot { get; }

        public string Message { get; }

        public string SleepSpotId { get; }

        public string InstanceId { get; }

        public string DisplayName { get; }

        public Vector3 WorldPosition { get; }

        public bool Success { get; }

        #endregion
    }
}
