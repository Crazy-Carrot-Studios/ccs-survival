using System;

// =============================================================================
// SCRIPT: CCS_FirearmSnapshot
// CATEGORY: Modules / Firearms / Runtime / Data
// PURPOSE: Serializable loaded-round state for owned frontier firearms.
// PLACEMENT: Captured by CCS_FirearmService and persisted through CCS_SaveService.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Firearms
{
    [Serializable]
    public sealed class CCS_FirearmSnapshot
    {
        public CCS_FirearmStateEntry[] firearmStates = Array.Empty<CCS_FirearmStateEntry>();

        public static CCS_FirearmSnapshot Empty => new CCS_FirearmSnapshot();
    }

    [Serializable]
    public sealed class CCS_FirearmStateEntry
    {
        public string firearmItemId = string.Empty;
        public int loadedRounds;
        public string activeEquippedItemId = string.Empty;
    }
}
