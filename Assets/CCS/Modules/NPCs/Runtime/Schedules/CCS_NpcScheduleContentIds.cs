// =============================================================================
// SCRIPT: CCS_NpcScheduleContentIds
// CATEGORY: Modules / NPCs / Runtime / Schedules
// PURPOSE: Stable ids for NPC schedule bootstrap, validation, and playtest.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 NPC schedule state foundation.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcScheduleContentIds
    {
        public const string ScheduleProfilesRoot = "Assets/CCS/Survival/Profiles/NPCs/Schedules";

        public const string DefaultScheduleProfilePath =
            ScheduleProfilesRoot + "/CCS_DefaultNpcScheduleProfile.asset";

        public const string DefaultScheduleProfileId = "ccs.survival.profile.npcschedule.default";

        public const string WorkerScheduleId = "ccs.survival.schedule.worker";

        public const string ServiceRepresentativeScheduleId = "ccs.survival.schedule.servicerepresentative";
    }
}
