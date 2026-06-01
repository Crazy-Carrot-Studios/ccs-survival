// =============================================================================
// SCRIPT: CCS_FishingSnapshot
// CATEGORY: Modules / Fishing / Runtime / Data
// PURPOSE: Lightweight fishing service state for diagnostics and future save hooks.
// PLACEMENT: Created by CCS_FishingService.CreateSnapshot.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Foundation snapshot only. No spot depletion state in 1.2.5.
// =============================================================================

namespace CCS.Modules.Fishing
{
    public sealed class CCS_FishingSnapshot
    {
        public CCS_FishingSnapshot(
            bool isInitialized,
            int registeredSpotCount,
            string lastResultMessage)
        {
            IsInitialized = isInitialized;
            RegisteredSpotCount = registeredSpotCount;
            LastResultMessage = lastResultMessage ?? string.Empty;
        }

        public bool IsInitialized { get; }

        public int RegisteredSpotCount { get; }

        public string LastResultMessage { get; }
    }
}
