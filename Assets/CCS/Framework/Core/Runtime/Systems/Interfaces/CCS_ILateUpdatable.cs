// =============================================================================
// SCRIPT: CCS_ILateUpdatable
// CATEGORY: Core / Runtime / Systems
// PURPOSE: Defines late-update contracts for systems that run after standard updates.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No MonoBehaviour inheritance. Interface only.
// =============================================================================

namespace CCS.Core
{
    public interface CCS_ILateUpdatable
    {
        void LateTick(float deltaTime);
    }
}
