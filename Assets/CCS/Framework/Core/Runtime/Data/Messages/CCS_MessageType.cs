// =============================================================================
// SCRIPT: CCS_MessageType
// CATEGORY: Core / Runtime / Data
// PURPOSE: Classifies framework messages for UI, debug, and logging layers.
// PLACEMENT: Runtime utility enum. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No Debug.Log integration in this milestone.
// =============================================================================

namespace CCS.Core
{
    public enum CCS_MessageType
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Success = 3
    }
}
