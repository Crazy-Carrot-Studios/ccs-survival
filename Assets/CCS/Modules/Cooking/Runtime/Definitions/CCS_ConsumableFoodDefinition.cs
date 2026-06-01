using System;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ConsumableFoodDefinition
// CATEGORY: Modules / Cooking / Runtime / Definitions
// PURPOSE: Maps inventory food items to hunger restoration and consume pacing rules.
// PLACEMENT: Serialized on CCS_CookingProfile assets.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No health restore or buffs in 0.9.4 foundation. Cooldown pacing in 0.9.5.
// =============================================================================

namespace CCS.Modules.Cooking
{
    [Serializable]
    public sealed class CCS_ConsumableFoodDefinition
    {
        #region Variables

        [Tooltip("Inventory item that may be consumed.")]
        [SerializeField] private CCS_ItemDefinition itemDefinition;

        [Tooltip("Hunger restored when one unit is consumed.")]
        [SerializeField] private float hungerRestoreAmount = 10f;

        [Tooltip("Optional per-item consume cooldown override. Zero uses survival core profile default.")]
        [SerializeField] private float consumeCooldownSeconds;

        [Tooltip("Optional notification label override. Falls back to item display name.")]
        [SerializeField] private string notificationDisplayName = string.Empty;

        #endregion

        #region Properties

        public CCS_ItemDefinition ItemDefinition => itemDefinition;

        public float HungerRestoreAmount => hungerRestoreAmount;

        public float ConsumeCooldownSeconds => consumeCooldownSeconds;

        public string NotificationDisplayName => notificationDisplayName;

        #endregion

        #region Public Methods

        public string ResolveNotificationDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(notificationDisplayName))
            {
                return notificationDisplayName;
            }

            if (itemDefinition != null && !string.IsNullOrWhiteSpace(itemDefinition.DisplayName))
            {
                return itemDefinition.DisplayName;
            }

            return "Food";
        }

        #endregion
    }
}
