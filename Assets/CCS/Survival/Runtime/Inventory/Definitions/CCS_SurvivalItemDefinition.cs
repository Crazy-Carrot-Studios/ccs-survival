using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalItemDefinition
// CATEGORY: Survival / Runtime / Inventory / Definitions
// PURPOSE: ScriptableObject authoring data for survival inventory items.
// PLACEMENT: Assets/CCS/Survival/Data/Items/. Referenced by inventory container and pickups.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: itemId must be save-stable (survival.item.*). Non-stackable items behave as max stack 1.
// =============================================================================

namespace CCS.Survival.Inventory
{
    [CreateAssetMenu(fileName = "ITM_NewItem", menuName = "CCS/Survival/Item Definition")]
    public sealed class CCS_SurvivalItemDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Save-stable lowercase reverse-DNS id (example: survival.item.food_tin).")]
        [SerializeField] private string itemId = "survival.item.prototype";

        [Tooltip("Readable label for UI, logs, and debug overlay.")]
        [SerializeField] private string displayName = "Prototype Item";

        [Tooltip("Optional longer description for future UI tooltips.")]
        [SerializeField] private string description;

        [Header("Presentation")]
        [Tooltip("Optional icon for future inventory UI.")]
        [SerializeField] private Sprite icon;

        [Tooltip("Optional debug color for prototype visuals.")]
        [SerializeField] private Color debugColor = Color.white;

        [Header("Classification")]
        [Tooltip("High-level item category for filtering and future systems.")]
        [SerializeField] private CCS_SurvivalItemCategory category = CCS_SurvivalItemCategory.Misc;

        [Tooltip("Optional string tags for crafting, filtering, or future rules.")]
        [SerializeField] private List<string> itemTags = new List<string>();

        [Header("Stacking")]
        [Tooltip("When false, effective max stack size is always 1.")]
        [SerializeField] private bool isStackable = true;

        [Tooltip("Maximum stack size when stackable. Minimum 1.")]
        [SerializeField] private int maxStackSize = 1;

        #endregion

        #region Properties

        public string ItemId => itemId;

        public string DisplayName => displayName;

        public string Description => description;

        public Sprite Icon => icon;

        public Color DebugColor => debugColor;

        public CCS_SurvivalItemCategory Category => category;

        public IReadOnlyList<string> ItemTags => itemTags;

        public bool IsStackable => isStackable;

        public int MaxStackSize => maxStackSize;

        #endregion

        #region Public Methods

        public int GetEffectiveMaxStackSize()
        {
            if (!isStackable)
            {
                return 1;
            }

            return Mathf.Max(1, maxStackSize);
        }

        public bool HasTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag) || itemTags == null)
            {
                return false;
            }

            for (int i = 0; i < itemTags.Count; i++)
            {
                if (string.Equals(itemTags[i], tag, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Unity Callbacks

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            itemId = itemId.Trim().ToLowerInvariant();
            maxStackSize = Mathf.Max(1, maxStackSize);
        }

        #endregion

        #region Internal Methods

        internal void ConfigureRuntimeTestData(
            string testItemId,
            string testDisplayName,
            CCS_SurvivalItemCategory testCategory,
            bool testIsStackable,
            int testMaxStackSize)
        {
            itemId = testItemId;
            displayName = testDisplayName;
            category = testCategory;
            isStackable = testIsStackable;
            maxStackSize = Mathf.Max(1, testMaxStackSize);
        }

        #endregion
    }
}
