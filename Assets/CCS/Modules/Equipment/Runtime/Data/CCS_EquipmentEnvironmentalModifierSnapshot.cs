// =============================================================================
// SCRIPT: CCS_EquipmentEnvironmentalModifierSnapshot
// CATEGORY: Modules / Equipment / Runtime / Data
// PURPOSE: Aggregated environmental survival modifiers from equipped items.
// PLACEMENT: Produced by CCS_PlayerEquipmentService.GetEnvironmentalModifiers().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Survival modifiers only. No armor, combat stats, or damage reduction.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public readonly struct CCS_EquipmentEnvironmentalModifierSnapshot
    {
        #region Public Methods

        public CCS_EquipmentEnvironmentalModifierSnapshot(
            float temperatureResistance,
            float wetnessResistance,
            float exposureResistance)
        {
            TemperatureResistance = temperatureResistance < 0f ? 0f : temperatureResistance;
            WetnessResistance = wetnessResistance < 0f ? 0f : wetnessResistance;
            ExposureResistance = exposureResistance < 0f ? 0f : exposureResistance;
        }

        public static CCS_EquipmentEnvironmentalModifierSnapshot Empty =>
            new CCS_EquipmentEnvironmentalModifierSnapshot(0f, 0f, 0f);

        #endregion

        #region Properties

        public float TemperatureResistance { get; }

        public float WetnessResistance { get; }

        public float ExposureResistance { get; }

        #endregion
    }
}
