using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcActivityProfile
// CATEGORY: Modules / NPCs / Runtime / Activities
// PURPOSE: Schedule block to activity mappings and fallback activity rules.
// PLACEMENT: Assets/CCS/Survival/Profiles/NPCs/Activities/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 — wired on CCS_NpcActivityService.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [CreateAssetMenu(
        fileName = "CCS_NpcActivityProfile",
        menuName = "CCS/Survival/NPCs/NPC Activity Profile")]
    public sealed class CCS_NpcActivityProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_NpcActivityBlockMapping[] blockMappings =
            Array.Empty<CCS_NpcActivityBlockMapping>();

        [SerializeField] private CCS_NpcActivityType scheduleMissingFallbackActivity = CCS_NpcActivityType.Idle;

        [SerializeField] private CCS_NpcActivityType movementMissingFallbackActivity = CCS_NpcActivityType.Idle;

        public CCS_NpcActivityBlockMapping[] BlockMappings => blockMappings ?? Array.Empty<CCS_NpcActivityBlockMapping>();

        public CCS_NpcActivityType ScheduleMissingFallbackActivity => scheduleMissingFallbackActivity;

        public CCS_NpcActivityType MovementMissingFallbackActivity => movementMissingFallbackActivity;

        public bool TryGetActivityForBlock(
            CCS_NpcScheduleBlockType blockType,
            out CCS_NpcActivityType activityType)
        {
            activityType = CCS_NpcActivityType.None;
            CCS_NpcActivityBlockMapping[] mappings = BlockMappings;
            for (int index = 0; index < mappings.Length; index++)
            {
                CCS_NpcActivityBlockMapping mapping = mappings[index];
                if (mapping != null && mapping.ScheduleBlockType == blockType)
                {
                    activityType = mapping.ActivityType;
                    return activityType != CCS_NpcActivityType.None;
                }
            }

            return false;
        }
    }
}
