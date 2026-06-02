using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CampTierDefinition
// CATEGORY: Modules / Shelter / Runtime / Data
// PURPOSE: Profile-driven camp tier with prerequisite tier and structure requirements.
// PLACEMENT: Serialized on CCS_CampTierProfile assets.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Shelter
{
    [Serializable]
    public sealed class CCS_CampTierDefinition
    {
        [SerializeField] private CCS_CampTier campTier = CCS_CampTier.None;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private CCS_CampTier prerequisiteTier = CCS_CampTier.None;
        [SerializeField] private CCS_CampRequirement[] requirements = Array.Empty<CCS_CampRequirement>();

        public CCS_CampTier CampTier => campTier;

        public string DisplayName => displayName ?? string.Empty;

        public CCS_CampTier PrerequisiteTier => prerequisiteTier;

        public CCS_CampRequirement[] Requirements => requirements ?? Array.Empty<CCS_CampRequirement>();
    }
}
