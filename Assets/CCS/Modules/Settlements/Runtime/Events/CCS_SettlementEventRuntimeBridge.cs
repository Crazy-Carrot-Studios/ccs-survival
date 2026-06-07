using System;

// =============================================================================
// SCRIPT: CCS_SettlementEventRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: Runtime bridge for event snapshots, modifiers, and presentation hooks.
// PLACEMENT: Wired by CCS_SettlementEventService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 — used by playtest, dialogue, social, and world simulation hooks.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementEventRuntimeBridge
    {
        public static Func<string, CCS_SettlementEventSnapshot> ResolveActiveEvent;

        public static Func<string, bool> TryForceEventForPlaytest;

        public static Func<string, CCS_SettlementEventType, bool> TryForceEventTypeForPlaytest;

        public static Action RefreshAllEventPresentation;

        public static Action<string, CCS_SettlementEventSnapshot> NotifyEventActivated;

        public static Func<string, float> ResolveProsperityBonus;

        public static Func<string, float> ResolveSupplyBonus;

        public static Func<string, float> ResolveContractRewardMultiplier;

        public static Func<string, float> ResolveReputationGainMultiplier;

        public static Func<string, string> ResolveDialogueAppendLine;

        public static Func<string, string> ResolvePreferredSocialAnchorId;

        public static CCS_SettlementEventSnapshot LastEventSnapshot { get; set; } = CCS_SettlementEventSnapshot.Empty;

        public static bool TryGetActiveEvent(string settlementId, out CCS_SettlementEventSnapshot snapshot)
        {
            snapshot = ResolveActiveEvent?.Invoke(settlementId) ?? CCS_SettlementEventSnapshot.Empty;
            return snapshot != null && snapshot.IsValid;
        }

        public static void RefreshAllEvents()
        {
            RefreshAllEventPresentation?.Invoke();
        }
    }
}
