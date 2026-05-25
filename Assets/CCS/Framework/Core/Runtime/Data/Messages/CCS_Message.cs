using System;

// =============================================================================
// SCRIPT: CCS_Message
// CATEGORY: Core / Runtime / Data
// PURPOSE: Serializable framework message for future UI/debug/logging abstraction.
// PLACEMENT: Runtime utility type. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No Debug.Log integration in this milestone.
// =============================================================================

namespace CCS.Core
{
    [Serializable]
    public readonly struct CCS_Message
    {
        #region Properties

        public CCS_MessageType MessageType { get; }

        public string Message { get; }

        #endregion

        #region Public Methods

        public CCS_Message(CCS_MessageType messageType, string message)
        {
            MessageType = messageType;
            Message = message ?? string.Empty;
        }

        public override string ToString()
        {
            return $"[CCS_Message] {MessageType}: {Message}";
        }

        #endregion
    }
}
