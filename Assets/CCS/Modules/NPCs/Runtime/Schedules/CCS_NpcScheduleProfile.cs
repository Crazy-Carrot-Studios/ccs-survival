using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcScheduleProfile
// CATEGORY: Modules / NPCs / Runtime / Schedules
// PURPOSE: Schedule definitions and role-to-schedule mappings for placeholder NPCs.
// PLACEMENT: Assets/CCS/Survival/Profiles/NPCs/Schedules/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 — wired on CCS_NpcScheduleService.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [CreateAssetMenu(
        fileName = "CCS_NpcScheduleProfile",
        menuName = "CCS/Survival/NPCs/NPC Schedule Profile")]
    public sealed class CCS_NpcScheduleProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_NpcScheduleDefinition[] scheduleDefinitions =
            Array.Empty<CCS_NpcScheduleDefinition>();

        [SerializeField] private CCS_NpcScheduleRoleMapping[] roleMappings =
            Array.Empty<CCS_NpcScheduleRoleMapping>();

        [SerializeField] private string fallbackScheduleId = string.Empty;

        [SerializeField] private CCS_NpcScheduleBlockType gapFallbackBlockType = CCS_NpcScheduleBlockType.Idle;

        public CCS_NpcScheduleDefinition[] ScheduleDefinitions =>
            scheduleDefinitions ?? Array.Empty<CCS_NpcScheduleDefinition>();

        public CCS_NpcScheduleRoleMapping[] RoleMappings => roleMappings ?? Array.Empty<CCS_NpcScheduleRoleMapping>();

        public string FallbackScheduleId => fallbackScheduleId ?? string.Empty;

        public CCS_NpcScheduleBlockType GapFallbackBlockType => gapFallbackBlockType;

        public bool TryGetDefinition(string scheduleId, out CCS_NpcScheduleDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(scheduleId))
            {
                return false;
            }

            CCS_NpcScheduleDefinition[] definitions = ScheduleDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcScheduleDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.ScheduleId, scheduleId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
