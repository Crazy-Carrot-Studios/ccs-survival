using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcDialogueStubDefinition
// CATEGORY: Modules / NPCs / Runtime / Dialogue
// PURPOSE: Role-scoped bundle of dialogue stub lines for profile resolution.
// PLACEMENT: Serialized on CCS_NpcDialogueStubProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 — one definition per active workforce/service role.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcDialogueStubDefinition
    {
        [SerializeField] private int roleType = (int)CCS_NpcRoleType.Unknown;

        [SerializeField] private CCS_NpcDialogueStubLine[] lines = Array.Empty<CCS_NpcDialogueStubLine>();

        public CCS_NpcRoleType RoleType =>
            Enum.IsDefined(typeof(CCS_NpcRoleType), roleType)
                ? (CCS_NpcRoleType)roleType
                : CCS_NpcRoleType.Unknown;

        public CCS_NpcDialogueStubLine[] Lines => lines ?? Array.Empty<CCS_NpcDialogueStubLine>();
    }
}
