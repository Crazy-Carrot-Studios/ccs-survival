using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcDialogueStubProfile
// CATEGORY: Modules / NPCs / Runtime / Dialogue
// PURPOSE: Profile-driven dialogue stub lines by role, settlement, business, and route.
// PLACEMENT: Assets/CCS/Survival/Profiles/NPCs/Dialogue/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 — static profile data; no dialogue persistence.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [CreateAssetMenu(
        fileName = "CCS_NpcDialogueStubProfile",
        menuName = "CCS/Survival/NPCs/NPC Dialogue Stub Profile")]
    public sealed class CCS_NpcDialogueStubProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_NpcDialogueStubDefinition[] roleDefinitions =
            Array.Empty<CCS_NpcDialogueStubDefinition>();

        [SerializeField] private CCS_NpcDialogueStubLine[] globalLines = Array.Empty<CCS_NpcDialogueStubLine>();

        [SerializeField] private string genericFallbackLine =
            "Good to see you. Frontier life keeps us all busy.";

        [SerializeField] private bool requireAffiliationForDialogue = true;

        public CCS_NpcDialogueStubDefinition[] RoleDefinitions =>
            roleDefinitions ?? Array.Empty<CCS_NpcDialogueStubDefinition>();

        public CCS_NpcDialogueStubLine[] GlobalLines => globalLines ?? Array.Empty<CCS_NpcDialogueStubLine>();

        public string GenericFallbackLine => genericFallbackLine ?? string.Empty;

        public bool RequireAffiliationForDialogue => requireAffiliationForDialogue;

        public bool TryGetDefinitionForRole(CCS_NpcRoleType roleType, out CCS_NpcDialogueStubDefinition definition)
        {
            definition = null;
            CCS_NpcDialogueStubDefinition[] definitions = RoleDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcDialogueStubDefinition candidate = definitions[index];
                if (candidate != null && candidate.RoleType == roleType)
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
