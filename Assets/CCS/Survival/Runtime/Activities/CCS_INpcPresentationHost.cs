using System;

// =============================================================================
// SCRIPT: CCS_INpcPresentationHost
// CATEGORY: Survival / Runtime / Activities
// PURPOSE: Contract for refreshing dev-readable placeholder labels and indicators.
// PLACEMENT: Implemented by CCS_PopulationPlaceholderActor.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 — avoids Settlements -> NPCs assembly reference cycle.
// =============================================================================

namespace CCS.Survival
{
    public interface CCS_INpcPresentationHost
    {
        void RefreshPresentation();
    }
}
