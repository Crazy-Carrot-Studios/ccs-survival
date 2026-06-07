using System;

// =============================================================================
// SCRIPT: CCS_NpcDialogueStubResult
// CATEGORY: Modules / NPCs / Runtime / Dialogue
// PURPOSE: Resolved dialogue stub lines and metadata for debug HUD display.
// PLACEMENT: Returned by CCS_NpcDialogueStubService and runtime bridge.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 — no branching or player choices.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcDialogueStubResult
    {
        public static readonly CCS_NpcDialogueStubResult Empty = new CCS_NpcDialogueStubResult();

        public CCS_NpcDialogueStubResultType ResultType { get; set; } = CCS_NpcDialogueStubResultType.Failed;

        public string Message { get; set; } = string.Empty;

        public string NpcIdentityId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string RoleDisplayName { get; set; } = string.Empty;

        public string SettlementDisplayName { get; set; } = string.Empty;

        public string BusinessDisplayName { get; set; } = string.Empty;

        public string GreetingLine { get; set; } = string.Empty;

        public string RoleIntroductionLine { get; set; } = string.Empty;

        public string SettlementIntroductionLine { get; set; } = string.Empty;

        public string BusinessIntroductionLine { get; set; } = string.Empty;

        public string ServiceHintLine { get; set; } = string.Empty;

        public string GenericFallbackLine { get; set; } = string.Empty;

        public string[] DisplayLines { get; set; } = Array.Empty<string>();

        public bool IsSuccess => ResultType == CCS_NpcDialogueStubResultType.Success;

        public bool HasGreeting => !string.IsNullOrWhiteSpace(GreetingLine);

        public bool HasRoleIntroduction => !string.IsNullOrWhiteSpace(RoleIntroductionLine);

        public bool HasServiceHint => !string.IsNullOrWhiteSpace(ServiceHintLine);
    }
}
