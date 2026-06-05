using System;
using CCS.Modules.Settlements;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcIdentityDefinition
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Per-settlement name pools and optional business id for identity generation.
// PLACEMENT: Serialized on CCS_NpcIdentityProfile settlement definitions.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 — profile-driven stable name generation per placeholder slot.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcIdentityDefinition
    {
        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private string[] firstNamePool = Array.Empty<string>();

        [SerializeField] private string[] lastNamePool = Array.Empty<string>();

        public string SettlementId => settlementId ?? string.Empty;

        public string[] FirstNamePool => firstNamePool ?? Array.Empty<string>();

        public string[] LastNamePool => lastNamePool ?? Array.Empty<string>();
    }
}
