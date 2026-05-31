// =============================================================================
// SCRIPT: CCS_ShelterState
// CATEGORY: Modules / Shelter / Runtime / Data
// PURPOSE: Mutable runtime shelter state owned by CCS_ShelterService.
// PLACEMENT: Internal to shelter service. Exposed through CCS_ShelterSnapshot.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Safe when no active shelter exists.
// =============================================================================

namespace CCS.Modules.Shelter
{
    public sealed class CCS_ShelterState
    {
        #region Variables

        public bool IsSheltered;

        public string ActiveShelterId = string.Empty;

        public float WetnessProtection;

        public float ExposureProtection;

        public float TemperatureProtection;

        public float ProtectionMultiplier = 1f;

        #endregion

        #region Public Methods

        public void Clear()
        {
            IsSheltered = false;
            ActiveShelterId = string.Empty;
            WetnessProtection = 0f;
            ExposureProtection = 0f;
            TemperatureProtection = 0f;
            ProtectionMultiplier = 1f;
        }

        public void ApplyShelter(
            string shelterId,
            float wetnessProtection,
            float exposureProtection,
            float temperatureProtection,
            float protectionMultiplier)
        {
            IsSheltered = true;
            ActiveShelterId = shelterId ?? string.Empty;
            WetnessProtection = wetnessProtection < 0f ? 0f : wetnessProtection;
            ExposureProtection = exposureProtection < 0f ? 0f : exposureProtection;
            TemperatureProtection = temperatureProtection;
            ProtectionMultiplier = protectionMultiplier <= 0f ? 1f : protectionMultiplier;
        }

        public CCS_ShelterSnapshot CreateSnapshot()
        {
            return new CCS_ShelterSnapshot(
                IsSheltered,
                ActiveShelterId,
                WetnessProtection,
                ExposureProtection,
                TemperatureProtection,
                ProtectionMultiplier);
        }

        #endregion
    }
}
