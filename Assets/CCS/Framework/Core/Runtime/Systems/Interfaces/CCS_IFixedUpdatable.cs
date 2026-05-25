// =============================================================================
// SCRIPT: CCS_IFixedUpdatable
// CATEGORY: Core / Runtime / Systems
// PURPOSE: Defines fixed-timestep update contracts for physics-aligned systems.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No MonoBehaviour inheritance. Interface only.
// =============================================================================

namespace CCS.Core
{
    public interface CCS_IFixedUpdatable
    {
        void FixedTick(float fixedDeltaTime);
    }
}
