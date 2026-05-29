using System;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreEvents
// CATEGORY: Survival / Runtime / SurvivalCore / Events
// PURPOSE: Event name constants and delegate types for survival core service notifications.
// PLACEMENT: Subscribed by future UI, gameplay, and diagnostics bridges.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Instance events live on CCS_SurvivalCoreService; this type documents contracts.
// =============================================================================

namespace CCS.Survival.SurvivalCore
{
    public static class CCS_SurvivalCoreEvents
    {
        public const string StatChangedEventName = "SurvivalCore.StatChanged";
        public const string StatDepletedEventName = "SurvivalCore.StatDepleted";
        public const string StatRestoredEventName = "SurvivalCore.StatRestored";
        public const string SurvivalCoreInitializedEventName = "SurvivalCore.Initialized";
    }

    public delegate void SurvivalStatChangedHandler(CCS_SurvivalStatChangedEventArgs eventArgs);

    public delegate void SurvivalStatDepletedHandler(CCS_SurvivalStatType statType, CCS_SurvivalStatSnapshot snapshot);

    public delegate void SurvivalStatRestoredHandler(CCS_SurvivalStatType statType, CCS_SurvivalStatSnapshot snapshot);

    public delegate void SurvivalCoreInitializedHandler(CCS_SurvivalCoreProfile profile);
}
