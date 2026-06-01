using CCS.Modules.Crafting;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SleepProfile
// CATEGORY: Modules / Sleep / Runtime / Profiles
// PURPOSE: Tuning profile for sleep duration, fatigue restore, and bedroll rules.
// PLACEMENT: Assets/CCS/Survival/Profiles/Sleep/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No dreams, death, enemy interruption, or sleep UI in 0.9.6 foundation.
// =============================================================================

namespace CCS.Modules.Sleep
{
    [CreateAssetMenu(
        fileName = "CCS_SleepProfile",
        menuName = "CCS/Survival/Sleep/Sleep Profile")]
    public sealed class CCS_SleepProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Sleep Duration")]
        [Tooltip("Default hours advanced when a sleep request omits a valid hour count.")]
        [SerializeField] private float defaultSleepHours = 6f;

        [Tooltip("Minimum allowed sleep hours per request.")]
        [SerializeField] private float minimumSleepHours = 1f;

        [Tooltip("Maximum allowed sleep hours per request.")]
        [SerializeField] private float maximumSleepHours = 10f;

        [Header("Fatigue Restore")]
        [Tooltip("Fatigue reduced per game hour slept at full shelter effectiveness.")]
        [SerializeField] private float fatigueRestorePerHour = 12f;

        [Header("Requirements")]
        [Tooltip("When enabled, the player must carry or equip a configured bedroll item.")]
        [SerializeField] private bool requireBedroll = true;

        [Tooltip("When enabled, unsheltered sleep attempts fail instead of using the penalty multiplier.")]
        [SerializeField] private bool requireShelter;

        [Tooltip("Multiplier applied to fatigue restore when sleeping outside shelter.")]
        [SerializeField] private float unshelteredFatigueRestoreMultiplier = 0.5f;

        [Header("Sleep Stat Drain")]
        [Tooltip("Multiplier applied to hunger drain simulated across slept game hours.")]
        [SerializeField] private float hungerDrainDuringSleepMultiplier = 1f;

        [Tooltip("Multiplier applied to thirst drain simulated across slept game hours.")]
        [SerializeField] private float thirstDrainDuringSleepMultiplier = 1f;

        [Header("Bedroll Content")]
        [Tooltip("Inventory item that satisfies the bedroll requirement.")]
        [SerializeField] private CCS_ItemDefinition bedrollItemDefinition;

        [Tooltip("Equipment definition used when checking equipped bedroll slot.")]
        [SerializeField] private CCS_EquipmentItemDefinition bedrollEquipmentDefinition;

        [Tooltip("Primitive hand recipe used to craft a bedroll.")]
        [SerializeField] private CCS_CraftingRecipeDefinition bedrollRecipeDefinition;

        #endregion

        #region Properties

        public float DefaultSleepHours => defaultSleepHours;

        public float MinimumSleepHours => minimumSleepHours;

        public float MaximumSleepHours => maximumSleepHours;

        public float FatigueRestorePerHour => fatigueRestorePerHour;

        public bool RequireBedroll => requireBedroll;

        public bool RequireShelter => requireShelter;

        public float UnshelteredFatigueRestoreMultiplier => unshelteredFatigueRestoreMultiplier;

        public float HungerDrainDuringSleepMultiplier => hungerDrainDuringSleepMultiplier;

        public float ThirstDrainDuringSleepMultiplier => thirstDrainDuringSleepMultiplier;

        public CCS_ItemDefinition BedrollItemDefinition => bedrollItemDefinition;

        public CCS_EquipmentItemDefinition BedrollEquipmentDefinition => bedrollEquipmentDefinition;

        public CCS_CraftingRecipeDefinition BedrollRecipeDefinition => bedrollRecipeDefinition;

        #endregion
    }
}
