// =============================================================================
// SCRIPT: CCS_TimeOfDayPhase
// CATEGORY: Modules / TimeOfDay / Runtime / Data
// PURPOSE: High-level day cycle phases for the global game clock.
// PLACEMENT: Referenced by snapshots, events, and future lighting/weather systems.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Foundation only. No lighting or weather coupling in 0.7.0.
// =============================================================================

namespace CCS.Modules.TimeOfDay
{
    public enum CCS_TimeOfDayPhase
    {
        Dawn = 0,
        Day = 1,
        Dusk = 2,
        Night = 3
    }
}
