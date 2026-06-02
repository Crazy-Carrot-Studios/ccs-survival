using System;
using System.Collections.Generic;
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
            bool hasStorage,
            bool hasWorkArea,
            bool ownsCamp,
            string campOwnerId,
            long campCreationTimeUtcTicks,
            Vector3 campCenter,
            IReadOnlyList<string> structuresPresent,
            string message)
        {
            CampTier = campTier;
            HasShelter = hasShelter;
            HasCampfire = hasCampfire;
            HasBedroll = hasBedroll;
            HasStorage = hasStorage;
            HasWorkArea = hasWorkArea;
            OwnsCamp = ownsCamp;
            CampOwnerId = campOwnerId ?? string.Empty;
            CampCreationTimeUtcTicks = campCreationTimeUtcTicks;
            CampCenter = campCenter;
            StructuresPresent = structuresPresent ?? Array.Empty<string>();
            Message = message ?? string.Empty;
        }

        public CCS_CampTier CampTier { get; }

        public bool HasShelter { get; }

        public bool HasCampfire { get; }

        public bool HasBedroll { get; }

        public bool HasStorage { get; }

        public bool HasWorkArea { get; }

        public bool HasCompleteTemporaryCamp => HasShelter && HasCampfire && HasBedroll;

        public bool HasCompleteFrontierCamp => HasCompleteTemporaryCamp && HasStorage;

        public bool HasCompleteFrontierHomestead => HasCompleteFrontierCamp && HasWorkArea;

        public bool OwnsCamp { get; }

        public string CampOwnerId { get; }

        public long CampCreationTimeUtcTicks { get; }

        public Vector3 CampCenter { get; }

        public IReadOnlyList<string> StructuresPresent { get; }

        public string Message { get; }

        public static CCS_CampSnapshot Empty { get; } = new CCS_CampSnapshot(
            CCS_CampTier.None,
            false,
            false,
            false,
            false,
            false,
            false,
            string.Empty,
            0L,
            Vector3.zero,
            Array.Empty<string>(),
            "No camp.");
    }
}
