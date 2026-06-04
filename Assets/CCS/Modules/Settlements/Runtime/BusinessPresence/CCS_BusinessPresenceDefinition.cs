using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BusinessPresenceDefinition
// CATEGORY: Modules / Settlements / Runtime / BusinessPresence
// PURPOSE: Maps a business to a bootstrap presence anchor and display metadata.
// PLACEMENT: Serialized on CCS_BusinessPresenceProfile.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — visible business presence foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_BusinessPresenceDefinition
    {
        public string anchorId = string.Empty;

        public string settlementId = string.Empty;

        public string businessId = string.Empty;

        public CCS_BusinessType businessType = CCS_BusinessType.Unknown;

        public string displayName = string.Empty;

        public string AnchorId => anchorId ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        public string BusinessId => businessId ?? string.Empty;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName) ? businessType.ToString() : displayName;
    }
}
