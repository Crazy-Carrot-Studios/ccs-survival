using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FishingProfile
// CATEGORY: Modules / Fishing / Runtime / Profiles
// PURPOSE: Tuning profile for fishing service, default catch table, and item catalog.
// PLACEMENT: Assets/CCS/Survival/Profiles/Fishing/CCS_DefaultFishingProfile.asset
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Foundation profile for 1.2.5. No minigame tuning fields yet.
// =============================================================================

namespace CCS.Modules.Fishing
{
    [CreateAssetMenu(
        fileName = "CCS_FishingProfile",
        menuName = "CCS/Survival/Fishing/Fishing Profile")]
    public sealed class CCS_FishingProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Interaction")]
        [Tooltip("Fallback interaction distance when spot definition distance is zero.")]
        [SerializeField] private float defaultInteractionDistance = 4f;

        [Header("Catalog")]
        [Tooltip("Item definitions used to resolve catch itemDefinitionId strings.")]
        [SerializeField] private CCS_ItemDefinition[] itemCatalog;

        [Header("Default Catch Table")]
        [Tooltip("Used when a fishing spot has no per-spot catch table.")]
        [SerializeField] private CCS_FishingCatchDefinition[] defaultCatchTable =
        {
            new CCS_FishingCatchDefinition
            {
                catchKind = CCS_FishingCatchKind.Fish,
                itemDefinitionId = "ccs.survival.item.resource.rawfish",
                quantity = 1,
                weight = 35
            },
            new CCS_FishingCatchDefinition
            {
                catchKind = CCS_FishingCatchKind.SmallFish,
                itemDefinitionId = "ccs.survival.item.resource.smallfish",
                quantity = 1,
                weight = 25
            },
            new CCS_FishingCatchDefinition
            {
                catchKind = CCS_FishingCatchKind.Junk,
                itemDefinitionId = "ccs.survival.item.resource.junk",
                quantity = 1,
                weight = 10
            },
            new CCS_FishingCatchDefinition
            {
                catchKind = CCS_FishingCatchKind.Nothing,
                weight = 30
            }
        };

        private readonly Dictionary<string, CCS_ItemDefinition> itemLookup = new Dictionary<string, CCS_ItemDefinition>();

        #endregion

        #region Properties

        public float DefaultInteractionDistance => defaultInteractionDistance;

        public CCS_ItemDefinition[] ItemCatalog => itemCatalog;

        public CCS_FishingCatchDefinition[] DefaultCatchTable => defaultCatchTable;

        #endregion

        #region Public Methods

        public void BuildItemLookup()
        {
            itemLookup.Clear();
            if (itemCatalog == null)
            {
                return;
            }

            for (int index = 0; index < itemCatalog.Length; index++)
            {
                CCS_ItemDefinition itemDefinition = itemCatalog[index];
                if (itemDefinition == null || string.IsNullOrWhiteSpace(itemDefinition.ItemId))
                {
                    continue;
                }

                itemLookup[itemDefinition.ItemId] = itemDefinition;
            }
        }

        public bool TryResolveItem(string itemDefinitionId, out CCS_ItemDefinition itemDefinition)
        {
            itemDefinition = null;
            if (string.IsNullOrWhiteSpace(itemDefinitionId))
            {
                return false;
            }

            return itemLookup.TryGetValue(itemDefinitionId, out itemDefinition);
        }

        #endregion
    }
}
