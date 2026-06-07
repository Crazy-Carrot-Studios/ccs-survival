using System;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_NpcDialogueStubRuntimeBridge
// CATEGORY: Modules / NPCs / Runtime / Dialogue
// PURPOSE: Runtime bridge for dialogue stub resolution, display, and playtest hooks.
// PLACEMENT: Wired by CCS_NpcDialogueStubService and interactables.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 — last result cached for playtest verification.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcDialogueStubRuntimeBridge
    {
        public static Func<CCS_NpcDialogueStubRequest, CCS_NpcDialogueStubResult> ResolveDialogue;

        public static Func<CCS_INpcMovementHost, CCS_NpcDialogueStubResult> ResolveDialogueForHost;

        public static Func<CCS_INpcMovementHost, bool> ResolveAndDisplayForHost;

        public static Action RefreshDialogueHosts;

        public static CCS_NpcDialogueStubResult LastDialogueResult { get; set; } = CCS_NpcDialogueStubResult.Empty;

        public static bool TryResolveDialogue(
            CCS_NpcDialogueStubRequest request,
            out CCS_NpcDialogueStubResult result)
        {
            result = ResolveDialogue?.Invoke(request) ?? CCS_NpcDialogueStubResult.Empty;
            return result.IsSuccess;
        }

        public static bool TryResolveForHost(CCS_INpcMovementHost host, out CCS_NpcDialogueStubResult result)
        {
            result = ResolveDialogueForHost?.Invoke(host) ?? CCS_NpcDialogueStubResult.Empty;
            return result.IsSuccess;
        }

        public static bool TryGetFirstHostDialogueResult(
            string settlementId,
            out CCS_INpcMovementHost host,
            out CCS_NpcDialogueStubResult result)
        {
            host = null;
            result = CCS_NpcDialogueStubResult.Empty;
            bool found = false;
            CCS_INpcMovementHost resolvedHost = null;
            CCS_NpcDialogueStubResult resolvedResult = CCS_NpcDialogueStubResult.Empty;
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(candidate =>
            {
                if (found
                    || candidate == null
                    || !candidate.HasIdentity
                    || !string.Equals(candidate.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                resolvedHost = candidate;
                TryResolveForHost(candidate, out resolvedResult);
                found = true;
            });

            host = resolvedHost;
            result = resolvedResult;
            return found;
        }

        public static void RefreshAllDialogueHosts()
        {
            RefreshDialogueHosts?.Invoke();
        }
    }
}
