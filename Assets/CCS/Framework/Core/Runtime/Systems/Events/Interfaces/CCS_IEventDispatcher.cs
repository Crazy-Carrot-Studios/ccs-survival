using System;

// =============================================================================
// SCRIPT: CCS_IEventDispatcher
// CATEGORY: Core / Runtime / Systems / Events
// PURPOSE: Contract for subscribing to and dispatching CCS runtime events.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Interface only. No static global instance. Compiled by CCS.Core.Runtime.
// =============================================================================

namespace CCS.Core
{
    public interface CCS_IEventDispatcher
    {
        void Subscribe<TEvent>(Action<TEvent> callback) where TEvent : CCS_IEvent;

        void Unsubscribe<TEvent>(Action<TEvent> callback) where TEvent : CCS_IEvent;

        void Dispatch<TEvent>(TEvent eventData) where TEvent : CCS_IEvent;

        void Clear();
    }
}
