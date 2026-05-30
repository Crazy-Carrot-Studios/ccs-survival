using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentProfile
// CATEGORY: Modules / Equipment / Runtime / Profiles
// PURPOSE: Tuning profile for future equipment rules and slot policy.
// PLACEMENT: Assets/CCS/Survival/Profiles/Equipment/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Placeholder rules only in 0.4.1. No UI, combat, or visual references.
// =============================================================================

namespace CCS.Modules.Equipment
{
    [CreateAssetMenu(
        fileName = "CCS_EquipmentProfile",
        menuName = "CCS/Survival/Equipment/Equipment Profile")]
    public sealed class CCS_EquipmentProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Rules (Placeholder)")]
        [Tooltip("When enabled, future systems may allow dual wielding in hand slots.")]
        [SerializeField] private bool allowDualWield;

        [Tooltip("When enabled, future systems enforce durability checks before equipping.")]
        [SerializeField] private bool requireDurabilityForEquip;

        #endregion

        #region Properties

        public bool AllowDualWield => allowDualWield;

        public bool RequireDurabilityForEquip => requireDurabilityForEquip;

        #endregion
    }
}
