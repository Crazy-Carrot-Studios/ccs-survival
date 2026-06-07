using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcActivityBlockMapping
// CATEGORY: Modules / NPCs / Runtime / Activities
// PURPOSE: Maps schedule block types to visible NPC activity types.
// PLACEMENT: Serialized on CCS_NpcActivityProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 — traveling override comes from movement status.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcActivityBlockMapping
    {
        [SerializeField] private CCS_NpcScheduleBlockType scheduleBlockType = CCS_NpcScheduleBlockType.Unknown;

        [SerializeField] private CCS_NpcActivityType activityType = CCS_NpcActivityType.None;

        public CCS_NpcScheduleBlockType ScheduleBlockType => scheduleBlockType;

        public CCS_NpcActivityType ActivityType => activityType;
    }
}
