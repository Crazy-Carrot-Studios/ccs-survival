using System;
using CCS.Modules.Settlements;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcDialogueStubLine
// CATEGORY: Modules / NPCs / Runtime / Dialogue
// PURPOSE: Profile line with optional role/settlement/business/affiliation/route filters.
// PLACEMENT: Serialized on CCS_NpcDialogueStubDefinition and profile globals.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 — most specific matching filter wins per category.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcDialogueStubLine
    {
        [SerializeField] private CCS_NpcDialogueStubCategory category = CCS_NpcDialogueStubCategory.Unknown;

        [SerializeField] private string lineText = string.Empty;

        [SerializeField] private int roleType = (int)CCS_NpcRoleType.Unknown;

        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private string businessId = string.Empty;

        [SerializeField] private int affiliationType = (int)CCS_NpcAffiliationType.None;

        [SerializeField] private int serviceRoute = (int)CCS_SettlementServiceRouteType.Unknown;

        public CCS_NpcDialogueStubCategory Category => category;

        public string LineText => lineText ?? string.Empty;

        public CCS_NpcRoleType RoleType =>
            Enum.IsDefined(typeof(CCS_NpcRoleType), roleType)
                ? (CCS_NpcRoleType)roleType
                : CCS_NpcRoleType.Unknown;

        public string SettlementId => settlementId ?? string.Empty;

        public string BusinessId => businessId ?? string.Empty;

        public CCS_NpcAffiliationType AffiliationType =>
            Enum.IsDefined(typeof(CCS_NpcAffiliationType), affiliationType)
                ? (CCS_NpcAffiliationType)affiliationType
                : CCS_NpcAffiliationType.None;

        public CCS_SettlementServiceRouteType ServiceRoute =>
            Enum.IsDefined(typeof(CCS_SettlementServiceRouteType), serviceRoute)
                ? (CCS_SettlementServiceRouteType)serviceRoute
                : CCS_SettlementServiceRouteType.Unknown;
    }
}
