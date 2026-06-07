using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcScheduleBlock
// CATEGORY: Modules / NPCs / Runtime / Schedules
// PURPOSE: One daily time window and activity block within a schedule definition.
// PLACEMENT: Serialized on CCS_NpcScheduleDefinition.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 — supports overnight windows (startHour > endHour).
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcScheduleBlock
    {
        [SerializeField] private int startHour;

        [SerializeField] private int endHour;

        [SerializeField] private CCS_NpcScheduleBlockType blockType = CCS_NpcScheduleBlockType.Unknown;

        public int StartHour => startHour;

        public int EndHour => endHour;

        public CCS_NpcScheduleBlockType BlockType => blockType;
    }
}
