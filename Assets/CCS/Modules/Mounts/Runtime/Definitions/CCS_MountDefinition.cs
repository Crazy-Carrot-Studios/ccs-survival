using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MountDefinition
// CATEGORY: Modules / Mounts / Runtime / Definitions
// PURPOSE: Generic mount species definition (horse, mule, donkey, future animals).
// PLACEMENT: Assets/CCS/Survival/Content/Mounts/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    [CreateAssetMenu(
        fileName = "CCS_MountDefinition",
        menuName = "CCS/Survival/Mounts/Mount Definition")]
    public sealed class CCS_MountDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string mountId = "ccs.survival.mount.example";
        [SerializeField] private string displayName = "Mount";
        [SerializeField] private string description = string.Empty;

        [Header("Movement")]
        [SerializeField] private float movementSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 7.5f;
        [SerializeField] private float staminaPlaceholder = 100f;

        [Header("Economy / Carry")]
        [SerializeField] private int carryCapacityBonus;
        [SerializeField] private int purchaseValue = 2500;

        [Header("World")]
        [SerializeField] private GameObject worldPrefab;
        [SerializeField] private string saddlebagContainerDefinitionId = CCS_MountContentIds.HorseSaddlebagContainerId;
        [SerializeField] private int saddlebagSlotCount = 12;

        public string MountId => mountId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public string Description => description ?? string.Empty;

        public float MovementSpeed => movementSpeed < 0f ? 0f : movementSpeed;

        public float SprintSpeed => sprintSpeed < movementSpeed ? movementSpeed : sprintSpeed;

        public float StaminaPlaceholder => staminaPlaceholder < 0f ? 0f : staminaPlaceholder;

        public int CarryCapacityBonus => carryCapacityBonus < 0 ? 0 : carryCapacityBonus;

        public int PurchaseValue => purchaseValue < 0 ? 0 : purchaseValue;

        public GameObject WorldPrefab => worldPrefab;

        public string SaddlebagContainerDefinitionId => saddlebagContainerDefinitionId ?? string.Empty;

        public int SaddlebagSlotCount => saddlebagSlotCount < 1 ? 1 : saddlebagSlotCount;
    }
}
