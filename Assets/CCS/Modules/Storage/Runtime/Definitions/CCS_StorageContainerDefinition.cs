using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StorageContainerDefinition
// CATEGORY: Modules / Storage / Runtime / Definitions
// PURPOSE: ScriptableObject definition for primitive storage container placement and capacity.
// PLACEMENT: Assets/CCS/Survival/Content/Storage/Primitive/
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.2 storage container foundation. Primitives only.
// =============================================================================

namespace CCS.Modules.Storage
{
    [CreateAssetMenu(
        fileName = "CCS_StorageContainerDefinition",
        menuName = "CCS/Survival/Storage/Storage Container Definition")]
    public sealed class CCS_StorageContainerDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS container definition id.")]
        [SerializeField] private string containerId = string.Empty;

        [Tooltip("Player-facing container label.")]
        [SerializeField] private string displayName = "Storage Crate";

        [Header("Capacity")]
        [Tooltip("Number of inventory slots inside the container.")]
        [SerializeField] private int slotCount = 8;

        [Tooltip("Optional maximum total weight. Zero disables weight enforcement.")]
        [SerializeField] private float maxWeight;

        [Header("Placement")]
        [Tooltip("Prefab spawned when this container is placed in the world.")]
        [SerializeField] private GameObject prefabReference;

        [Tooltip("Optional kit item used for frontier placeable storage placement.")]
        [SerializeField] private CCS_ItemDefinition placeableKitItem;

        [Tooltip("When true, placed instances within camp radius count toward FrontierCamp tier.")]
        [SerializeField] private bool contributesToCampTier;

        [Header("Diagnostics")]
        [Tooltip("Emit storage container debug logs for this definition.")]
        [SerializeField] private bool enableDebugLogging;

        #endregion

        #region Properties

        public string ContainerId => containerId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public int SlotCount => slotCount > 0 ? slotCount : 1;

        public float MaxWeight => maxWeight < 0f ? 0f : maxWeight;

        public bool HasMaxWeight => maxWeight > 0f;

        public GameObject PrefabReference => prefabReference;

        public CCS_ItemDefinition PlaceableKitItem => placeableKitItem;

        public bool ContributesToCampTier => contributesToCampTier;

        public bool EnableDebugLogging => enableDebugLogging;

        #endregion
    }
}
