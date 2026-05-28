using System;
using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalTraversalValidationLifecycleEvent
// CATEGORY: Survival / Runtime / Survival / Events
// PURPOSE: Notifies listeners when dev traversal validation starts or stops.
// PLACEMENT: Dispatched through CCS_RuntimeHost.EventDispatcher (no static singleton).
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: CCS_TraversalTestAgent publishes; CCS_SurvivalModule subscribes for vitals isolation.
// =============================================================================

namespace CCS.Survival
{
    public readonly struct CCS_SurvivalTraversalValidationLifecycleEvent : CCS_IEvent
    {
        public CCS_SurvivalTraversalValidationLifecycleEvent(bool isActive)
        {
            IsActive = isActive;
            Timestamp = DateTime.UtcNow;
        }

        public bool IsActive { get; }

        public DateTime Timestamp { get; }
    }
}
