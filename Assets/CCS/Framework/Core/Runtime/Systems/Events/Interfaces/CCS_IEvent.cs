using System;

// =============================================================================
// SCRIPT: CCS_IEvent
// CATEGORY: Core / Runtime / Systems / Events
// PURPOSE: Base contract for all CCS runtime events.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Interface only. Lightweight data contract. Compiled by CCS.Core.Runtime.
// =============================================================================

namespace CCS.Core
{
    public interface CCS_IEvent
    {
        DateTime Timestamp { get; }
    }
}
