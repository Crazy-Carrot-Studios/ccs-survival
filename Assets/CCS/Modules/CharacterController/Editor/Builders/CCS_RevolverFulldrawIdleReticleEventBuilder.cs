using System;
using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverFulldrawIdleReticleEventBuilder
// CATEGORY: Modules / CharacterController / Editor / Builders
// PURPOSE: Ensures Fulldraw_Idle hold clip has reticle reveal Animation Event metadata.
// PLACEMENT: Editor builder invoked from aim layer and reticle reveal validation batches.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Modifies FBX importer clip events only. Does not edit animation curves.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverFulldrawIdleReticleEventBuilder
    {
        public static bool EnsureFulldrawIdleReticleRevealAnimationEvent(out float eventTimeUsed)
        {
            eventTimeUsed = CCS_CharacterControllerConstants.RevolverAimHoldReticleRevealAnimationEventPreferredTime;
            string assetPath = CCS_CharacterControllerConstants.WildWestFulldrawIdleClipPath;
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError("[Fulldraw Idle Reticle Event Builder] Missing ModelImporter for " + assetPath);
                return false;
            }

            ModelImporterClipAnimation[] clipAnimations = importer.clipAnimations;
            if (clipAnimations == null || clipAnimations.Length == 0)
            {
                Debug.LogError("[Fulldraw Idle Reticle Event Builder] Missing clipAnimations on " + assetPath);
                return false;
            }

            bool changed = false;
            for (int i = 0; i < clipAnimations.Length; i++)
            {
                if (!string.Equals(
                        clipAnimations[i].name,
                        CCS_CharacterControllerConstants.WildWestFulldrawIdleClipName,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                AnimationEvent[] existingEvents = clipAnimations[i].events ?? Array.Empty<AnimationEvent>();
                int matchingCount = CountMatchingEvents(existingEvents);
                if (matchingCount == 1)
                {
                    eventTimeUsed = existingEvents[0].time;
                    return false;
                }

                if (matchingCount > 1)
                {
                    existingEvents = RemoveMatchingEvents(existingEvents);
                    changed = true;
                }

                AnimationEvent revealEvent = new AnimationEvent
                {
                    functionName = CCS_CharacterControllerConstants.RevolverAimHoldReticleRevealAnimationEventName,
                    time = CCS_CharacterControllerConstants.RevolverAimHoldReticleRevealAnimationEventPreferredTime,
                };

                AnimationEvent[] updatedEvents = new AnimationEvent[existingEvents.Length + 1];
                Array.Copy(existingEvents, updatedEvents, existingEvents.Length);
                updatedEvents[updatedEvents.Length - 1] = revealEvent;
                clipAnimations[i].events = updatedEvents;
                eventTimeUsed = revealEvent.time;
                changed = true;
                break;
            }

            if (!changed)
            {
                return false;
            }

            importer.clipAnimations = clipAnimations;
            importer.SaveAndReimport();
            AssetDatabase.SaveAssets();
            return true;
        }

        public static bool TryReadFulldrawIdleReticleEventTime(out float eventTime, out int matchingEventCount)
        {
            eventTime = -1f;
            matchingEventCount = 0;
            string assetPath = CCS_CharacterControllerConstants.WildWestFulldrawIdleClipPath;
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                return false;
            }

            ModelImporterClipAnimation[] clipAnimations = importer.clipAnimations;
            if (clipAnimations == null)
            {
                return false;
            }

            for (int i = 0; i < clipAnimations.Length; i++)
            {
                if (!string.Equals(
                        clipAnimations[i].name,
                        CCS_CharacterControllerConstants.WildWestFulldrawIdleClipName,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                AnimationEvent[] events = clipAnimations[i].events ?? Array.Empty<AnimationEvent>();
                matchingEventCount = CountMatchingEvents(events);
                if (matchingEventCount > 0)
                {
                    eventTime = events[0].time;
                }

                return true;
            }

            return false;
        }

        private static int CountMatchingEvents(AnimationEvent[] events)
        {
            int count = 0;
            for (int i = 0; i < events.Length; i++)
            {
                if (IsMatchingEvent(events[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private static AnimationEvent[] RemoveMatchingEvents(AnimationEvent[] events)
        {
            int keptCount = 0;
            for (int i = 0; i < events.Length; i++)
            {
                if (!IsMatchingEvent(events[i]))
                {
                    keptCount++;
                }
            }

            if (keptCount == events.Length)
            {
                return events;
            }

            AnimationEvent[] kept = new AnimationEvent[keptCount];
            int writeIndex = 0;
            for (int i = 0; i < events.Length; i++)
            {
                if (!IsMatchingEvent(events[i]))
                {
                    kept[writeIndex++] = events[i];
                }
            }

            return kept;
        }

        private static bool IsMatchingEvent(AnimationEvent animationEvent)
        {
            return animationEvent != null
                && string.Equals(
                    animationEvent.functionName,
                    CCS_CharacterControllerConstants.RevolverAimHoldReticleRevealAnimationEventName,
                    StringComparison.Ordinal);
        }
    }
}
