// =============================================================================
// SCRIPT: CCS_IUpdatable
// CATEGORY: Core / Runtime / Systems
// PURPOSE: Defines update-driven systems for per-frame Tick execution.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No MonoBehaviour inheritance. Interface only.
// =============================================================================

namespace CCS.Core
{
    public interface CCS_IUpdatable
    {
        void Tick(float deltaTime);
    }
}
