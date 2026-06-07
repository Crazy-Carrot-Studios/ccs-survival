using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcScheduleDefinition
// CATEGORY: Modules / NPCs / Runtime / Schedules
// PURPOSE: Named daily schedule composed of ordered time blocks.
// PLACEMENT: Serialized on CCS_NpcScheduleProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 — profile-driven placeholder routines.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcScheduleDefinition
    {
        [SerializeField] private string scheduleId = string.Empty;

        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private CCS_NpcScheduleBlock[] blocks = Array.Empty<CCS_NpcScheduleBlock>();

        public string ScheduleId => scheduleId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public CCS_NpcScheduleBlock[] Blocks => blocks ?? Array.Empty<CCS_NpcScheduleBlock>();
    }
}
