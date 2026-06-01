using System;

// =============================================================================
// SCRIPT: CCS_EquipmentVisualRuntimeBridge
// CATEGORY: Modules / Equipment / Runtime / Visuals
// PURPOSE: Cross-scene read-only hooks for playtest and validation of equipped visuals.
// PLACEMENT: Updated by CCS_EquipmentVisualController on spawn/remove.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Avoids direct coupling from playtesting module to player MonoBehaviours.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public static class CCS_EquipmentVisualRuntimeBridge
    {
        public static event Action<string> VisualSpawned;
        public static event Action<string> VisualRemoved;

        public static bool HasActiveVisualForItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            return activeItemIds.Contains(itemId);
        }

        private static readonly System.Collections.Generic.HashSet<string> activeItemIds =
            new System.Collections.Generic.HashSet<string>();

        internal static void NotifyVisualSpawned(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            activeItemIds.Add(itemId);
            VisualSpawned?.Invoke(itemId);
        }

        internal static void NotifyVisualRemoved(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            activeItemIds.Remove(itemId);
            VisualRemoved?.Invoke(itemId);
        }

        internal static void ClearAll()
        {
            activeItemIds.Clear();
        }
    }
}
