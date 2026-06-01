using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ActiveItemProfile
// CATEGORY: Modules / Hotbar / Runtime / Profiles
// PURPOSE: Tuning profile for active item selection and use flow foundation.
// PLACEMENT: Assets/CCS/Survival/Profiles/Hotbar/CCS_DefaultActiveItemProfile.asset
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Not final hotbar UI. Placeholder cooldown for future networked authority.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    [CreateAssetMenu(
        fileName = "CCS_ActiveItemProfile",
        menuName = "CCS/Survival/Hotbar/Active Item Profile")]
    public sealed class CCS_ActiveItemProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Use")]
        [Tooltip("Placeholder cooldown between active item use attempts.")]
        [SerializeField] private float useCooldownSeconds;

        [Tooltip("When enabled, selecting from equipped MainHand auto-syncs after equip events.")]
        [SerializeField] private bool autoSelectMainHandOnEquip = true;

        [Header("Cycle")]
        [Tooltip("When enabled, cycle active item across occupied equipment slots.")]
        [SerializeField] private bool enableEquipmentSlotCycling = true;

        [Header("Tool Routing")]
        [Tooltip("When enabled, active tools route to CCS_GatheringService for gathering nodes.")]
        [SerializeField] private bool enableGatheringRouting = true;

        [Tooltip("When enabled, active tools route to harvestable world resources.")]
        [SerializeField] private bool enableResourceHarvestRouting = true;

        [Tooltip("When enabled, fishing poles route to CCS_FishingService for fishing spots.")]
        [SerializeField] private bool enableFishingRouting = true;

        #endregion

        #region Properties

        public float UseCooldownSeconds => useCooldownSeconds;

        public bool AutoSelectMainHandOnEquip => autoSelectMainHandOnEquip;

        public bool EnableEquipmentSlotCycling => enableEquipmentSlotCycling;

        public bool EnableGatheringRouting => enableGatheringRouting;

        public bool EnableResourceHarvestRouting => enableResourceHarvestRouting;

        public bool EnableFishingRouting => enableFishingRouting;

        #endregion
    }
}
