// =============================================================================
// SCRIPT: CCS_DamageSourceType
// CATEGORY: Modules / Attributes / Runtime / Data
// PURPOSE: Standardized damage source type labels for combat-aware consumers.
// PLACEMENT: Shared damage metadata enum.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.0 AI and revolver combat foundation.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public enum CCS_DamageSourceType
    {
        Unknown = 0,
        RevolverShot = 1,
        AIRevolverShot = 2,
        Environmental = 3,
    }
}
