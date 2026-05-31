// =============================================================================
// SCRIPT: CCS_ShelterModifierSnapshot
// CATEGORY: Modules / Shelter / Runtime / Data
// PURPOSE: Read-only shelter protection values consumed by Environment Effects.
// PLACEMENT: Embedded in CCS_EnvironmentSnapshot and CCS_ShelterSnapshot.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Protection only. No building placement or structure durability.
// =============================================================================

namespace CCS.Modules.Shelter
{
    public readonly struct CCS_ShelterModifierSnapshot
    {
        #region Public Methods

        public CCS_ShelterModifierSnapshot(
            float wetnessProtection,
            float exposureProtection,
            float temperatureProtection,
            float protectionMultiplier)
        {
            WetnessProtection = wetnessProtection < 0f ? 0f : wetnessProtection;
            ExposureProtection = exposureProtection < 0f ? 0f : exposureProtection;
            TemperatureProtection = temperatureProtection;
            ProtectionMultiplier = protectionMultiplier <= 0f ? 1f : protectionMultiplier;
        }

        public static CCS_ShelterModifierSnapshot Empty =>
            new CCS_ShelterModifierSnapshot(0f, 0f, 0f, 1f);

        #endregion

        #region Properties

        public float WetnessProtection { get; }

        public float ExposureProtection { get; }

        public float TemperatureProtection { get; }

        public float ProtectionMultiplier { get; }

        #endregion
    }
}
