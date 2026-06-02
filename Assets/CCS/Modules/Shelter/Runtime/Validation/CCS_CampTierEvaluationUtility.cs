using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_CampTierEvaluationUtility
// CATEGORY: Modules / Shelter / Runtime / Validation
// PURPOSE: Evaluates highest camp tier from profile requirements and structure presence.
// PLACEMENT: Used by CCS_CampService during RecalculateCamp.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Shelter
{
    public static class CCS_CampTierEvaluationUtility
    {
        public static CCS_CampTier EvaluateHighestTier(
            CCS_CampTierProfile profile,
            IReadOnlyDictionary<CCS_CampStructureKind, bool> structurePresence)
        {
            if (profile == null || structurePresence == null)
            {
                return CCS_CampTier.None;
            }

            CCS_CampTier bestTier = CCS_CampTier.None;
            CCS_CampTierDefinition[] ordered = profile.GetTiersOrderedAscending();
            for (int index = 0; index < ordered.Length; index++)
            {
                CCS_CampTierDefinition tierDefinition = ordered[index];
                if (tierDefinition == null || tierDefinition.CampTier == CCS_CampTier.None)
                {
                    continue;
                }

                if (!MeetsTierDefinition(tierDefinition, structurePresence, profile))
                {
                    continue;
                }

                bestTier = tierDefinition.CampTier;
            }

            return bestTier;
        }

        private static bool MeetsTierDefinition(
            CCS_CampTierDefinition tierDefinition,
            IReadOnlyDictionary<CCS_CampStructureKind, bool> structurePresence,
            CCS_CampTierProfile profile)
        {
            if (tierDefinition.PrerequisiteTier != CCS_CampTier.None
                && profile.TryGetTierDefinition(tierDefinition.PrerequisiteTier, out CCS_CampTierDefinition prerequisite)
                && !MeetsTierDefinition(prerequisite, structurePresence, profile))
            {
                return false;
            }

            CCS_CampRequirement[] requirements = tierDefinition.Requirements;
            if (requirements == null || requirements.Length == 0)
            {
                return true;
            }

            for (int index = 0; index < requirements.Length; index++)
            {
                CCS_CampRequirement requirement = requirements[index];
                if (requirement == null || requirement.StructureKind == CCS_CampStructureKind.None)
                {
                    continue;
                }

                if (!structurePresence.TryGetValue(requirement.StructureKind, out bool isPresent) || !isPresent)
                {
                    return false;
                }
            }

            return true;
        }

        public static Dictionary<CCS_CampStructureKind, bool> CreatePresenceMap()
        {
            return new Dictionary<CCS_CampStructureKind, bool>
            {
                { CCS_CampStructureKind.Shelter, false },
                { CCS_CampStructureKind.Bedroll, false },
                { CCS_CampStructureKind.Campfire, false },
                { CCS_CampStructureKind.Storage, false },
                { CCS_CampStructureKind.WorkArea, false },
                { CCS_CampStructureKind.Barn, false },
                { CCS_CampStructureKind.Stable, false },
                { CCS_CampStructureKind.Garden, false },
                { CCS_CampStructureKind.Livestock, false },
                { CCS_CampStructureKind.SawTable, false },
                { CCS_CampStructureKind.CharcoalKiln, false },
                { CCS_CampStructureKind.PrimitiveForge, false }
            };
        }
    }
}
