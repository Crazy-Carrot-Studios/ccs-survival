using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StorageEventArgs
// CATEGORY: Modules / Storage / Runtime / Events
// PURPOSE: Event payload for storage container open, transfer, and restore notifications.
// PLACEMENT: Raised by CCS_StorageService and CCS_StorageContainer.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Includes container identity, world position, success state, and message.
// =============================================================================

namespace CCS.Modules.Storage
{
    public sealed class CCS_StorageEventArgs
    {
        #region Public Methods

        public CCS_StorageEventArgs(
            string containerId,
            string instanceId,
            string displayName,
            Vector3 worldPosition,
            bool isSuccess,
            string message)
        {
            ContainerId = containerId ?? string.Empty;
            InstanceId = instanceId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            WorldPosition = worldPosition;
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public string ContainerId { get; }

        public string InstanceId { get; }

        public string DisplayName { get; }

        public Vector3 WorldPosition { get; }

        public bool IsSuccess { get; }

        public string Message { get; }

        #endregion
    }
}
