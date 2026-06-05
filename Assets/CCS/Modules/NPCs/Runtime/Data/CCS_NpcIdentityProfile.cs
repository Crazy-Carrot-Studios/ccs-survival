using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcIdentityProfile
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Name pools, role display names, and workforce/business role mappings.
// PLACEMENT: Assets/CCS/Survival/Profiles/NPCs/Identity/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 — wired on CCS_NpcIdentityService.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [CreateAssetMenu(
        fileName = "CCS_NpcIdentityProfile",
        menuName = "CCS/Survival/NPCs/NPC Identity Profile")]
    public sealed class CCS_NpcIdentityProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private string[] defaultFirstNamePool = Array.Empty<string>();

        [SerializeField] private string[] defaultLastNamePool = Array.Empty<string>();

        [SerializeField] private CCS_NpcIdentityDefinition[] settlementDefinitions =
            Array.Empty<CCS_NpcIdentityDefinition>();

        [SerializeField] private CCS_NpcRoleAssignment[] roleAssignments = Array.Empty<CCS_NpcRoleAssignment>();

        [SerializeField] private CCS_NpcRoleDisplayEntry[] roleDisplayNames = Array.Empty<CCS_NpcRoleDisplayEntry>();

        public string[] DefaultFirstNamePool => defaultFirstNamePool ?? Array.Empty<string>();

        public string[] DefaultLastNamePool => defaultLastNamePool ?? Array.Empty<string>();

        public CCS_NpcIdentityDefinition[] SettlementDefinitions =>
            settlementDefinitions ?? Array.Empty<CCS_NpcIdentityDefinition>();

        public CCS_NpcRoleAssignment[] RoleAssignments => roleAssignments ?? Array.Empty<CCS_NpcRoleAssignment>();

        public CCS_NpcRoleDisplayEntry[] RoleDisplayNames => roleDisplayNames ?? Array.Empty<CCS_NpcRoleDisplayEntry>();

        public bool TryGetSettlementDefinition(string settlementId, out CCS_NpcIdentityDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            CCS_NpcIdentityDefinition[] definitions = SettlementDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcIdentityDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public sealed class CCS_NpcRoleDisplayEntry
    {
        public CCS_NpcRoleType roleType = CCS_NpcRoleType.Unknown;

        public string displayName = string.Empty;
    }
}
