using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CampSnapshot
// CATEGORY: Modules / Shelter / Runtime / Data
// PURPOSE: Read-only camp state for HUD, playtest, and save validation.
// PLACEMENT: Produced by CCS_CampService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Shelter
{
    public readonly struct CCS_CampSnapshot
    {
        public CCS_CampSnapshot(
            CCS_CampTier campTier,
            bool hasShelter,
            bool hasCampfire,
            bool hasBedroll,
            bool ownsCamp,
            string campOwnerId,
            Vector3 campCenter,
            string message)
        {
            CampTier = campTier;
            HasShelter = hasShelter;
            HasCampfire = hasCampfire;
            HasBedroll = hasBedroll;
            OwnsCamp = ownsCamp;
            CampOwnerId = campOwnerId ?? string.Empty;
            CampCenter = campCenter;
            Message = message ?? string.Empty;
        }

        public CCS_CampTier CampTier { get; }

        public bool HasShelter { get; }

        public bool HasCampfire { get; }

        public bool HasBedroll { get; }

        public bool HasCompleteTemporaryCamp => HasShelter && HasCampfire && HasBedroll;

        public bool OwnsCamp { get; }

        public string CampOwnerId { get; }

        public Vector3 CampCenter { get; }

        public string Message { get; }

        public static CCS_CampSnapshot Empty { get; } = new CCS_CampSnapshot(
            CCS_CampTier.None,
            false,
            false,
            false,
            false,
            string.Empty,
            Vector3.zero,
            "No camp.");
    }
}
