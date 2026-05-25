using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_EventDispatcher
// CATEGORY: Core / Runtime / Systems / Events
// PURPOSE: Lightweight runtime dispatcher for decoupled CCS event communication.
// PLACEMENT: Instantiated by bootstrap code. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No static singleton. No async/threading. No UnityEditor references.
// =============================================================================

namespace CCS.Core
{
    public sealed class CCS_EventDispatcher : CCS_IEventDispatcher
    {
        private const string LogCategory = "Event Dispatcher";

        #region Variables

        private readonly Dictionary<Type, Delegate> registeredCallbacks;
        private readonly bool enableDebugLogs;

        #endregion

        #region Public Methods

        public CCS_EventDispatcher()
            : this(false)
        {
        }

        public CCS_EventDispatcher(bool enableDebugLogs)
        {
            registeredCallbacks = new Dictionary<Type, Delegate>();
            this.enableDebugLogs = enableDebugLogs;
        }

        public void Subscribe<TEvent>(Action<TEvent> callback) where TEvent : CCS_IEvent
        {
            if (!CCS_Validation.IsObjectValid(callback))
            {
                return;
            }

            Type eventType = typeof(TEvent);
            if (registeredCallbacks.TryGetValue(eventType, out Delegate existingCallback))
            {
                registeredCallbacks[eventType] = Delegate.Combine(existingCallback, callback);
            }
            else
            {
                registeredCallbacks[eventType] = callback;
            }

            CCS_Logger.Log(LogCategory, $"Subscribed to event: {eventType.Name}", enableDebugLogs);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> callback) where TEvent : CCS_IEvent
        {
            if (!CCS_Validation.IsObjectValid(callback))
            {
                return;
            }

            Type eventType = typeof(TEvent);
            if (!registeredCallbacks.TryGetValue(eventType, out Delegate existingCallback))
            {
                return;
            }

            Delegate updatedCallback = Delegate.Remove(existingCallback, callback);
            if (updatedCallback == null)
            {
                registeredCallbacks.Remove(eventType);
            }
            else
            {
                registeredCallbacks[eventType] = updatedCallback;
            }

            CCS_Logger.Log(LogCategory, $"Unsubscribed from event: {eventType.Name}", enableDebugLogs);
        }

        public void Dispatch<TEvent>(TEvent eventData) where TEvent : CCS_IEvent
        {
            if (!CCS_Validation.IsObjectValid(eventData))
            {
                return;
            }

            Type eventType = typeof(TEvent);
            if (!registeredCallbacks.TryGetValue(eventType, out Delegate callback))
            {
                return;
            }

            if (callback is Action<TEvent> typedCallback)
            {
                typedCallback.Invoke(eventData);
                CCS_Logger.Log(LogCategory, $"Dispatched event: {eventType.Name}", enableDebugLogs);
            }
        }

        public void Clear()
        {
            registeredCallbacks.Clear();
            CCS_Logger.Log(LogCategory, "Cleared all event subscriptions.", enableDebugLogs);
        }

        public int GetSubscriptionCount()
        {
            int subscriptionCount = 0;

            foreach (KeyValuePair<Type, Delegate> entry in registeredCallbacks)
            {
                if (entry.Value == null)
                {
                    continue;
                }

                subscriptionCount += entry.Value.GetInvocationList().Length;
            }

            return subscriptionCount;
        }

        #endregion
    }
}
