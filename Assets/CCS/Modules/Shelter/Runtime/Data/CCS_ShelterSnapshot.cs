// =============================================================================
// SCRIPT: CCS_ShelterSnapshot
// CATEGORY: Modules / Shelter / Runtime / Data
// PURPOSE: Read-only shelter state snapshot for HUD and environment integration.
// PLACEMENT: Produced by CCS_ShelterService.GetSnapshot().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Local protection only. No weather or survival stat mutation.
// =============================================================================

namespace CCS.Modules.Shelter
{
    public readonly struct CCS_ShelterSnapshot
    {
        #region Public Methods

        public CCS_ShelterSnapshot(
            bool isSheltered,
            string activeShelterId,
            float wetnessProtection,
            float exposureProtection,
            float temperatureProtection,
            float protectionMultiplier)
        {
            IsSheltered = isSheltered;
            ActiveShelterId = activeShelterId ?? string.Empty;
            WetnessProtection = wetnessProtection < 0f ? 0f : wetnessProtection;
            ExposureProtection = exposureProtection < 0f ? 0f : exposureProtection;
            TemperatureProtection = temperatureProtection;
            ProtectionMultiplier = protectionMultiplier <= 0f ? 1f : protectionMultiplier;
        }

        public static CCS_ShelterSnapshot Empty =>
            new CCS_ShelterSnapshot(false, string.Empty, 0f, 0f, 0f, 1f);

        public CCS_ShelterModifierSnapshot ToModifierSnapshot()
        {
            if (!IsSheltered)
            {
                return CCS_ShelterModifierSnapshot.Empty;
            }

            return new CCS_ShelterModifierSnapshot(
                WetnessProtection,
                ExposureProtection,
                TemperatureProtection,
                ProtectionMultiplier);
        }

        #endregion

        #region Properties

        public bool IsSheltered { get; }

        public string ActiveShelterId { get; }

        public float WetnessProtection { get; }

        public float ExposureProtection { get; }

        public float TemperatureProtection { get; }

        public float ProtectionMultiplier { get; }

        #endregion
    }
}
