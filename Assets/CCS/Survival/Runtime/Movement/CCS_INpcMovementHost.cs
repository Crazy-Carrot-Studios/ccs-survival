using UnityEngine;

// =============================================================================
// SCRIPT: CCS_INpcMovementHost
// CATEGORY: Survival / Runtime / Movement
// PURPOSE: Contract for placeholder actors driven by NPC movement service.
// PLACEMENT: Implemented by CCS_PopulationPlaceholderActor; consumed by NPC module bridge.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.5.0 — transform movement only; no NavMesh or physics locomotion.
// =============================================================================

namespace CCS.Survival
{
    public interface CCS_INpcMovementHost
    {
        string NpcIdentityId { get; }

        string SettlementId { get; }

        string WorkforceAnchorId { get; }

        string BusinessId { get; }

        string HomeHousingId { get; }

        bool IsServiceRepresentative { get; }

        int WorkforceCategoryValue { get; }

        bool HasIdentity { get; }

        Transform MovementTransform { get; }
    }
}
